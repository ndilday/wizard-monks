using System;
using System.Collections.Generic;
using System.Linq;

using WizardMonks.Instances;

namespace WizardMonks
{
    public abstract class BaseHelper
    {
        public double BaseDesire { get; private set; }
        public uint? BaseDueDate { get; private set; }
        public byte BaseTier { get; private set; }
        protected List<Ability> _abilities;

        public BaseHelper(List<Ability> abilities, double baseDesire, byte tier, uint? baseDueDate = null)
        {
            _abilities = abilities;
            BaseDesire = baseDesire;
            BaseTier = tier;
            BaseDueDate = baseDueDate;
        }

        protected BaseHelper(double baseDesire, byte tier, uint? baseDueDate = null)
        {
            BaseDesire = baseDesire;
            BaseTier = tier;
            BaseDueDate = baseDueDate;
        }

        protected abstract double CalculateDesire(double increase);

        public virtual void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log)
        {
            if (BaseDueDate != null && BaseDueDate == 0)
            {
                return;
            }
            IEnumerable<IBook> readableBooks = character.ReadableBooks;
            foreach (Ability ability in _abilities)
            {
                // Handle Reading
                CharacterAbilityBase charAbility = character.GetAbility(ability);
                var topicalBooks = readableBooks.Where(b => b.Topic == ability);
                HandleReading(character, alreadyConsidered, topicalBooks);

                // Handle Practice
                if (!MagicArts.IsArt(ability))
                {
                    HandlePractice(character, alreadyConsidered, log, ability);
                }
                else if (character.GetType() == typeof(Magus))
                {
                    Magus mage = (Magus)character;
                    HandleVisUse(mage, charAbility, alreadyConsidered, log);
                }

                // TODO: Learning By Training
                // TODO: Learning by Teaching
            }
        }

        protected void HandlePractice(Character character, ConsideredActions alreadyConsidered, IList<string> log, Ability ability)
        {
            // Handle Practice
            // For now, assume 4pt practice on everything
            // after lots of math, the right equation is:
            // Desire * (labTotal + increase)/(labTotal + increase + level)
            double increase = character.GetAbility(ability).GetValueGain(4);
            double desire = CalculateDesire(increase) / (BaseTier + 1);
            if (BaseDueDate != null)
            {
                desire /= (double)BaseDueDate;
            }
            Practice practiceAction = new Practice(ability, desire);
            log.Add("Practicing " + ability.AbilityName + " worth " + (desire).ToString("0.00"));
            alreadyConsidered.Add(practiceAction);
        }

        protected void HandleReading(Character character, ConsideredActions alreadyConsidered, IEnumerable<IBook> topicalBooks)
        {
            if (topicalBooks.Any())
            {
                var bestBook =
                    (from book in topicalBooks
                     orderby character.GetBookLevelGain(book),
                             book.Level ascending
                     select book).First();
                double increase = character.GetBookLevelGain(bestBook);
                double desire = CalculateDesire(increase) / (BaseTier + 1);
                if (BaseDueDate != null)
                {
                    desire /= (double)BaseDueDate;
                }

                Reading readingAction = new Reading(bestBook, desire);
                alreadyConsidered.Add(readingAction);
            }
        }

