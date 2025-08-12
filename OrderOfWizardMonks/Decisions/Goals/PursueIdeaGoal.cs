using System.Collections.Generic;
using WizardMonks.Decisions.Conditions.Helpers;
using WizardMonks.Instances;
using WizardMonks.Models;
using WizardMonks.Models.Ideas;
using WizardMonks.Models.Spells;

namespace WizardMonks.Decisions.Goals
{
    public class PursueIdeaGoal : AGoal
    {
        private readonly AIdea _idea;
        private Spell _targetSpell; // The concrete spell we decide to invent

        public PursueIdeaGoal(Magus magus, AIdea idea, uint? ageToCompleteBy = null)
            : base(magus, ageToCompleteBy, 0) // Desire is calculated dynamically
        {
            _idea = idea;
            Desire = CalculateDesire(magus);
        }

        private double CalculateDesire(Magus magus)
        {
            // Add value based on projected future benefits
            // Placeholder for reputation gain. This makes it forward-compatible.
            double reputationValue = 0; // e.g., ReputationSystem.EstimateGain(magus, _Idea);

            // Placeholder for tangible benefits, like the lab-heating spell example
            double utilityValue = 0; // e.g., UtilitySystem.EstimateValue(_Idea);

            double totalValue = reputationValue + utilityValue;

            // Apply personality modifiers
            totalValue *= magus.Personality.GetDesireMultiplier(HexacoFacet.Creativity);
            totalValue *= magus.Personality.GetDesireMultiplier(HexacoFacet.Inquisitiveness);

            return totalValue;
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, Desires desires, IList<string> log)
        {
            if (_completed) return;

            var magus = (Magus)Character;
            if (_idea is SpellIdea spellIdea)
            {
                // If we haven't defined a target spell yet, create one
                if (_targetSpell == null)
                {
                    // For now, invent a simple, generic spell based on the inspired Arts.
                    // This logic can become more sophisticated.
                    _targetSpell = new Spell(
                        EffectRanges.Touch, EffectDurations.Sun, EffectTargets.Individual,
                        new SpellBase(TechniqueEffects.Manipulate, FormEffects.Animal, SpellArts.Rego | SpellArts.Animal, spellIdea.Arts, SpellTag.Utility, 2, "Inspired Spell"),
                        0, false, $"Invention from {magus.Name}'s Insight");
                }

                // Check if the spell is already known
                if (magus.SpellList.Contains(_targetSpell))
                {
                    _completed = true;
                    return;
                }

                // Use a helper to plan the invention of the target spell
                var spellHelper = new LearnSpellHelper(magus, magus.SeasonalAge, 1, _targetSpell.Base,
                    (gain, depth) => this.Desire * (gain / _targetSpell.Level));
                spellHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);
            }
        }
    }
}