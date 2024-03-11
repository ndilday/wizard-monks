using System;
using System.Collections.Generic;
using System.Linq;

using WizardMonks.Core;
using WizardMonks.Decisions;
using WizardMonks.Decisions.Goals;
using WizardMonks.Instances;

namespace WizardMonks.Characters
{
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
            if (SeasonalAge < 60)
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
        private uint _noAgingSeasons;
        private readonly uint _baseAge;
        protected Ability _writingAbility;
        protected Ability _writingLanguage;
        protected Ability _areaAbility;
        protected List<IGoal> _goals;
        protected List<string> _verboseLog;

        private readonly string[] _virtueList = new string[10];
        private readonly string[] _flawList = new string[10];

        private readonly Dictionary<int, CharacterAbilityBase> _abilityList;
        protected readonly List<IAction> _seasonList;
        protected readonly List<IBook> _booksWritten;
        protected readonly List<IBook> _booksRead;
        protected readonly List<IBook> _booksOwned;
        protected List<Summa> _incompleteBooks;
        private readonly List<Ability> _writingAbilities;

        protected IAction _mandatoryAction;
        #endregion

        #region Events
        public event AgedEventHandler Aged;
        #endregion

        #region Public Properties
        public Personality Personality { get; private set; }
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
                    (!_booksRead.Contains(b) || b.Level != 1000 && b.Level > this.GetAbility(b.Topic).Value));
            }
        }
        public bool IsCollaborating { get; private set; }
        public bool WantsToFollow { get; protected set; }
        #endregion

        public Character(Ability writingLanguage, Ability writingAbility, Ability areaAbility, uint baseSeasonableAge = 20)
        {
            _attributes[(short)AttributeType.Strength] = new Attribute(Die.Instance.RollNormal());
            _attributes[(short)AttributeType.Stamina] = new Attribute(Die.Instance.RollNormal());
            _attributes[(short)AttributeType.Dexterity] = new Attribute(Die.Instance.RollNormal());
            _attributes[(short)AttributeType.Quickness] = new Attribute(Die.Instance.RollNormal());
            _attributes[(short)AttributeType.Intelligence] = new Attribute(Die.Instance.RollNormal());
            _attributes[(short)AttributeType.Communication] = new Attribute(Die.Instance.RollNormal());
            _attributes[(short)AttributeType.Perception] = new Attribute(Die.Instance.RollNormal());
            _attributes[(short)AttributeType.Presence] = new Attribute(Die.Instance.RollNormal());

            Personality = new Personality();

            Decrepitude = 0;
            CurrentSeason = Season.Spring;
            KnownAuras = [];
            IsCollaborating = false;
            WantsToFollow = true;

            _noAgingSeasons = 0;
            _baseAge = baseSeasonableAge;
            _mandatoryAction = null;

            _abilityList = [];
            _seasonList = [];
            _booksRead = [];
            _booksWritten = [];
            _booksOwned = [];
            _verboseLog = [];

            _areaAbility = areaAbility;
            _writingAbility = writingAbility;
            _writingLanguage = writingLanguage;
            _writingAbilities = [_writingAbility, _writingLanguage];

            _incompleteBooks = [];
            _goals = [];
            Log = [];
            Warping = new CharacterAbility(Abilities.Warping);
        }

        #region Ability Functions
        public virtual CharacterAbilityBase GetAbility(Ability ability)
        {
            if (!_abilityList.TryGetValue(ability.AbilityId, out CharacterAbilityBase value))
            {
                value = new CharacterAbility(ability);
                _abilityList[ability.AbilityId] = value;
            }

            return value;
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
            Aged?.Invoke(this, e);
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
                    if (GetAttribute(AttributeType.Stamina).Value + Die.Instance.RollSimpleDie() < staDiff)
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

            if (Decrepitude > 74)
            {
                died = true;
            }

            AgingEventArgs args = new(this, crisis, apparent, died);
            OnAged(args);
        }

        private void IncreaseDecrepitudeToNextLevel()
        {
            // TODO: decrepitude points need to go to attributes
            // TODO: add configuration option to choose between different methods of distributing decrepitude points
            if (Decrepitude < 5)
            {
                Decrepitude = 5;
            }
            else if (Decrepitude < 15)
            {
                Decrepitude = 15;
            }
            else if (Decrepitude < 30)
            {
                Decrepitude = 30;
            }
            else if (Decrepitude < 50)
            {
                Decrepitude = 50;
            }
            else if (Decrepitude < 75)
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

        IAction DecideSeasonalActivity()
        {
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
                        goal.AddActionPreferencesToList(actions, _verboseLog);
                    }
                }
                Log.AddRange(actions.Log());
                return actions.GetBestAction();
            }
        }

        public virtual void ReprioritizeGoals()
        {
            foreach (IGoal goal in _goals)
            {
                if (!goal.IsComplete())
                {
                    if (goal.AgeToCompleteBy > SeasonalAge)
                    {
                        Log.Add("Failed to achieve a goal");
                        _goals.Remove(goal);
                    }
                }
            }
        }

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
            if (!IsCollaborating)
            {
                Log.Add("");
                Log.Add(CurrentSeason.ToString() + " " + _seasonList.Count / 4);
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
            }
            IsCollaborating = false;
            ReprioritizeGoals();
        }

        internal virtual void Advance(IAction activity)
        {
            Log.Add("");
            Log.Add(CurrentSeason.ToString() + " " + _seasonList.Count / 4);
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
            IsCollaborating = true;
        }

        public virtual void PlanToBeTaught()
        {
            IsCollaborating = true;
            //_mandatoryAction =
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

            return ReadableBooks.Where(b => b.Topic == ability);
        }

        public virtual IBook GetBestBookToRead(Ability ability)
        {
            // TODO: may eventually want to take into account reading a slower summa before a higher quality tractatus?
            return ReadableBooks.Where(b => b.Topic == ability).OrderByDescending(b => GetBookLevelGain(b)).FirstOrDefault();
        }

        public virtual IBook GetBestSummaToRead(Ability ability)
        {
            return ReadableBooks.Where(b => b.Topic == ability & b.Level < 1000).OrderByDescending(b => GetBookLevelGain(b)).FirstOrDefault();
        }

        public virtual IEnumerable<IBook> GetUnneededBooksFromCollection()
        {
            return _booksOwned.Where(b => b.Author == this || _booksRead.Contains(b) && b.Level == 1000 || GetAbility(b.Topic).Value >= b.Level);
        }

        public virtual bool ValidToRead(IBook book)
        {
            return book.Author != this && (!_booksRead.Contains(book) || GetAbility(book.Topic).Value < book.Level);
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
            // see if it's a tractatus
            if (book.Level == 1000)
            {
                return RateSeasonalExperienceGain(book.Topic, book.Quality);
            }
            if (charAbility == null)
            {
                charAbility = GetAbility(book.Topic);
            }

            // if this book is beneath me, don't pay for it
            if (charAbility.Value >= book.Level)
            {
                return 0;
            }

            //TODO: see if we already have a summa on this topic
            IBook existingBook = GetBestBookToRead(book.Topic);
            double expValue = charAbility.GetExperienceUntilLevel(book.Level);
            double bookSeasons = expValue / book.Quality;

            if (existingBook != null)
            {
                // for now, rate it in terms of marginal value difference
                return (RateSeasonalExperienceGain(book.Topic, book.Quality) - RateSeasonalExperienceGain(existingBook.Topic, existingBook.Quality)) * bookSeasons;
            }
            else
            {
                return RateSeasonalExperienceGain(book.Topic, book.Quality) * bookSeasons;
            }
        }

        public virtual void ReadBook(IBook book)
        {
            Log.Add("Reading " + book.Title);
            CharacterAbilityBase ability = GetAbility(book.Topic);
            bool previouslyRead = _booksRead.Contains(book);
            if (!previouslyRead || book.Level != 1000 && ability.Value < book.Level)
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
            Tractatus t = new()
            {
                Author = this,
                Quality = GetAttribute(AttributeType.Communication).Value + 6,
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
                double difference = ability.Value / 2 - desiredLevel;
                if (difference < 0)
                {
                    throw new Exception("Attempting to write a summa when not skilled enough");
                }
                s = new Summa()
                {
                    Author = this,
                    Level = desiredLevel,
                    Topic = topic,
                    Title = name,
                    Quality = MagicArts.IsArt(ability.Ability) ?
                        GetAttribute(AttributeType.Communication).Value + difference + 6 :
                        GetAttribute(AttributeType.Communication).Value + difference * 3 + 6
                };
            }
            else
            {
                s = previousWork;
            }

            s.PointsComplete += GetAttribute(AttributeType.Communication).Value + GetAbility(_writingLanguage).Value;
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

        public virtual IBook GetBestBookToWrite()
        {
            return null;
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
