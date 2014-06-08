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
	public partial class Magus : Character
	{
        private Ability _magicAbility;
        private Covenant _covenant;
        private Laboratory _laboratory;
        public List<Spell> SpellList { get; private set; }
        private Spell _partialSpell;
        private double _partialSpellProgress;
        private Dictionary<Ability, double> _visStock;
        private List<Ability> _extractionAbilities;
        private List<Ability> _magicSearchAbilities;
        private Magus _apprentice;

        public Houses House { get; set; }

        public Covenant Covenant
        {
            get
            {
                return _covenant;
            }
        }
        
        public Arts Arts { get; private set; }

        public Laboratory Laboratory
        {
            get { return _laboratory;}
        }

        #region Initialization Functions
        public Magus(Ability magicAbility, Ability writingLanguage, Ability writingAbility, Ability areaAbility)
            : base(writingLanguage, writingAbility, areaAbility)
        {
            _magicAbility = magicAbility;
            Arts = new Arts();
            _covenant = null;
            _laboratory = null;
            _visStock = new Dictionary<Ability, double>();
            SpellList = new List<Spell>();
            _partialSpell = null;
            _partialSpellProgress = 0;
            foreach (Ability art in MagicArts.GetEnumerator())
            {
                _visStock[art] = 0;
            }

            InitializeExtractionAbilities();
            InitializeMagicSearchAbilities();

        }

        private void InitializeExtractionAbilities()
        {
            _extractionAbilities = new List<Ability>(3);
            _extractionAbilities.Add(_magicAbility);
            _extractionAbilities.Add(MagicArts.Creo);
            _extractionAbilities.Add(MagicArts.Vim);
        }

        private void InitializeMagicSearchAbilities()
        {
            _magicSearchAbilities = new List<Ability>(3);
            _magicSearchAbilities.Add(_areaAbility);
            _magicSearchAbilities.Add(MagicArts.Intellego);
            _magicSearchAbilities.Add(MagicArts.Vim);
        }
        #endregion

        #region Ability Functions
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
                        Quality = GetAttribute(AttributeType.Communication).Value + 6,
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
                    if (qualityAdd > (GetAttribute(AttributeType.Communication).Value + 6))
                    {
                        qualityAdd = GetAttribute(AttributeType.Communication).Value + 6;
                    }

                    Summa s = new Summa
                    {
                        Quality = GetAttribute(AttributeType.Communication).Value + 6 + qualityAdd,
                        Level = (ability.GetValue() / 2.0) - qualityAdd,
                        Topic = ability.Ability
                    };
                    double seasonsNeeded = s.GetWritingPointsNeeded() / (GetAttribute(AttributeType.Communication).Value + GetAbility(_writingAbility).GetValue());
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
            double distillVisRate = GetLabTotal(MagicArtPairs.CrVi, Activity.DistillVis) / 10.0;
            if (MagicArts.IsTechnique(ability))
            {
                distillVisRate /= 4;
            }
            else if (MagicArts.IsForm(ability) && ability != MagicArts.Vim)
            {
                distillVisRate /= 2;
            }

            CharacterAbilityBase charAbility = GetAbility(ability);
            double visUsedPerStudySeason = 0.5 + (charAbility.GetValue() / 10.0);
            // the gain per season depends on how the character views vis
            double visNeeded = gain * visUsedPerStudySeason;
            // compare to the number of seasons we would need to extract the vis
            // plus the number of seasons we would need to study the extracted vis
            // this effectively means that a gain's base value is twice its vis cost
            double extractTime = visNeeded / distillVisRate;
            // exposure should get rated according to the visUse of the preferred exposure choice
            // rather than the visUse of the base ability
            double extractVisUsePer = (GetAbility(Abilities.MagicTheory).GetValue() / 10.0) + 0.5;
            double visValueOfExposure = extractTime * 2 * extractVisUsePer;
            return (2 * visNeeded) - visValueOfExposure;
        }
        #endregion

        #region Magic Functions
        public double GetCastingTotal(ArtPair artPair)
        {
            double techValue = Arts.GetAbility(artPair.Technique).GetValue();
            double formValue = Arts.GetAbility(artPair.Form).GetValue();
            return techValue + formValue + GetAttribute(AttributeType.Stamina).Value;
        }

        protected void CheckTwilight()
        {
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
        #endregion

        #region Lab Activities
        public double GetLabTotal(ArtPair artPair, Activity activity)
        {
            double magicTheory = GetAbility(_magicAbility).GetValue();
            double techValue = Arts.GetAbility(artPair.Technique).GetValue();
            double formValue = Arts.GetAbility(artPair.Form).GetValue();
            double labTotal = magicTheory + techValue + formValue + GetAttribute(AttributeType.Intelligence).Value;
            if (_covenant != null)
            {
                labTotal += _covenant.Aura;

                if (_laboratory != null)
                {
                    labTotal += _laboratory.GetModifier(artPair, activity);
                }
            }

            //TODO: foci
            //TODO: lab assistant
            //TODO: familiar
            return labTotal;
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
            if (GetAbility(_magicAbility).GetValue() - 4 < _laboratory.Refinement)
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
            // TODO: Implement
        }

        public void ExtractVis(Ability exposureAbility)
        {
            // add vis to personal inventory or covenant inventory
            _visStock[MagicArts.Vim] += GetLabTotal(MagicArtPairs.CrVi, Activity.DistillVis) / 10;

            // grant exposure experience
            GetAbility(exposureAbility).AddExperience(2);
        }

        public void InventSpell(Spell spell)
        {
            // TODO: multiple spells in a season
            // TODO: foci
            // TODO: Working from Lab Text
            double labTotal = GetLabTotal(spell.BaseArts, Activity.InventSpells);
            if(spell == _partialSpell)
            {
                // continue previous spell work
                _partialSpellProgress += labTotal - spell.Level;
                if (_partialSpellProgress >= _partialSpell.Level)
                {
                    SpellList.Add(_partialSpell);
                    _partialSpell = null;
                    _partialSpellProgress = 0;
                }
            }
            if(labTotal <= spell.Level)
            {
                throw new ArgumentException("This mage cannot invent this spell!");
            }
            else if (labTotal >= spell.Level * 2)
            {
                SpellList.Add(spell);
                _partialSpell = null;
                _partialSpellProgress = 0;
            }
            else
            {
                _partialSpell = spell;
                _partialSpellProgress = labTotal - spell.Level;
            }
        }
        #endregion

        #region Apprentice Functions
        public void TakeApprentice(Magus apprentice)
        {
            // TODO: what sort of error checking should go here?
            _apprentice = apprentice;
        }

        public void GauntletApprentice()
        {
            _apprentice.House = House;
            _apprentice = null;
        }
        #endregion
    }
}
