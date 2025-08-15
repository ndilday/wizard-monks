using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Activities;
using WizardMonks.Core;
using WizardMonks.Decisions;
using WizardMonks.Decisions.Goals;
using WizardMonks.Instances;
using WizardMonks.Models.Beliefs;
using WizardMonks.Models.Books;
using WizardMonks.Services.Characters;

namespace WizardMonks.Models.Characters
{
    public class AgingEventArgs : EventArgs
    {
        public bool IsCrisis { get; private set; }
        public bool IsApparent { get; private set; }
        public string AbilityName { get; private set; }
        public sbyte PointsLost { get; private set; }
        public Character Character { get; private set; }
        public bool Died { get; private set; }

        public AgingEventArgs(Character character, bool crisis, bool apparent, bool death, string ability, sbyte lost)
        {
            Character = character;
            IsCrisis = crisis;
            IsApparent = apparent;
            Died = death;
            AbilityName = ability;
            PointsLost = lost;
        }

        public AgingEventArgs(Character character, bool crisis, bool apparent, bool death) : this(character, crisis, apparent, death, "", 0){}
    }

    public delegate void AgedEventHandler(object sender, AgingEventArgs e);

	[Serializable]
	public partial class Character : IBeliefSubject
	{
        #region Attributes
        protected Attribute[] _attributes = new Attribute[Enum.GetNames(typeof(AttributeType)).Length];
        public Attribute GetAttribute(AttributeType attributeType)
        {
            return _attributes[(short)attributeType];
        }

        public double GetAttributeValue(AttributeType attributeType)
        {
            double value = _attributes[(short)attributeType].Value;
            if(SeasonalAge < 60)
            {
                return value - (60 - SeasonalAge) / 8.0;
            }
            else
            {
                return value;
            }
        }
        #endregion

        #region Private Fields
        private uint _baseAge;
        protected Ability _writingAbility;
        protected Ability _areaAbility;

        private readonly string[] _virtueList = new string[10];
		private readonly string[] _flawList = new string[10];

        private readonly Dictionary<int, CharacterAbilityBase> _abilityMap;
        protected readonly List<IActivity> _seasonList;
        #endregion

        #region Events
        public event AgedEventHandler Aged;
        #endregion

        #region Public Properties
        public Guid Id { get; private set; } = Guid.NewGuid();
        public uint NoAgingSeasons { get; set; }
        public Personality Personality { get; private set; }
        public ushort LongevityRitual { get; set; }
        public byte Decrepitude { get; set; }
        public CharacterAbility Warping { get; private set; }
        public List<IGoal> ActiveGoals { get; private set; }
        public List<IGoal> CompletedGoals { get; private set; }
        public Desires Desires { get; set; }
        public IList<Aura> KnownAuras { get; private set; }
        public string Name { get; set; }
        public Ability WritingLanguage { get; private set; }
        public CharacterAbilityBase WritingCharacterAbility { get; private set; }
        public CharacterAbilityBase WritingLanguageCharacterAbility { get; private set; }
        public List<Ability> WritingAbilities { get; private set; }
        public List<ABook> BooksWritten { get; private set; }
        public HashSet<ABook> BooksRead { get; private set; }
        public List<ABook> Books { get; private set; }
        public List<Summa> IncompleteBooks { get; private set; }
        public Dictionary<IBeliefSubject, BeliefProfile> Beliefs { get; private set; }
        public Dictionary<string, double> ReputationFocuses { get; private set; }
        public Season CurrentSeason { get; set; }
        public List<string> Log { get; private set; }
        public ABook BestBookCache { get; set; }
        public bool IsBestBookCacheClean { get; set; }
        public HashSet<CharacterAbilityBase> WritableTopicsCache { get; private set; }
        public bool IsWritableTopicsCacheClean { get; set; }
        public bool IsCollaborating { get; set; }
        public bool WantsToFollow { get; set; }
        public IActivity MandatoryAction { get; set; }

        public IEnumerable<ABook> ReadableBooks
        {
            get
            {
                return Books.Where(b => b.Author != this && 
                    (!BooksRead.Contains(b) || (b.Level != 1000 && b.Level > GetAbility(b.Topic).Value)));
            }
        }
        #endregion