        protected void HandleVisUse(Magus mage, CharacterAbilityBase magicArt, ConsideredActions alreadyConsidered, IList<string> log)
        {
            // see if the mage has enough vis of this type
            double stockpile = mage.GetVisCount(magicArt.Ability);
            double visNeed = 0.5 + (magicArt.Value / 10.0);
            double increase = magicArt.GetValueGain(6);
            double baseDesire = CalculateDesire(increase) / (BaseTier + 1);
            if (BaseDueDate != null)
            {
                baseDesire /= (double)BaseDueDate;
            }
            // if so, assume vis will return an average of 6XP
            if (stockpile > visNeed)
            {
                log.Add("Studying vis for " + magicArt.Ability.AbilityName + " worth " + baseDesire.ToString("0.00"));
                VisStudying visStudy = new VisStudying(magicArt.Ability, baseDesire);
                alreadyConsidered.Add(visStudy);
                // TODO: how do we decrement the cost of the vis?
            }
            // putting a limit here to how far the circular loop will go
            else if (baseDesire >= 0.01)
            {
                List<Ability> visType = new List<Ability>();
                visType.Add(magicArt.Ability);
                VisCondition visCondition =
                    new VisCondition(visType, visNeed - stockpile, baseDesire, (byte)(BaseTier + 1), BaseDueDate == null ? null : BaseDueDate - 1);
                visCondition.ModifyActionList(mage, alreadyConsidered, log);
            }
        }
    }

    /// <summary>
    /// This is a specialized goal-like object used to figure out
    /// the value of increasing abilities relative to a certain lab total
    /// </summary>
    class IncreaseAbilitiesForVisHelper : BaseHelper
    {
        protected double _currentTotal;
        public IncreaseAbilitiesForVisHelper(List<Ability> abilities, double desire, double currentTotal, byte tier, uint? dueDate = null)
            : base(abilities, desire, tier, dueDate)
        {
            _currentTotal = currentTotal;
        }

        public IncreaseAbilitiesForVisHelper(double desire, double currentTotal, byte tier, uint? dueDate = null)
            : base(desire, tier, dueDate)
        {
            _currentTotal = currentTotal;
        }

        protected override double CalculateDesire(double increase)
        {
            if (_currentTotal == 0) return BaseDesire;
            return (increase + _currentTotal) * BaseDesire / (2 * _currentTotal);
        }
    }

    class IncreseAbilitiesForVisSearchHelper : IncreaseAbilitiesForVisHelper
    {
        protected double _currentML;
        protected double _aura;
        private double _currentGain;

        public IncreseAbilitiesForVisSearchHelper(double desire, double currentVis, double currentML, double aura, byte tier, uint? dueDate = null)
            : base(desire, currentVis, tier, dueDate)
        {
            _abilities = new List<Ability>();
            _abilities.Add(Abilities.MagicLore);
            _abilities.Add(MagicArts.Intellego);
            _abilities.Add(MagicArts.Vim);
            _currentML = currentML;
            _aura = aura;
            _currentGain = GetAverageVisSize(_currentML);
        }

        protected override double CalculateDesire(double increase)
        {
            return GetAverageVisSize(_currentML + increase) - _currentGain;
        }

        private double GetAverageVisSize(double magicLoreTotal)
        {
            double currentRoll = Math.Pow(_currentTotal, 2) / (_currentML * _aura);
            double multiplier = Math.Sqrt(_currentML * _aura) * 2 / 3;
            return (11.180339887498948482045868343656 - Math.Pow(currentRoll, 1.5)) * multiplier / 5.0;
        }
    }

    /// <summary>
    /// IncreaseLabTotalHelper assumes that a laboratory has already been built
    /// </summary>
    class IncreaseLabTotalHelper : BaseHelper
    {
        public IncreaseLabTotalHelper(ArtPair artPair, double desirePerPoint, byte tier, uint? dueDate = null)
            : base(desirePerPoint, tier, dueDate)
        {
            List<Ability> abilities = new List<Ability>();
            abilities.Add(Abilities.MagicTheory);
            abilities.Add(artPair.Technique);
            abilities.Add(artPair.Form);
            _abilities = abilities;
        }

        protected override double CalculateDesire(double increase)
        {
            return BaseDesire * increase;
        }

        public override void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log)
        {
            if (BaseDueDate != null && BaseDueDate == 0)
            {
                return;
            }
            IEnumerable<IBook> readableBooks = character.ReadableBooks;
            foreach (Ability ability in _abilities)
            {
                // Handle Reading
                CharacterAbilityBase charAbility = character.GetAbility(ability);
                var topicalBooks = readableBooks.Where(b => b.Topic == ability);
                HandleReading(character, alreadyConsidered, topicalBooks);

                // Handle Practice
                if (!MagicArts.IsArt(ability))
                {
                    HandlePractice(character, alreadyConsidered, log, ability);
                }
                else if (character.GetType() == typeof(Magus))
                {
                    Magus mage = (Magus)character;
                    HandleVisUse(mage, charAbility, alreadyConsidered, log);
                    HandleAuras(mage, alreadyConsidered, log);
                }
            }
        }

        private void HandleAuras(Magus mage, ConsideredActions alreadyConsidered, IList<string> log)
        {
            double greatestAura = mage.KnownAuras.Select(a => a.Strength).Max();
            double currentAura = mage.Covenant.Aura.Strength;
            if (greatestAura > currentAura)
            {
                Aura bestAura = mage.KnownAuras.Where(a => a.Strength == greatestAura).First();
                if (mage.Laboratory == null)
                {
                    // just move the covenant right now
                    log.Add("Since no lab built, moving to better aura.");
                    mage.FoundCovenant(bestAura);
                }
                else
                {
                    // how do we want to rate the value of moving and having to build a new lab?
                    // it seems like basically the same as any other single-month activity
                    double gain = greatestAura - currentAura;
                    double dueDateDesire = CalculateDesire(gain) / (BaseTier + 1);
                    if (BaseDueDate != null)
                    {
                        if (BaseDueDate == 0)
                        {
                            return;
                        }
                        dueDateDesire /= (double)(BaseDueDate - 1);
                    }
                    log.Add("Moving to new aura (to boost lab total) worth " + dueDateDesire.ToString("0.00"));
                    alreadyConsidered.Add(new BuildLaboratory(bestAura, Abilities.MagicTheory, dueDateDesire));
                }
            }
            else
            {
                double auraCount = mage.KnownAuras.Count;
                double areaLore = mage.GetAbility(Abilities.AreaLore).Value;
                areaLore += mage.GetCastingTotal(MagicArtPairs.InVi) / 10;
                areaLore += mage.GetAttribute(AttributeType.Perception).Value;
                double probOfBetter = 1 - (currentAura * currentAura * auraCount / (5 * areaLore));
                double maxAura = Math.Sqrt(5.0 * areaLore / auraCount);
                double averageGain = maxAura * probOfBetter / 2.0;

                double dueDateDesire = CalculateDesire(averageGain) / (BaseTier + 1);
                if (BaseDueDate != null)
                {
                    if (BaseDueDate == 0)
                    {
                        return;
                    }
                    dueDateDesire /= (double)(BaseDueDate - 1);
                }

                if (probOfBetter > 0)
                {
                    log.Add("Finding a better aura to build a lab in worth " + dueDateDesire.ToString("0.00"));
                    alreadyConsidered.Add(new FindAura(Abilities.AreaLore, dueDateDesire));
                }
            }
        }
    }

    class LongevityRitualAbilitiesHelper : IncreaseLabTotalHelper
    {
        public LongevityRitualAbilitiesHelper(double desirePerPoint, byte tier, uint? dueDate = null)
            : base(MagicArtPairs.CrVi, desirePerPoint, tier, dueDate) { }

        protected override double CalculateDesire(double increase)
        {
            // for purposes of the longevity ritual, each season that we tarry
            // means that we'll pay slightly more vis
            // when we finally get around to the actual LR
            return (increase * BaseDesire) - (BaseTier * 0.05);
        }
    }

    /// <summary>
    /// This is a specialized goal-like object used to figure out 
    /// the value of increasing abilities relative to a certain lab spell/effect.
    /// </summary>
    class IncreaseLabTotalVersusEffectLevelHelper : IncreaseLabTotalHelper
    {
        private double _currentGain;
        private double _effectLevel;
        public IncreaseLabTotalVersusEffectLevelHelper(ArtPair artPair, double desire, double currentRate, double level, byte tier, uint? dueDate = null)
            : base(artPair, desire, tier, dueDate)
        {
            _currentGain = currentRate;
            _effectLevel = level;
        }

        protected override double CalculateDesire(double increase)
        {
            return (_currentGain + increase) * BaseDesire / ((_currentGain + increase) + _effectLevel);
        }
    }

    class IncreaseAuraHelper
    {
        public uint? DueDate { get; private set; }
        public byte Tier { get; private set; }
        private double _desirePerPoint;

        public IncreaseAuraHelper(double desirePerPoint, byte tier, uint? dueDate = null)
        {
            DueDate = dueDate;
            Tier = tier;
            _desirePerPoint = desirePerPoint;
        }

        public void ModifyActionList(Magus mage, ConsideredActions alreadyConsidered, IList<string> log)
        {
            double dueDateDesire = _desirePerPoint / (Tier + 1);
            if (DueDate != null)
            {
                if (DueDate == 0)
                {
                    log.Add("Has Covenant Condition failed");
                    return;
                }
                dueDateDesire /= (double)DueDate;
            }

            double currentAura = mage.Covenant.Aura.Strength;
            double auraCount = mage.KnownAuras.Count;
            double areaLore = mage.GetAbility(Abilities.AreaLore).Value;
            areaLore += mage.GetCastingTotal(MagicArtPairs.InVi) / 10;
            areaLore += mage.GetAttribute(AttributeType.Perception).Value;
            double probOfBetter = 1 - (currentAura * currentAura * auraCount / (5 * areaLore));
            double maxAura = Math.Sqrt(5.0 * areaLore / auraCount);
            double averageGain = maxAura * probOfBetter / 2.0;

            if (probOfBetter > 0)
            {
                double desire = dueDateDesire * averageGain;
                log.Add("Finding a better aura to build a lab in worth " + desire.ToString("0.00"));
                alreadyConsidered.Add(new FindAura(Abilities.AreaLore, desire));
            }
        }
    }
}
