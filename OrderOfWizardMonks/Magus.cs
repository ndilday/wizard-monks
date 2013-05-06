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
        private Laboratory _laboratory;
        private Dictionary<Ability, double> _visStock;
        private List<Ability> _extractionAbilities;
        private List<Ability> _magicSearchAbilities;
        private Ability _preferredExtractionAbility;
        private Ability _preferredMagicSearchAbility;

        public Magus(Ability magicAbility, Ability writingLanguage, Ability writingAbility, Ability areaAbility, Dictionary<Preference, double> preferences)
            : base(writingLanguage, writingAbility, areaAbility, preferences)
        {
            _magicAbility = magicAbility;
            Arts = new Arts();
            _covenant = null;
            _laboratory = null;
            _visStock = new Dictionary<Ability, double>();
            foreach (Ability art in MagicArts.GetEnumerator())
            {
                _visStock[art] = 0;
            }
            _extractionAbilities = new List<Ability>(3);
            _extractionAbilities.Add(_magicAbility);
            _extractionAbilities.Add(MagicArts.Creo);
            _extractionAbilities.Add(MagicArts.Vim);

            // set up events for caching which abilities to choose for exposure in various situations
            foreach (Ability ability in _extractionAbilities)
            {
                GetAbility(ability).Changed += ExtractionAbilityChanged;
            }

            _magicSearchAbilities = new List<Ability>(3);
            _magicSearchAbilities.Add(_areaAbility);
            _magicSearchAbilities.Add(MagicArts.Intellego);
            _magicSearchAbilities.Add(MagicArts.Vim);

            // set up events for caching which abilities to choose for exposure in various situations
            foreach (Ability ability in _magicSearchAbilities)
            {
                GetAbility(ability).Changed += SearchAbilityChanged;
            }


            RecalculateBestExtractionAbility();
            RecalculateBestSearchAbility();
        }

		public Houses House { get; set; }

        public Covenant Covenant
        {
            get
            {
                return _covenant;
            }
        }
        
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

        #region Event Handlers
        private void ExtractionAbilityChanged(object sender, EventArgs e)
        {
            RecalculateBestExtractionAbility();
        }

        private void SearchAbilityChanged(object sender, EventArgs e)
        {
            RecalculateBestSearchAbility();
        }

        private void RecalculateBestExtractionAbility()
        {
            _preferredExtractionAbility = GetBestAbilityToBoost(_extractionAbilities);
        }

        private void RecalculateBestSearchAbility()
        {
            _preferredMagicSearchAbility = GetBestAbilityToBoost(_magicSearchAbilities);
        }
        #endregion

        #region Covenant Functions
        public void Join(Covenant covenant)
        {
            _covenant = covenant;
            covenant.AddMagus(this);
        }

        public Covenant FoundCovenant(double auraLevel = 0)
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
            double visGainPer = GetLabTotal(MagicArts.Creo, MagicArts.Vim, Activity.DistillVis) / 10.0;
            if (MagicArts.IsTechnique(ability))
            {
                visGainPer /= 4;
            }
            else if (MagicArts.IsForm(ability) && ability != MagicArts.Vim)
            {
                visGainPer /= 2;
            }

            CharacterAbilityBase charAbility = GetAbility(ability);
            double visUsePer = 0.5 + (charAbility.GetValue() / 10.0);
            // the gain per season depends on how the character views vis
            double visNeeded = (gain / _preferences[_visDesire]) * visUsePer;
            // compare to the number of seasons we would need to extract the vis
            // plus the number of seasons we would need to study the extracted vis
            // this effectively means that a gain's base value is twice its vis cost
            double extractTime = visNeeded / visGainPer;
            // exposure should get rated according to the visUse of the preferred exposure choice
            // rather than the visUse of the base ability
            double extractVisUsePer = (GetAbility(_preferredExtractionAbility).GetValue() / 10.0) + 0.5;
            double visValueOfExposure = extractTime * 2 * extractVisUsePer / _preferences[_visDesire];
            return (2 * visNeeded) - visValueOfExposure;
        }
        #endregion

        #region Seasonal/Rating Functions
        public override IAction DecideSeasonalActivity()
        {
            // TODO: how to estimate future return on investment?
            // we're currently really rating everything on a basis
            // sort  of defined by opportunity cost
            IAction start;
            if (_laboratory == null)
            {
                if (_covenant == null)
                {
                    // if we don't have a covenant, perhaps we should look for a site to found one
                    // before building a lab
                    return new FindAura(_preferredMagicSearchAbility, 100);
                }
                // in most circumstances, building a lab should come first
                // TODO: when shouldn't it?
                return new BuildLaboratory(_magicAbility, 100);
            }
            else
            {
                // TODO: how do we rate lab work against non-lab work?
                // TODO: refine lab logic
            }

            // make sure all preference values are scaled the same
            start = base.DecideSeasonalActivity();

            // study vis
            start = RateVisStudy(start);

            start = RateVisExtraction(start);

            return start;
        }

        private IAction RateVisExtraction(IAction start)
        {
            if (_covenant != null && _covenant.Aura > 0)
            {
                // factor in what ability the exposure will be in, and the value of that exposure
                double visGainPer = GetLabTotal(MagicArts.Creo, MagicArts.Vim, Activity.DistillVis) / 10;

                CharacterAbilityBase charAbility = GetAbility(_preferredExtractionAbility);
                double visUsePer = 0.5 + (charAbility.GetValue() / 10.0);
                double visNeeded = visUsePer * 2 / _preferences[_visDesire];

                start = new VisExtracting(_preferredExtractionAbility, visGainPer + visNeeded);
            }
            return start;
        }

        private IAction RateVisStudy(IAction start)
        {
            foreach (Ability art in _visStock.Keys)
            {
                double visUse = 0.5 + (GetAbility(art).GetValue() / 10);
                // prorate vis use in terms of vim eqivalent
                if (MagicArts.IsForm(art))
                {
                    visUse *= 2;
                }
                if (MagicArts.IsTechnique(art))
                {
                    visUse *= 4;
                }

                if (_visStock[art] + Covenant.GetVis(art) >= visUse)
                {
                    double desire = RateSeasonalExperienceGain(art, _preferences[new Preference(PreferenceType.Vis, null)]) - visUse;
                    if (desire > start.Desire)
                    {
                        start = new VisStudying(art, desire);
                    }
                }
            }
            return start;
        }
        #endregion

        #region Magic Functions
        public double GetCastingTotal(Ability technique, Ability form)
        {
            double techValue = Arts.GetAbility(technique).GetValue();
            double formValue = Arts.GetAbility(form).GetValue();
            return techValue + formValue + Stamina.Value;
        }

        public double GetLabTotal(Ability technique, Ability form, Activity activity)
        {
            double magicTheory = GetAbility(_magicAbility).GetValue();
            double techValue = Arts.GetAbility(technique).GetValue();
            double formValue = Arts.GetAbility(form).GetValue();
            double labTotal =  magicTheory + techValue + formValue + Intelligence.Value;
            if (_covenant != null)
            {
                labTotal += _covenant.Aura;

                if (_laboratory != null)
                {
                    labTotal += _laboratory.GetModifier(technique, form, activity);
                }
            }

            //TODO: foci
            //TODO: lab assistant
            //TODO: familiar
            return labTotal;
        }

        protected void CheckTwilight()
        {
        }

        public void ExtractVis()
        {
            // add vis to personal inventory or covenant inventory
            _visStock[MagicArts.Vim] += GetLabTotal(MagicArts.Creo, MagicArts.Vim, Activity.DistillVis) / 10;
            
            // grant exposure experience
            GetAbility(_preferredExtractionAbility).AddExperience(2);
        }

        public double UseVis(Ability visType, double amount)
        {
            if (!MagicArts.IsArt(visType))
            {
                throw new ArgumentException("Only magic arts have vis!");
            }
            if (_visStock[visType] + Covenant.GetVis(visType) < amount)
            {
                throw new ArgumentException("Insufficient vis available!");
            }
            if (amount > _visStock[visType])
            {
                amount -= _visStock[visType];
                Covenant.RemoveVis(visType, amount);
                return 0;
            }
            else
            {
                _visStock[visType] -= amount;
                return _visStock[visType];
            }
        }

        public void BuildLaboratory()
        {
            // TODO: flesh out laboratory specialization
            _laboratory = new Laboratory(this, 0);
        }

        public void RefineLaboratory()
        {
            if (_laboratory == null)
            {
                throw new NullReferenceException("The mage has no laboratory!");
            }
            if(GetAbility(_magicAbility).GetValue() - 4 < _laboratory.Refinement)
            {
                throw new ArgumentOutOfRangeException("The mage's magical understanding is not high enough to refine this laboratory any further.");
            }
            _laboratory.Refine();
        }

        public void AddFeatureToLaboratory(Feature feature)
        {
            if (_laboratory == null)
            {
                throw new NullReferenceException("The mage has no laboratory!");
            }

        }
        #endregion
    }
}
