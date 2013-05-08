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
        protected Ability _writingAbility;
        protected Ability _writingLanguage;
        protected Ability _areaAbility;
        protected Dictionary<Preference, double> _preferences;

        private readonly string[] _virtueList = new string[10];
		private readonly string[] _flawList = new string[10];

        private readonly Dictionary<int, CharacterAbilityBase> _abilityList;
        protected readonly List<IAction> _seasonList;
        protected readonly List<IBook> _booksWritten;
        protected readonly List<IBook> _booksRead;
        protected readonly List<IBook> _booksOwned;
        protected readonly List<GoalBase> _goals;
        protected List<Summa> _incompleteBooks;
        private readonly List<Ability> _writingAbilities;
        #endregion

        #region Events
        public event AgedEventHandler Aged;
        #endregion

        #region Public Fields
        public byte Decrepitude { get; private set; }
        public CharacterAbility Warping { get; private set; }
        public string Name { get; set; }
        public string Log { get; set; }
        #endregion

        public Character(Ability writingLanguage, Ability writingAbility, Ability areaAbility, Dictionary<Preference, double> preferences)
        {
            Die die = new Die();
            Strength = new Attribute(die.RollNormal());
            Stamina = new Attribute(die.RollNormal());
            Dexterity = new Attribute(die.RollNormal());
            Quickness = new Attribute(die.RollNormal());
            Intelligence = new Attribute(die.RollNormal());
            Communication = new Attribute(die.RollNormal());
            Perception = new Attribute(die.RollNormal());
            Presence = new Attribute(die.RollNormal());

            Decrepitude = 0;

            // All characters start at age 5
            _noAgingSeasons = 0;

            _abilityList = new Dictionary<int, CharacterAbilityBase>();
            _seasonList = new List<IAction>();
            _booksRead = new List<IBook>();
            _booksWritten = new List<IBook>();
            _booksOwned = new List<IBook>();
            _goals = new List<GoalBase>();

            _areaAbility = areaAbility;
            _writingAbility = writingAbility;
            _writingLanguage = writingLanguage;
            if (preferences == null)
            {
                _preferences = new Dictionary<Preference, double>();
            }
            else
            {
                _preferences = preferences;
            }
            _writingAbilities = new List<Ability>();
            _writingAbilities.Add(_writingAbility);
            _writingAbilities.Add(_writingLanguage);

            _incompleteBooks = new List<Summa>();

            Log = "";
        }

        #region Ability Functions
        public virtual CharacterAbilityBase GetAbility(Ability ability)
        {
            if (!_abilityList.ContainsKey(ability.AbilityId))
            {
                _abilityList[ability.AbilityId] = new CharacterAbility(ability);

                // if the character doesn't already have a preference related to this ability, add it
                if(!_preferences.ContainsKey(new Preference(PreferenceType.Ability, ability)))
                {
                    _preferences[new Preference(PreferenceType.Ability, ability)] = Die.Instance.RollDouble();
                }
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
            // process books, looking for most interesting readable title
            IAction action = null;
            var availableBooks = _booksOwned.Except(_booksRead.Where(b => b.Level == 0)).Where(b => b.Level > GetAbility(b.Topic).GetValue());
            if (availableBooks.Any())
            {
                IBook bestBook = availableBooks.OrderBy(b => GetBookLevelGain(b) * GetDesire(new Preference(PreferenceType.Ability, b.Topic))).FirstOrDefault();
                if (bestBook != null)
                {
                    Log += "Could read a book on " + bestBook.Topic.AbilityName + " at Q" + bestBook.Quality + "\r\n";
                    action = ConfirmLiteracy(bestBook, RateSeasonalExperienceGain(bestBook.Topic, GetBookLevelGain(bestBook)));
                }
            }

            // consider the value created per season if writing, instead
            EvaluatedBook book = RateBestBookToWrite();
            if (book != null && (action == null || book.PerceivedValue > action.Desire))
            {
                Log += "Could write a book on " + book.Book.Topic.AbilityName + " at Q" + book.Book.Quality + "\r\n";
                action = ConfirmLiteracy(book, book.PerceivedValue);
            }

            // compare the most desirable ability to practice
            double practiceDesire = 0;
            Ability practice = GetPreferredAbilityToPractice(out practiceDesire);
            if (action == null || practiceDesire > action.Desire)
            {
                Log += "Decided to practice " + practice.AbilityName + "\r\n";
                action = new Practice(practice, practiceDesire);
            }

            return action;
        }

        public IAction ConfirmLiteracy(IBook book, double desire)
        {
            if (GetAbility(_writingLanguage).GetValue() < 4)
            {
                // we need to learn to read before we can do anything with these books
                Log += "Cannont read " + _writingLanguage.AbilityName + "\r\n";
                return new Practice(_writingLanguage, desire);

            }
            else if (GetAbility(_writingAbility).GetValue() < 1)
            {
                // TODO: figure out the best way to inject the right activity here once teaching and training are implemented
                Log += "Does not know alphabet" + "\r\n";
                return new Practice(_writingAbility, desire);
            }
            else
            {
                return new Reading(book, desire);
            }
        }

        public IAction ConfirmLiteracy(EvaluatedBook book, double desire)
        {
            double languageValue = GetAbility(_writingLanguage).GetValue();
            if ( languageValue < 5 && languageValue >= 4)
            {
                Log += "Cannont write " + _writingLanguage.AbilityName + "\r\n";
                return GetBestActionForGain(_writingLanguage, desire);
            }
            else if ( languageValue < 4)
            {
                // we need to learn to read before we can do anything with these books
                Log += "Cannont read " + _writingLanguage.AbilityName + "\r\n";
                return new Practice(_writingLanguage, desire);

            }
            else if (GetAbility(_writingAbility).GetValue() < 1)
            {
                // TODO: figure out the best way to inject the right activity here once teaching and training are implemented
                Log += "Does not know alphabet" + "\r\n";
                return new Practice(_writingAbility, desire);
            }
            else
            {
                return new Writing(book.Book.Topic, book.ExposureAbility, book.Book.Level, desire);
            }
        }

        private IAction GetBestActionForGain(Ability ability, double desire)
        {
            // see if there are any books on the topic worth reading
            IBook book = GetBestBookFromCollection(ability);
            double bookExp = GetBookLevelGain(book);
            if (bookExp < 4)
            {
                return new Practice(ability, desire);
            }
            else
            {
                return new Reading(book, desire);
            }
        }

        /// <summary>
        /// Determines which ability in the provided list the character is most interested in boosting
        /// </summary>
        /// <param name="abilityList"></param>
        /// <returns>whichever ability from the list has the highest product of value gain and preference score</returns>
        protected Ability GetBestAbilityToBoost(IEnumerable<Ability> abilityList)
        {
            return abilityList.OrderBy(a => RateSeasonalExperienceGain(a, 2) * _preferences[new Preference(PreferenceType.Ability, a)]).First();
        }

        public virtual void CommitAction(IAction action)
        {
            _seasonList.Add(action);
            action.Act(this);
            if (SeasonalAge >= 140)
            {
                Age(0);
            }
        }

        public virtual void Advance()
        {
            Log += "Season " + _seasonList.Count() + "\r\n";
            IAction activity = DecideSeasonalActivity();
            _seasonList.Add(activity);
            activity.Act(this);
            if (SeasonalAge >= 140)
            {
                Age(0);
            }
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
            if (book.Level == 0)
            {
                return RateSeasonalExperienceGain(book.Topic, book.Quality);
            }
            if (charAbility == null)
            {
                charAbility = GetAbility(book.Topic);
            }

            if (charAbility.GetValue() > book.Level)
            {
                return 0;
            }
            
            double expValue = charAbility.GetExperienceUntilLevel(book.Level);
            double bookSeasons = expValue / book.Quality;
            return RateSeasonalExperienceGain(book.Topic, book.Quality) / bookSeasons;
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

        public bool CanWriteTractatus(CharacterAbilityBase charAbility)
        {
            return charAbility.GetTractatiiLimit() > _booksWritten.Where(b => b.Topic == charAbility.Ability && b.Level == 0).Count();
        }

        public IBook WriteBook(Ability topic, double desiredLevel = 0)
        {
            // grant exposure experience
            List<Ability> abilityList = new List<Ability>(_writingAbilities);
            abilityList.Add(topic);
            Ability exposureAbility = GetBestAbilityToBoost(abilityList);
            GetAbility(exposureAbility).AddExperience(2);
            
            // TODO: When should books moved from the owned list to the covenant library?
            if (desiredLevel == 0)
            {
                Tractatus t = WriteTractatus(topic);
                _booksOwned.Add(t);
                return t;
            }
            else
            {
                Summa s = WriteSumma(topic, desiredLevel);
                if (s != null)
                {
                    _booksOwned.Add(s);
                }
                return s;
            }
        }

        protected Tractatus WriteTractatus(Ability topic)
        {
            Tractatus t = new Tractatus
            {
                Author = this,
                Quality = this.Communication.Value + 3,
                Topic = topic
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
        protected Summa WriteSumma(Ability topic, double desiredLevel)
        {
            Summa s;
            CharacterAbilityBase ability = GetAbility(topic);
            Summa previousWork = _incompleteBooks.Where(b => b.Topic == topic && b.Level == desiredLevel).FirstOrDefault();
            if (previousWork == null)
            {
                double difference = (ability.GetValue() / 2) - desiredLevel;
                if (difference < 0)
                {
                    throw new ArgumentOutOfRangeException();
                }
                s = new Summa()
                {
                    Author = this,
                    Level = desiredLevel,
                    Topic = topic,
                    Quality = MagicArts.IsArt(ability.Ability) ?
                        this.Communication.Value + difference + 6 :
                        this.Communication.Value + (difference * 3) + 6
                };
            }
            else
            {
                s = previousWork;
            }

            s.PointsComplete += this.Communication.Value + GetAbility(_writingLanguage).GetValue();
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

        public virtual EvaluatedBook RateBestBookToWrite()
        {
            EvaluatedBook bestBook = new EvaluatedBook
            {
                Book = null,
                PerceivedValue = 0
            };
            foreach (CharacterAbilityBase charAbility in _abilityList.Values)
            {
                bestBook = RateAgainstBestBook(bestBook, charAbility);
            }
            
            // compare expose in book ability versus writing language
            if (GetAbility(bestBook.Book.Topic).GetValueGain(2) > GetAbility(_writingLanguage).GetValueGain(2))
            {
                bestBook.ExposureAbility = bestBook.Book.Topic;
                bestBook.PerceivedValue += RateSeasonalExperienceGain(bestBook.Book.Topic, 2);
            }
            else
            {
                bestBook.ExposureAbility = _writingLanguage;
                bestBook.PerceivedValue += RateSeasonalExperienceGain(_writingLanguage, 2);
            }


            return bestBook;
        }

        protected EvaluatedBook RateAgainstBestBook(EvaluatedBook bestBook, CharacterAbilityBase charAbility)
        {
            if (CanWriteTractatus(charAbility))
            {
                // calculate tractatus value
                EvaluatedBook tract = EstimateTractatus(charAbility);
                if (tract.PerceivedValue > bestBook.PerceivedValue)
                {
                    bestBook = tract;
                }
            }

            bestBook = RateSummaAgainstBestBook(bestBook, charAbility);
            return bestBook;
        }

        protected virtual EvaluatedBook RateSummaAgainstBestBook(EvaluatedBook bestBook, CharacterAbilityBase ability)
        {
            // calculate summa value
            // TODO: how to decide what audience the magus is writing for?
            // when art > 10, magus will write a /2 book
            // when art >=20, magus will write a /4 book
            if (ability.GetValue() >= 4)
            {
                // start with no q/l switching
                CharacterAbilityBase theoreticalPurchaser = new CharacterAbility(ability.Ability);
                theoreticalPurchaser.AddExperience(ability.Experience / 2);
                Summa s = new Summa
                {
                    Quality = Communication.Value + 6,
                    Level = ability.GetValue() / 2.0,
                    Topic = ability.Ability
                };
                double value = RateLifetimeBookValue(s, theoreticalPurchaser);
                if (value > bestBook.PerceivedValue)
                {
                    bestBook = new EvaluatedBook
                    {
                        Book = s,
                        PerceivedValue = value
                    };
                }
            }
            if (ability.GetValue() >= 6)
            {
                // if more expert, try some q/l switching
                CharacterAbilityBase theoreticalPurchaser = new CharacterAbility(ability.Ability);
                theoreticalPurchaser.AddExperience(ability.Experience / 4);

                double qualityAdd = ability.GetValue() / 4;
                if (qualityAdd > (Communication.Value + 6))
                {
                    qualityAdd = Communication.Value + 6;
                }

                Summa s = new Summa
                {
                    Quality = Communication.Value + 6 + qualityAdd,
                    Level = (ability.GetValue() / 2.0) - qualityAdd,
                    Topic = ability.Ability
                };
                double seasonsNeeded = s.GetWritingPointsNeeded() / (Communication.Value + GetAbility(_writingAbility).GetValue());
                double value = RateLifetimeBookValue(s, theoreticalPurchaser) / seasonsNeeded;
                if (value > bestBook.PerceivedValue)
                {
                    bestBook = new EvaluatedBook
                    {
                        Book = s,
                        PerceivedValue = value
                    };
                }
            }
            return bestBook;
        }

        public virtual EvaluatedBook EstimateTractatus(CharacterAbilityBase charAbility)
        {
            Tractatus t = new Tractatus
            {
                Quality = Communication.Value + 3,
                Topic = charAbility.Ability
            };
            return new EvaluatedBook
            {
                Book = t,
                PerceivedValue = RateLifetimeBookValue(t)
            };
        }
        #endregion

        #region Preference/Goal Functions
        public double GetDesire(Preference pref)
        {
            return _preferences.ContainsKey(pref) ? _preferences[pref] : 0;
        }

        public Ability GetPreferredAbilityToPractice(out double preference)
        {
            Ability ability = null;
            preference = 0;
            foreach (KeyValuePair<Preference, double> prefPair in _preferences)
            {
                if (prefPair.Key.Type == PreferenceType.Ability)
                {
                    Ability thisAbility = (Ability)prefPair.Key.Specifier;
                    CharacterAbilityBase charAbility = GetAbility(thisAbility);
                    double gain = charAbility.GetValueGain(4);
                    double thisPreference = gain * prefPair.Value;
                    if (thisPreference > preference)
                    {
                        ability = thisAbility;
                        preference = thisPreference;
                    }
                }
            }

            return ability;
        }

        /// <summary>
        /// Determines the value of an experience gain in terms of practice seasons
        /// </summary>
        /// <param name="ability"></param>
        /// <param name="gain"></param>
        /// <returns>the season equivalence of this gain</returns>
        protected virtual double RateSeasonalExperienceGain(Ability ability, double gain)
        {
            return gain / 4;
        }
        #endregion
    }
}
