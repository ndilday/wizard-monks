using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using WizardMonks.Instances;

namespace WizardMonks
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
	public partial class Character
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
            // TODO: make less stair-step
            if (SeasonalAge <= 28)
            {
                return value - 4.0;
            }
            else if (SeasonalAge <= 36)
            {
                return value - 3.0;
            }
            else if (SeasonalAge <= 44)
            {
                return value - 2.0;
            }
            else if (SeasonalAge <= 52)
            {
                return value - 1.0;
            }
            else
            {
                return value;
            }
        }
        #endregion

        #region Private Fields
        private uint _noAgingSeasons;
        private uint _baseAge;
        protected Ability _writingAbility;
        protected Ability _writingLanguage;
        protected Ability _areaAbility;

        private readonly string[] _virtueList = new string[10];
		private readonly string[] _flawList = new string[10];

        private readonly Dictionary<int, CharacterAbilityBase> _abilityList;
        protected readonly List<IAction> _seasonList;
        protected readonly List<IBook> _booksWritten;
        protected readonly List<IBook> _booksRead;
        protected readonly List<IBook> _booksOwned;
        protected List<Summa> _incompleteBooks;
        private readonly List<Ability> _writingAbilities;
        #endregion

        #region Events
        public event AgedEventHandler Aged;
        #endregion

        #region Public Properties
        public ushort LongevityRitual { get; private set; }
        public byte Decrepitude { get; private set; }
        public CharacterAbility Warping { get; private set; }
        public IList<Aura> KnownAuras { get; private set; }
        public string Name { get; set; }
        public Season CurrentSeason { get; private set; }
        public List<string> Log { get; private set; }
        public IEnumerable<IBook> Books
        {
            get
            {
                return _booksOwned;
            }
        }
        public IEnumerable<IBook> ReadableBooks
        {
            get
            {
                return _booksOwned.Where(b => b.Author != this && 
                    ((b.Level == 1000 && !_booksRead.Contains(b)) || (b.Level != 1000 && b.Level > this.GetAbility(b.Topic).Value)));
            }
        }
        #endregion

        public Character(Ability writingLanguage, Ability writingAbility, Ability areaAbility, List<IGoal> startingGoals = null, uint baseSeasonableAge = 20)
        {
            Die die = new Die();
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

            _noAgingSeasons = 0;
            _baseAge = baseSeasonableAge;

            _abilityList = new Dictionary<int, CharacterAbilityBase>();
            _seasonList = new List<IAction>();
            _booksRead = new List<IBook>();
            _booksWritten = new List<IBook>();
            _booksOwned = new List<IBook>();

            _areaAbility = areaAbility;
            _writingAbility = writingAbility;
            _writingLanguage = writingLanguage;
            _writingAbilities = new List<Ability>();
            _writingAbilities.Add(_writingAbility);
            _writingAbilities.Add(_writingLanguage);

            _incompleteBooks = new List<Summa>();
            _goals = startingGoals == null ? new List<IGoal>() : startingGoals;
            Log = new List<string>();
            Warping = new CharacterAbility(Abilities.Warping);
        }

        #region Ability Functions
        public virtual CharacterAbilityBase GetAbility(Ability ability)
        {
            if (!_abilityList.ContainsKey(ability.AbilityId))
            {
                _abilityList[ability.AbilityId] = new CharacterAbility(ability);
            }
            
            return _abilityList[ability.AbilityId];
        }

        public virtual IEnumerable<CharacterAbilityBase> GetAbilities()
        {
            return _abilityList.Values;
        }
        
        protected virtual void AddAbility(Ability ability)
        {
            if (ability.AbilityType == AbilityType.Art)
            {
                _abilityList.Add(ability.AbilityId, new AcceleratedAbility(ability));
            }
            else
            {
                _abilityList.Add(ability.AbilityId, new CharacterAbility(ability));
            }
        }

        public virtual double GetAbilityMaximumFromReading(Ability ability)
        {
            CharacterAbilityBase charAbility = GetAbility(ability).MakeCopy();
            double value = charAbility.Value;
            var books = GetReadableBooksFromCollection(ability);
            IBook summa = books.Where(b => b.Level < 1000).OrderBy(b => b.Level).FirstOrDefault();
            if (summa != null && summa.Level > value)
            {
                charAbility.AddExperience(1000, summa.Level);
            }
            foreach (IBook book in books.Where(b => b.Level == 1000))
            {
                charAbility.AddExperience(book.Quality);
            }
            return charAbility.Value;
        }
        #endregion

        #region Aging
        public virtual void OnAged(AgingEventArgs e)
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
            get { return SeasonalAge - _noAgingSeasons; }
	    }

        public void ApplyLongevityRitual(ushort strength)
        {
            LongevityRitual = strength;
        }

        private void Age(ushort modifiers)
        {
            // roll exploding die for aging
            if (LongevityRitual > 0)
            {
                Warping.AddExperience(0.25);
            }
            bool apparent = true;
            bool crisis = false;
            bool died = false;
            ushort agingRoll = Die.Instance.RollExplodingDie();
            agingRoll -= modifiers;
            ushort ageModifier = (ushort)Math.Ceiling(SeasonalAge / 40.0m);
            agingRoll += ageModifier;

            if (agingRoll < 3)
            {
                _noAgingSeasons++;
                apparent = false;
            }

            if (agingRoll == 13 || agingRoll > 21)
            {
                crisis = true;
                LongevityRitual = 0;
                IncreaseDecrepitudeToNextLevel();
                int crisisRoll = Die.Instance.RollSimpleDie();
                crisisRoll = crisisRoll + ageModifier + GetDecrepitudeScore();
                if (crisisRoll > 14)
                {
                    int staDiff = 3 * (crisisRoll - 14);
                    if(GetAttribute(AttributeType.Stamina).Value + Die.Instance.RollSimpleDie() < staDiff)
                    {
                        died = true;
                        Decrepitude = 75;
                    }
                }
            }
            else if (agingRoll > 9)
            {
                Decrepitude++;
            }

            if(Decrepitude > 74)
            {
                died = true;
            }

            AgingEventArgs args = new AgingEventArgs(this, crisis, apparent, died);
            OnAged(args);
        }

        private void IncreaseDecrepitudeToNextLevel()
        {
            // TODO: decrepitude points need to go to attributes
            // TODO: add configuration option to choose between different methods of distributing decrepitude points
            if(Decrepitude < 5)
            {
                Decrepitude = 5;
            }
            else if(Decrepitude < 15)
            {
                Decrepitude = 15;
            }
            else if(Decrepitude < 30)
            {
                Decrepitude = 30;
            }
            else if(Decrepitude < 50)
            {
                Decrepitude = 50;
            }
            else if(Decrepitude < 75)
            {
                Decrepitude = 75;
            }
        }

        private byte GetDecrepitudeScore()
        {
            if (Decrepitude < 5) return 0;
            if (Decrepitude < 15) return 1;
            if (Decrepitude < 30) return 2;
            if (Decrepitude < 50) return 3;
            if (Decrepitude < 75) return 4;
            return 5;
        }
        #endregion

        #region Seasonal Functions

        public virtual void CommitAction(IAction action)
        {
            _seasonList.Add(action);
            action.Act(this);
            if (SeasonalAge >= 140)
            {
                Age(LongevityRitual);
            }
        }

        public virtual void Advance()
        {
            Log.Add("");
            Log.Add(CurrentSeason.ToString() + " " + _seasonList.Count() / 4);
            IAction activity = DecideSeasonalActivity();
            _seasonList.Add(activity);
            activity.Act(this);
            if (SeasonalAge >= 140)
            {
                Age(LongevityRitual);
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
            ReprioritizeGoals();
        }
        #endregion

        #region Book Functions
        public virtual void AddBookToCollection(IBook book)
        {
            _booksOwned.Add(book);
        }

        public virtual void RemoveBookFromCollection(IBook book)
        {
            _booksOwned.Remove(book);
        }

        public virtual IEnumerable<IBook> GetBooksFromCollection(Ability ability)
        {
            return _booksOwned.Where(b => b.Topic == ability);
        }

        public virtual IEnumerable<IBook> GetReadableBooksFromCollection(Ability ability)
        {
            var books = _booksOwned.Where(b => !_booksRead.Contains(b));
            return books.Where(b => b.Topic == ability && b.Author != this && b.Level > GetAbility(ability).Value);
        }

        public virtual IEnumerable<IBook> GetUnneededBooksFromCollection()
        {
            return _booksOwned.Where(b => b.Author == this || (_booksRead.Contains(b) && b.Level == 1000) || (GetAbility(b.Topic).Value >= b.Level));
        }
        
        public virtual bool ValidToRead(IBook book)
        {
            return book.Author != this && (!this._booksRead.Contains(book) || (GetAbility(book.Topic).Value < book.Level));
        }

        public virtual double GetBookLevelGain(IBook book)
        {
            if (book == null)
            {
                return 0;
            }

            // determine difference in ability using the new book compared to the old book
            return GetAbility(book.Topic).GetValueGain(book.Quality, book.Level);
        }

        public double RateLifetimeBookValue(IBook book, CharacterAbilityBase charAbility = null)
        {
            if (book.Level == 1000)
            {
                return RateSeasonalExperienceGain(book.Topic, book.Quality);
            }
            if (charAbility == null)
            {
                charAbility = GetAbility(book.Topic);
            }

            if (charAbility.Value > book.Level)
            {
                return 0;
            }
            
            double expValue = charAbility.GetExperienceUntilLevel(book.Level);
            double bookSeasons = expValue / book.Quality;
            return RateSeasonalExperienceGain(book.Topic, book.Quality) * bookSeasons;
        }

        public virtual void ReadBook(IBook book)
        {
            Log.Add("Reading " + book.Title);
            CharacterAbilityBase ability = GetAbility(book.Topic);
            bool previouslyRead = _booksRead.Contains(book);
            if (!previouslyRead || (book.Level != 1000 && ability.Value < book.Level))
            {
                ability.AddExperience(book.Quality, book.Level);
            }
            if (!previouslyRead)
            {
                _booksRead.Add(book);
            }
        }

        public bool CanWriteTractatus(CharacterAbilityBase charAbility)
        {
            return charAbility.GetTractatiiLimit() > GetTractatiiWrittenOnTopic(charAbility.Ability);
        }

        public IBook WriteBook(Ability topic, string name, Ability exposureAbility, double desiredLevel = 0)
        {
            // grant exposure experience
            List<Ability> abilityList = new List<Ability>(_writingAbilities);
            abilityList.Add(topic);
            GetAbility(exposureAbility).AddExperience(2);
            
            // TODO: When should books moved from the owned list to the covenant library?
            if (desiredLevel == 1000)
            {
                Tractatus t = WriteTractatus(topic, name);
                _booksOwned.Add(t);
                return t;
            }
            else
            {
                Summa s = WriteSumma(topic, name, desiredLevel);
                if (s != null)
                {
                    _booksOwned.Add(s);
                }
                return s;
            }
        }

        protected Tractatus WriteTractatus(Ability topic, string name)
        {
            Tractatus t = new Tractatus
            {
                Author = this,
                Quality = this.GetAttribute(AttributeType.Communication).Value + 6,
                Topic = topic,
                Title = name
            };
            _booksWritten.Add(t);
            return t;
        }

        /// <summary>
        /// Works on a summa on the given topic. 
        /// If the work invested is not enough to finish a book on that topic,
        /// the incomplete work is added to the list
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="desiredLevel"></param>
        /// <returns>the summa if it is completed, null otherwise</returns>
        protected Summa WriteSumma(Ability topic, string name, double desiredLevel)
        {
            Summa s;
            CharacterAbilityBase ability = GetAbility(topic);
            Summa previousWork = _incompleteBooks.Where(b => b.Title == name).FirstOrDefault();
            if (previousWork == null)
            {
                double difference = (ability.Value / 2) - desiredLevel;
                if (difference < 0)
                {
                    throw new ArgumentOutOfRangeException();
                }
                s = new Summa()
                {
                    Author = this,
                    Level = desiredLevel,
                    Topic = topic,
                    Title = name,
                    Quality = MagicArts.IsArt(ability.Ability) ?
                        this.GetAttribute(AttributeType.Communication).Value + difference + 6 :
                        this.GetAttribute(AttributeType.Communication).Value + (difference * 3) + 6
                };
            }
            else
            {
                s = previousWork;
            }

            s.PointsComplete += this.GetAttribute(AttributeType.Communication).Value + GetAbility(_writingLanguage).Value;
            if (s.PointsComplete >= s.GetWritingPointsNeeded())
            {
                _booksWritten.Add(s);
                if (previousWork != null)
                {
                    _incompleteBooks.Remove(previousWork);
                }
                return s;
            }
            return null;
        }

        public bool HasWrittenBookWithTitle(string title)
        {
            return _booksWritten.Where(b => b.Title == title).Any();
        }

        public ushort GetTractatiiWrittenOnTopic(Ability topic)
        {
            return (ushort)_booksWritten.Where(b => b.Topic == topic && b.Level == 1000).Count();
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
        #endregion
    }
}
