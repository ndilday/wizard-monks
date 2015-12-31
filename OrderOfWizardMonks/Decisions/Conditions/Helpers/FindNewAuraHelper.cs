using System;
using System.Collections.Generic;

using WizardMonks.Instances;

namespace WizardMonks.Decisions.Conditions.Helpers
{
    class FindNewAuraHelper : AHelper
    {
        public FindNewAuraHelper(Magus mage, uint ageToCompleteBy, double desirePerPoint, ushort conditionDepth) :
            base(mage, ageToCompleteBy, desirePerPoint, conditionDepth)
        {
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log)
        {
            double currentAura = Mage.Covenant == null ? 0 : Mage.Covenant.Aura.Strength;
            double auraCount = Mage.KnownAuras.Count;

            double findAuraScore = CalculateFindAuraScore();
            double probOfBetter = 1 - (currentAura * currentAura * auraCount / (5 * findAuraScore));
            double maxAura = Math.Sqrt(5.0 * findAuraScore / auraCount);
            double averageGain = maxAura * probOfBetter / 2.0;

            if (averageGain > 0)
            {
                double desire = Desire * averageGain / ConditionDepth;
                log.Add("Finding a better aura to build a lab in worth " + desire.ToString("0.00"));
                alreadyConsidered.Add(new FindAura(Abilities.AreaLore, desire));
            }
            // consider the value of increasing find aura related scores
        }

        private double CalculateFindAuraScore()
        {
            double areaLore = Mage.GetAbility(Abilities.AreaLore).Value;
            areaLore += Mage.GetCastingTotal(MagicArtPairs.InVi) / 10;
            areaLore += Mage.GetAttribute(AttributeType.Perception).Value;
            return areaLore;
        }
    }
}