        public Character(Ability writingLanguage, Ability writingAbility, Ability areaAbility, uint baseSeasonableAge = 20, Personality personality = null, Dictionary<string, double> reputationFocuses = null)
        {
            Die die = new();
            _attributes[(short)AttributeType.Strength] = new Attribute(die.RollNormal());
            _attributes[(short)AttributeType.Stamina] = new Attribute(die.RollNormal());
            _attributes[(short)AttributeType.Dexterity] = new Attribute(die.RollNormal());
            _attributes[(short)AttributeType.Quickness] = new Attribute(die.RollNormal());
            _attributes[(short)AttributeType.Intelligence] = new Attribute(die.RollNormal());
            _attributes[(short)AttributeType.Communication] = new Attribute(die.RollNormal());
            _attributes[(short)AttributeType.Perception] = new Attribute(die.RollNormal());
            _attributes[(short)AttributeType.Presence] = new Attribute(die.RollNormal());

            Decrepitude = 0;
            CurrentSeason = Season.Spring;
            KnownAuras = new List<Aura>();
            Beliefs = [];
            IsCollaborating = false;
            WantsToFollow = true;

            NoAgingSeasons = 0;
            _baseAge = baseSeasonableAge;
            MandatoryAction = null;

            _abilityMap = new Dictionary<int, CharacterAbilityBase>();
            _seasonList = new List<IActivity>();
            BooksRead = new HashSet<ABook>();
            BooksWritten = new List<ABook>();
            Books = new List<ABook>();
            IsWritableTopicsCacheClean = false;

            _areaAbility = areaAbility;
            _writingAbility = writingAbility;
            WritingLanguage = writingLanguage;
            WritingCharacterAbility = GetAbility(writingAbility);
            WritingLanguageCharacterAbility = GetAbility(writingLanguage);
            WritingAbilities = [_writingAbility, WritingLanguage];

            IncompleteBooks = new List<Summa>();
            ActiveGoals = new List<IGoal>();
            Log = new List<string>();
            Warping = new CharacterAbility(Abilities.Warping);
            Personality = personality ?? new Personality();
            if(reputationFocuses != null)
            {
                ReputationFocuses = reputationFocuses;
            }
            else
            {
                ReputationFocuses = [];
            }
        }

        public void OnAged(AgingEventArgs e)
        {
            if (Aged != null)
            {
                Aged(this, e);
            }
        }

        public uint SeasonalAge
        {
            get { return (uint)(_seasonList.Count + _baseAge); }
        }

        public uint ApparentAge
        {
            get { return SeasonalAge - NoAgingSeasons; }
        }

        public void AddSeasonActivity(IActivity activity)
        {
            _seasonList.Add(activity);
        }

        public int GetSeasonActivityLength()
        {
            return _seasonList.Count;
        }

        public BeliefProfile GetBeliefProfile(IBeliefSubject subject)
        {
            if (!Beliefs.TryGetValue(subject, out var profile))
            {
                profile = new BeliefProfile();
                Beliefs[subject] = profile;
            }
            return profile;
        }

        #region Ability Functions
        public virtual CharacterAbilityBase GetAbility(Ability ability)
        {
            if (!_abilityMap.ContainsKey(ability.AbilityId))
            {
                _abilityMap[ability.AbilityId] = new CharacterAbility(ability);
                _abilityMap[ability.AbilityId].Changed += (s, a) => this.InvalidateWritableTopicsCache();
            }
            
            return _abilityMap[ability.AbilityId];
        }

        public virtual IEnumerable<CharacterAbilityBase> GetAbilities()
        {
            return _abilityMap.Values;
        }
        
        protected virtual void AddAbility(Ability ability)
        {
            if (ability.AbilityType == AbilityType.Art)
            {
                _abilityMap.Add(ability.AbilityId, new AcceleratedAbility(ability));
            }
            else
            {
                _abilityMap.Add(ability.AbilityId, new CharacterAbility(ability));
            }
        }
        #endregion

        #region Book Functions
        public virtual void AddBookToCollection(ABook book)
        {
            Books.Add(book);
        }

        public virtual void RemoveBookFromCollection(ABook book)
        {
            Books.Remove(book);
        }
        #endregion

        #region Preference/Goal Functions

        public void AddGoal(IGoal goal)
        {
            ActiveGoals.Add(goal);
        }
        #endregion

        #region object Overrides
        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            if (obj is not Character other) return false;
            return this.Name == other.Name;
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }
        #endregion
    }
}
