using System;
using System.Collections.Generic;
using System.Linq;

using WizardMonks.Instances;

namespace WizardMonks.Decisions.Conditions.Helpers
{
    class FindNewAuraHelper : AHelper
    {
        private bool _allowVimVisUse;

        public FindNewAuraHelper(Magus mage, uint ageToCompleteBy, double desirePerPoint, ushort conditionDepth, bool allowVimVisUse) :
            base(mage, ageToCompleteBy, desirePerPoint, conditionDepth)
        {
            _allowVimVisUse = allowVimVisUse;
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log)
        {
            double currentAura = Mage.Covenant == null ? 0 : Mage.Covenant.Aura.Strength;
            int auraCount = Mage.KnownAuras.Count;

            // for now
            double findAuraScore = CalculateFindAuraScore();
            double probOfBetter = 1 - (currentAura * currentAura * auraCount / (5 * findAuraScore));
            double maxAura = Math.Sqrt(5.0 * findAuraScore / auraCount);
            double averageGain = maxAura * probOfBetter / 2.0;
            double desire = Desire * averageGain / ConditionDepth;

            if (desire > 0.01)
            {
                
                log.Add("Finding a better aura to build a lab in worth " + desire.ToString("0.00"));
                alreadyConsidered.Add(new FindAura(Abilities.AreaLore, desire));

                // consider the value of increasing find aura related scores
                //practice area lore
                double alPracticeDesire = CalculateScoreGainDesire(4, findAuraScore, currentAura, auraCount);
                if(alPracticeDesire > 0.01)
                {
                    log.Add("Practicing Area Lore before finding a new aura worth " + desire.ToString("0.00"));
                    alreadyConsidered.Add(new Practice(Abilities.AreaLore, alPracticeDesire));
                }

                // read area lore
                var bestBook = Mage.GetBestBookToRead(Abilities.AreaLore);
                if (bestBook != null)
                {
                    double gain = Mage.GetBookLevelGain(bestBook);
                    double effectiveDesire = CalculateScoreGainDesire(gain, findAuraScore, currentAura, auraCount);
                    if (effectiveDesire > 0.01)
                    {
                        log.Add("Reading " + bestBook.Title + " before finding a new aura worth " + (effectiveDesire).ToString("0.00"));
                        Reading readingAction = new Reading(bestBook, effectiveDesire);
                        alreadyConsidered.Add(readingAction);
                    }
                }

                // TODO: consider value of increasing InVi casting total
                //CastingTotalIncreaseHelper inViHelper = new CastingTotalIncreaseHelper(Mage, AgeToCompleteBy - 1, Desire / 10, (ushort)(ConditionDepth + 1), MagicArtPairs.InVi, _allowVimVisUse);

            }
        }

        private double CalculateFindAuraScore()
        {
            double areaLore = Mage.GetAbility(Abilities.AreaLore).Value;
            areaLore += Mage.GetCastingTotal(MagicArtPairs.InVi) / 10;
            areaLore += Mage.GetAttribute(AttributeType.Perception).Value;
            return areaLore;
        }

        private double CalculateScoreGainDesire(double gain, double findAuraScore, double currentAura, int auraCount)
        {
            double practicedAreaLoreScore = findAuraScore + 4;
            double probOfBetter = 1 - (currentAura * currentAura * auraCount / (5 * findAuraScore));
            double maxAura = Math.Sqrt(5.0 * findAuraScore / auraCount);
            double averageGain = maxAura * probOfBetter / 2.0;
            return Desire * averageGain / (ConditionDepth + 1);
            
        }
    }
}
