using System;
using System.Linq;
using System.Collections.Generic;
using WizardMonks.Activities.MageActivities;
using WizardMonks.Economy;
using WizardMonks.Instances;
using WizardMonks.Models.Books;
using WizardMonks.Models.Spells;
using WizardMonks.Models.Characters;

namespace WizardMonks.Decisions.Conditions.Helpers
{
    /// <summary>
    /// This helper evaluates the profitability of writing a clean lab text for trade.
    /// It is motivated by a need for capital (vis) and only considers writing spells
    /// for which there is known demand from other magi.
    /// </summary>
    public class WriteLabTextHelper : AHelper
    {
        public WriteLabTextHelper(Magus mage, uint ageToCompleteBy, ushort conditionDepth, CalculateDesireFunc desireFunc)
            : base(mage, ageToCompleteBy, conditionDepth, desireFunc)
        {
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, Desires desires, IList<string> log)
        {
            if (_ageToCompleteBy <= _mage.SeasonalAge) return;

            Spell bestSpellToWrite = null;
            double maxNetValue = 0;

            // 1. Iterate through spells the magus knows
            foreach (var spell in _mage.SpellList)
            {
                // 2. Check for demand for this spell's base effect
                var potentialBuyers = GlobalEconomy.LabTextDesiresBySpellBase[spell.Base]?
                    .Where(d => d.SpellBase == spell.Base && d.Character != _mage)
                    .ToList();

                if (potentialBuyers == null || !potentialBuyers.Any()) continue;

                // 3. Find the most lucrative potential buyer
                double bestOffer = 0;
                foreach (var buyerDesire in potentialBuyers)
                {
                    if (buyerDesire.Character is Magus buyerMage)
                    {
                        double buyerValuation = buyerMage.RateLifetimeLabTextValue(new LabText { SpellContained = spell });
                        if (buyerValuation > bestOffer)
                        {
                            bestOffer = buyerValuation;
                        }
                    }
                }

                if (bestOffer <= 0) continue;

                // 4. Calculate profitability
                double seasonsToWrite = Math.Ceiling(spell.Level / _mage.GetLabTextWritingRate());
                double opportunityCost = seasonsToWrite * _mage.GetVisDistillationRate();
                double netValue = bestOffer - opportunityCost;

                if (netValue > maxNetValue)
                {
                    maxNetValue = netValue;
                    bestSpellToWrite = spell;
                }
            }

            // 5. If a profitable opportunity exists, add it to the considered actions
            if (bestSpellToWrite != null)
            {
                double effectiveDesire = _desireFunc(maxNetValue, _conditionDepth);
                effectiveDesire *= _mage.Personality.GetInverseDesireMultiplier(HexacoFacet.Modesty); // Similar to book writing

                log.Add($"Considering writing lab text for '{bestSpellToWrite.Name}' with net value {maxNetValue:F2}, worth {effectiveDesire:0.000}");

                var writeAction = new WriteLabTextActivity(bestSpellToWrite, Abilities.Latin, effectiveDesire);
                alreadyConsidered.Add(writeAction);
            }
        }
    }
}