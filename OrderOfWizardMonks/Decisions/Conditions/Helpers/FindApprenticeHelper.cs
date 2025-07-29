using System;
using System.Collections.Generic;
using WizardMonks.Activities.ExposingActivities;
using WizardMonks.Instances;

namespace WizardMonks.Decisions.Conditions.Helpers
{
    public class FindApprenticeHelper : AHelper
    {
        private const double EASE_FACTOR = 9.0;
        private readonly SpellBase _giftFindingBase;

        public FindApprenticeHelper(Magus mage, uint ageToCompleteBy, ushort conditionDepth, CalculateDesireFunc desireFunc)
            : base(mage, ageToCompleteBy, conditionDepth, desireFunc)
        {
            _giftFindingBase = SpellBases.GetSpellBaseForEffect(TechniqueEffects.Detect, FormEffects.Gift);
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, Desires desires, IList<string> log)
        {
            if (_ageToCompleteBy <= _mage.SeasonalAge) return;

            // Step 1: Consider the direct action of searching.
            // The value is the chance of success multiplied by the desire for the goal.
            double searchTotal = GetSearchTotal();
            // Average roll of a stress die is ~5.5.
            double chanceOfSuccess = Math.Max(0, (5.5 + searchTotal - EASE_FACTOR) / 10.0);

            if (chanceOfSuccess > 0)
            {
                double effectiveDesire = _desireFunc(chanceOfSuccess, _conditionDepth);
                log.Add($"Considering search for apprentice with {chanceOfSuccess:P1} chance of success, worth {effectiveDesire:0.000}");
                alreadyConsidered.Add(new FindApprenticeActivity(Abilities.FolkKen, effectiveDesire));
            }

            // Step 2: Consider actions to improve the search total if the deadline is not immediate.
            if (_conditionDepth < 5 && _ageToCompleteBy > _mage.SeasonalAge + 1)
            {
                // How much does a +1 to searchTotal increase our desire?
                // It increases the chance of success by 10%.
                var desireGainPerPoint = _desireFunc(0.1, (ushort)(_conditionDepth + 1));

                // Option A: Practice Folk Ken
                var practiceHelper = new PracticeHelper(Abilities.FolkKen, _mage, (uint)(_ageToCompleteBy - 1), (ushort)(_conditionDepth + 1), (gain, depth) => gain * desireGainPerPoint);
                practiceHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);

                // Option B: Read a book on Folk Ken
                var readingHelper = new ReadingHelper(Abilities.FolkKen, _mage, (uint)(_ageToCompleteBy - 1), (ushort)(_conditionDepth + 1), (gain, depth) => gain * desireGainPerPoint);
                readingHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);

                // Option C: Invent a Gift-finding spell.
                // A +5 level spell gives +1 to the search total.
                var spellHelper = new LearnSpellHelper(_mage, (uint)(_ageToCompleteBy - 1), (ushort)(_conditionDepth + 1), _giftFindingBase, (gain, depth) => (gain / 5.0) * desireGainPerPoint);
                spellHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);
            }
        }

        private double GetSearchTotal()
        {
            double total = 0;
            total += _mage.GetAbility(Abilities.FolkKen).Value;
            total += _mage.GetAttributeValue(AttributeType.Perception);
            total += _mage.GetAbility(Abilities.AreaLore).Value / 2.0;
            total += _mage.GetAbility(Abilities.Etiquette).Value / 2.0;

            Spell bestSpell = _mage.GetBestSpell(_giftFindingBase);
            if (bestSpell != null)
            {
                total += bestSpell.Level / 5.0;
            }
            else
            {
                total += _mage.GetSpontaneousCastingTotal(MagicArtPairs.InVi) / 5.0;
            }
            return total;
        }
    }
}
