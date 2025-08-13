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
        protected List<IGoal> _goals;
        protected Desires _desires;
        protected List<string> _verboseLog;

        private readonly string[] _virtueList = new string[10];
		private readonly string[] _flawList = new string[10];

        private readonly Dictionary<int, CharacterAbilityBase> _abilityMap;
        protected readonly List<IActivity> _seasonList;
        public HashSet<CharacterAbilityBase> WritableTopicsCache { get; private set; }
        public bool IsWritableTopicsCacheClean { get; set; }

        protected IActivity _mandatoryAction;

        
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
        public Dictionary<Guid, BeliefProfile> Beliefs { get; private set; }
        public Dictionary<string, double> ReputationFocuses { get; private set; }
        public Season CurrentSeason { get; private set; }
        public List<string> Log { get; private set; }
        public ABook BestBookCache { get; set; }
        public bool IsBestBookCacheClean { get; set; }

        public IEnumerable<ABook> ReadableBooks
        {
            get
            {
                return Books.Where(b => b.Author != this && 
                    (!BooksRead.Contains(b) || (b.Level != 1000 && b.Level > GetAbility(b.Topic).Value)));
            }
        }
        public bool IsCollaborating { get; private set; }
        public bool WantsToFollow { get; protected set; }
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
            _mandatoryAction = null;

            _abilityMap = new Dictionary<int, CharacterAbilityBase>();
            _seasonList = new List<IActivity>();
            BooksRead = new HashSet<ABook>();
            BooksWritten = new List<ABook>();
            Books = new List<ABook>();
            _verboseLog = new List<string>();
            IsWritableTopicsCacheClean = false;

            _areaAbility = areaAbility;
            _writingAbility = writingAbility;
            WritingLanguage = writingLanguage;
            WritingCharacterAbility = GetAbility(writingAbility);
            WritingLanguageCharacterAbility = GetAbility(writingLanguage);
            WritingAbilities = [_writingAbility, WritingLanguage];

            IncompleteBooks = new List<Summa>();
            _goals = new List<IGoal>();
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

        #region Seasonal Functions

        IActivity DecideSeasonalActivity()
        {
            _desires = new Desires();
            if (IsCollaborating)
            {
                return _mandatoryAction;
            }
            else
            {
                ConsideredActions actions = new();
                _verboseLog.Add("----------");
                foreach (IGoal goal in _goals)
                {
                    if (!goal.IsComplete())
                    {
                        //List<string> dummy = new List<string>();
                        goal.AddActionPreferencesToList(actions, _desires, _verboseLog);
                    }
                }
                Log.AddRange(actions.Log());
                return actions.GetBestAction();
            }
        }

       public virtual void ReprioritizeGoals()
       {
           foreach (IGoal goal in _goals.ToList())
           {
               if (!goal.IsComplete())
               {
                   if (goal.AgeToCompleteBy < SeasonalAge)
                   {
                       Log.Add("Failed to achieve a goal");
                       _goals.Remove(goal);
                   }
               }
           }
       }

        public virtual void CommitAction(IActivity action)
        {
            _seasonList.Add(action);
            action.Act(this);
            if (SeasonalAge >= 140)
            {
                this.Age(LongevityRitual);
            }
        }

        public virtual IActivity Advance()
        {
            IActivity activity = null;
            if (!IsCollaborating)
            {
                Log.Add("");
                Log.Add(CurrentSeason.ToString() + " " + _seasonList.Count() / 4);
                activity = DecideSeasonalActivity();
                _seasonList.Add(activity);
                activity.Act(this);
                if (SeasonalAge >= 140)
                {
                    this.Age(LongevityRitual);
                }
                switch (CurrentSeason)
                {
                    case Season.Spring:
                        CurrentSeason = Season.Summer;
                        break;
                    case Season.Summer:
                        CurrentSeason = Season.Autumn;
                        break;
                    case Season.Autumn:
                        CurrentSeason = Season.Winter;
                        break;
                    case Season.Winter:
                        CurrentSeason = Season.Spring;
                        break;
                }
            }
            IsCollaborating = false;
            ReprioritizeGoals();
            return activity;
        }

        internal virtual void Advance(IActivity activity)
        {
            Log.Add("");
            Log.Add(CurrentSeason.ToString() + " " + _seasonList.Count() / 4);
            _seasonList.Add(activity);
            activity.Act(this);
            if (SeasonalAge >= 140)
            {
                this.Age(LongevityRitual);
            }
            switch (CurrentSeason)
            {
                case Season.Spring:
                    CurrentSeason = Season.Summer;
                    break;
                case Season.Summer:
                    CurrentSeason = Season.Autumn;
                    break;
                case Season.Autumn:
                    CurrentSeason = Season.Winter;
                    break;
                case Season.Winter:
                    CurrentSeason = Season.Spring;
                    break;
            }
            IsCollaborating = true;
        }

        public virtual void PlanToBeTaught()
        {
            IsCollaborating = true;
            //_mandatoryAction =
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

        /// <summary>
        /// Determines the value of an experience gain in terms of practice seasons
        /// </summary>
        /// <param name="ability"></param>
        /// <param name="gain"></param>
        /// <returns>the season equivalence of this gain</returns>
        public virtual double RateSeasonalExperienceGain(Ability ability, double gain)
        {
            if (MagicArts.IsArt(ability))
            {
                return 0;
            }
            return gain / 4;
        }

        public void AddGoal(IGoal goal)
        {
            _goals.Add(goal);
        }
        #endregion

        #region object Overrides
        public override string ToString()
        {
            return Name;
        }
        #endregion
    }
}
