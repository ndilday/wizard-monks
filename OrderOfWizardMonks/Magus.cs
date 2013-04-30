using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WizardMonks.Instances;

namespace WizardMonks
{
    public class TwilightEventArgs : EventArgs
    {
        DateTime _duration;
        ushort _extraWarping;

        public TwilightEventArgs(DateTime duration, ushort extraWarping)
        {
            _duration = duration;
            _extraWarping = extraWarping;
        }
    }

	[Serializable]
	public class Magus : Character
	{
        private Ability _magicAbility;
        private Covenant _covenant;
        private Dictionary<Ability, double> _visStock;
        private List<Ability> _extractionAbilities;

        public Magus(Ability magicAbility, Ability writingLanguage, Ability writingAbility, Dictionary<Preference, double> preferences)
            : base(writingLanguage, writingAbility, preferences)
        {
            _magicAbility = magicAbility;
            Arts = new Arts();
            _covenant = null;
            _visStock = new Dictionary<Ability, double>();
            foreach (Ability art in MagicArts.GetEnumerator())
            {
                _visStock[art] = 0;
            }
            _extractionAbilities = new List<Ability>(MagicArts.GetExtractionAbilities());
            _extractionAbilities.Add(_magicAbility);
        }

		public Houses House { get; set; }

        
        public Arts Arts { get; private set; }

        public override CharacterAbilityBase GetAbility(Ability ability)
        {
            if (MagicArts.IsArt(ability))
            {
                return Arts.GetAbility(ability);
            }
            else
            {
                return base.GetAbility(ability);
            }
        }

        #region Covenant Functions
        public void Join(Covenant covenant)
        {
            _covenant = covenant;
            covenant.AddMagus(this);
        }

        public Covenant FoundCovenant(int auraLevel = 0)
        {
            Covenant coventant = new Covenant();
            Join(coventant);
            coventant.Aura = auraLevel;
            return coventant;
        }
        #endregion

        #region Book Functions
        public override IEnumerable<IBook> GetBooksFromCollection(Ability ability)
        {
            IEnumerable<IBook> books = _booksOwned.Where(b => b.Topic == ability);
            if (_covenant != null)
            {
                books = books.Union(_covenant.GetLibrary(ability));
            }
            return books;
        }

        public override EvaluatedBook RateBestBookToWrite()
        {
            EvaluatedBook bestBook = base.RateBestBookToWrite();
            foreach (Ability art in MagicArts.GetEnumerator())
            {
                bestBook = RateAgainstBestBook(bestBook, Arts.GetAbility(art));
            }
            return bestBook;
        }

        protected override EvaluatedBook RateSummaAgainstBestBook(EvaluatedBook bestBook, CharacterAbilityBase ability)
        {
            if (MagicArts.IsArt(ability.Ability))
            {
                // calculate summa value
                // TODO: how to decide what audience the magus is writing for?
                // when art > 10, magus will write a /2 book
                // when art >=20, magus will write a /4 book
                if (ability.GetValue() >= 10)
                {
                    // start with no q/l switching
                    CharacterAbilityBase theoreticalPurchaser = new AcceleratedAbility(ability.Ability);
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
                if (ability.GetValue() >= 20)
                {
                    // start with no q/l switching
                    CharacterAbilityBase theoreticalPurchaser = new AcceleratedAbility(ability.Ability);
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
            else
            {
                return base.RateSummaAgainstBestBook(bestBook, ability);
            }
        }
        #endregion

        #region Goal/Preference Functions
        private void GenerateArtLearningGoal(Ability art, int seasonsLived, double level)
        {
            double desire = GetDesire(new Preference(PreferenceType.Ability, art));
            _goals.Add(new AbilityGoal
                {
                    Ability = art,
                    Level = level,
                    SeasonsToComplete = (uint)(400 - seasonsLived),
                    Priority = desire
                });
        }

        private void GenerateArtWritingGoal(Ability art)
        {
            double desire = GetDesire(new Preference(PreferenceType.Writing, art));
            uint timeFrame = (uint)(20 / desire);
            int tractLimit = GetAbility(art).GetTractatiiLimit();
            if (tractLimit > _booksWritten.Where(b => b.Topic == art && b.Level == 0).Count())
            {
                _goals.Add(new WritingGoal(art, 0, 0, timeFrame, desire));
            }
        }

        /// <summary>
        /// Determines the value of an experience gain in terms of the value of vis, 
        /// and the amount of time it would take the character to produce and learn from
        /// that amount of vis
        /// </summary>
        /// <param name="ability"></param>
        /// <param name="gain"></param>
        /// <returns>the vis equivalence of this gain</returns>
        protected override double RateSeasonalExperienceGain(Ability ability, double gain)
        {
            double visGainPer = GetLabTotal(MagicArts.Creo, MagicArts.Vim) / 10;

            CharacterAbilityBase charAbility = GetAbility(ability);
            double visUsePer = 0.5 + (charAbility.GetValue() / 10.0);
            // the gain per season depends on how the character views vis
            double visNeeded = (gain / _preferences[_visDesire]) * visUsePer;
            // compare to the number of seasons we would need to extract the vis
            // plus the number of seasons we would need to study the extracted vis
            // this effectively means that a gain's base value is twice its vis cost
            double extractTime = visNeeded / visGainPer;
            // TODO: exposure should get rated according to the visUse of the preferred exposure choice
            // rather than the visUse of the base ability
            double visValueOfExposure = extractTime * 2 * visUsePer / _preferences[_visDesire];
            return (2 * visNeeded) - visValueOfExposure;
        }
        #endregion

        protected void CheckTwilight()
        {
        }

        public override IAction DecideSeasonalActivity()
        {
            // make sure all preference values are scaled the same
            IAction start = base.DecideSeasonalActivity();

            // study vis
            foreach (Ability art in _visStock.Keys)
            {
                if (_visStock[art] >= GetAbility(art).GetValue())
                {
                    // TODO: figure out valuation of vis studying
                    double desire = 1;
                    start = new VisStudying(art, desire);
                }
            }

            if (start.Desire < 1 && _covenant != null && _covenant.Aura > 0)
            {
                //TODO: factor in what the exposure will be, and the value of that exposure
                Ability exposureAbility = GetBestAbilityToBoost(_extractionAbilities);
                double visGainPer = GetLabTotal(MagicArts.Creo, MagicArts.Vim) / 10;

                CharacterAbilityBase charAbility = GetAbility(exposureAbility);
                double visUsePer = 0.5 + (charAbility.GetValue() / 10.0);
                double visNeeded = visUsePer * 2 / _preferences[_visDesire];

                start = new VisExtracting(exposureAbility, visGainPer + visNeeded);
            }

            return start;
        }

        #region Magic Functions
        public double GetLabTotal(Ability technique, Ability form)
        {
            double magicTheory = GetAbility(_magicAbility).GetValue();
            double techValue = Arts.GetAbility(technique).GetValue();
            double formValue = Arts.GetAbility(form).GetValue();
            double labTotal =  magicTheory + techValue + formValue;
            if (_covenant != null)
            {
                labTotal += _covenant.Aura;
            }

            //TODO: laboratory
            //TODO: foci
            //TODO: lab assistant
            //TODO: familiar
            return labTotal;
        }

        public void ExtractVis()
        {
            // add vis to personal inventory or covenant inventory
            _visStock[MagicArts.Vim] += GetLabTotal(MagicArts.Creo, MagicArts.Vim) / 10;
            // TODO: grant exposure experience
        }
        #endregion
    }
}
