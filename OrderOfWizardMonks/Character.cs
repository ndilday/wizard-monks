using System;
using System.Collections.Generic;
using System.Linq;

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
	public class Character
	{
        #region Attributes
		public Attribute Strength { get; private set; }
		public Attribute Stamina { get; private set; }
		public Attribute Dexterity { get; private set; }
		public Attribute Quickness { get; private set; }
		public Attribute Intelligence { get; private set; }
		public Attribute Communication { get; private set; }
		public Attribute Perception { get; private set; }
		public Attribute Presence { get; private set; }
        #endregion

        #region Private Fields
        private uint _noAgingSeasons;
        private SortedList<PreferenceType, Preference> _preferences;

        private readonly string[] _virtueList = new string[10];
		private readonly string[] _flawList = new string[10];

        private readonly Dictionary<int, CharacterAbilityBase> _abilityList;
        private readonly List<IAction> _seasonList;
        private readonly List<IBook> _booksWritten;
        private readonly List<IBook> _booksRead;
        private readonly List<IBook> _booksOwned;
        private readonly List<GoalBase> _goals;
        #endregion

        public event AgedEventHandler Aged;

        public byte Decrepitude { get; private set; }
        public CharacterAbility Warping { get; private set; }

        public Character()
        {
            Strength = new Attribute(0);
            Stamina = new Attribute(0);
            Dexterity = new Attribute(0);
            Quickness = new Attribute(0);
            Intelligence = new Attribute(0);
            Communication = new Attribute(0);
            Perception = new Attribute(0);
            Presence = new Attribute(0);

            Decrepitude = 0;

            // All characters start at age 5
            _noAgingSeasons = 0;
            _preferences = new SortedList<PreferenceType, Preference>();

            _abilityList = new Dictionary<int, CharacterAbilityBase>();
            _seasonList = new List<IAction>();
            _booksRead = new List<IBook>();
            _booksWritten = new List<IBook>();
            _booksOwned = new List<IBook>();
            _goals = new List<GoalBase>();
        }

        #region Ability Functions
        public virtual CharacterAbilityBase GetAbility(Ability ability)
        {
            return _abilityList.ContainsKey(ability.AbilityId) ? _abilityList[ability.AbilityId] : new CharacterAbility(ability);
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

        public virtual double GetLabTotal(Ability technique, Ability form)
        {
            return 0;
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
            get { return (uint)(_seasonList.Count + 20); }
        }

	    public uint ApparentAge
	    {
            get { return SeasonalAge - _noAgingSeasons; }
	    }

        private void Age(ushort modifiers)
        {
            // roll exploding die for aging
            bool apparent = true;
            bool crisis = false;
            bool died = false;
            ushort agingRoll = Die.Instance.RollExplodingDie();
            agingRoll += modifiers;
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
                IncreaseDecrepitudeToNextLevel();
                int crisisRoll = Die.Instance.RollSimpleDie();
                crisisRoll = crisisRoll + ageModifier + GetDecrepitudeScore();
                if (crisisRoll > 14)
                {
                    int staDiff = 3 * (crisisRoll - 14);
                    if(Stamina.Value + Die.Instance.RollSimpleDie() < staDiff)
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

        public virtual IAction DecideSeasonalActivity()
        {
            if (_goals == null || _goals.Count == 0)
            {
                this.GenerateNewGoals();
            }

            _goals.ForEach(g => g.Flush());
            int bestGoalIndex = 0;
            for (int i = 1; i < _goals.Count; i++)
            {
                if (_goals[i].Score(this) > _goals[bestGoalIndex].Score(this))
                {
                    bestGoalIndex = i;
                }
            }

            // Now that we've deteremined the most important goal,
            // we need to turn it into a seasonal activity selection
            return _goals[bestGoalIndex].GetSeasonalActivity(this);
        }

        public virtual void CommitAction(IAction action)
        {
            action.Act(this);
            this.Age(0);
        }

        public virtual void GenerateNewGoals()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Book Functions
        public virtual bool ValidToRead(IBook book)
        {
            return book.Author != this && !this._booksRead.Contains(book);
        }

        public virtual IEnumerable<IBook> GetBooksFromCollection(Ability ability)
        {
            return _booksOwned.Where(b => b.Topic == ability).OrderBy(b => b.Quality);
        }

        public virtual IBook GetBestBookFromCollection(Ability ability)
        {
             return GetBooksFromCollection(ability).Except(_booksRead).Except(_booksWritten).OrderBy(b => b.Quality).FirstOrDefault();
        }

        public virtual double RateBook(IBook book)
        {
            CharacterAbilityBase ability;
            if (book == null)
            {
                return 0;
            }
            if (_abilityList.ContainsKey(book.Topic.AbilityId))
            {
                ability = _abilityList[book.Topic.AbilityId];
            }
            else
            {
                ability = new CharacterAbility(book.Topic);
            }

            // determine difference in ability using the new book compared to the old book
            CharacterAbilityBase newAbility = ability.MakeCopy();
            newAbility.AddExperience(book.Quality, book.Level);
            double difference = newAbility.GetValue() - ability.GetValue();

            //TODO: multiply difference by preference
            return difference;
        }

        public virtual void ReadBook(IBook book)
        {
            CharacterAbilityBase ability = GetAbility(book.Topic);
            bool previouslyRead = _booksRead.Contains(book);
            if (!previouslyRead || ability.GetValue() < book.Level)
            {
                ability.AddExperience(book.Quality, book.Level);
            }
            if (!previouslyRead)
            {
                _booksRead.Add(book);
            }
        }

        public virtual void WriteBook()
        {
        }

        public double GetBestGain(Ability ability)
        {
            // see if there are any books on the topic worth reading
            double bookExp = RateBook(GetBestBookFromCollection(ability));
            return bookExp <= 4 ? 4 : bookExp;
        }
        #endregion
    }
}
