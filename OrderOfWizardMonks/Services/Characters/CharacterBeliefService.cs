using System;
using WizardMonks.Instances;
using WizardMonks.Models.Beliefs;
using WizardMonks.Models.Characters;

namespace WizardMonks.Services.Characters
{
    public static class CharacterBeliefService
    {
        // These weights define the base value of different types of knowledge for prestige purposes.
        // Arts are the most prestigious, followed by core Attributes, then general Abilities.
        private const double ART_PRESTIGE_WEIGHT = 1.0;
        private const double ATTRIBUTE_PRESTIGE_WEIGHT = 0.75;
        private const double ABILITY_PRESTIGE_WEIGHT = 0.5;
        private const double PERSONALITY_PRESTIGE_WEIGHT = 0.25; // Less about skill, more about character.

        public static BeliefProfile GetBeliefProfile(this Character character, IBeliefSubject subject)
        {
            if (!character.Beliefs.TryGetValue(subject.Id, out var profile))
            {
                profile = new();
                character.Beliefs[subject.Id] = profile;
            }
            return profile;
        }

        /// <summary>
        /// Calculates the prestige value of a single Belief from this magus's perspective.
        /// This is the core, centralized valuation function.
        /// </summary>
        /// <param name="belief">The belief to evaluate.</param>
        /// <returns>A score representing the belief's contribution to prestige.</returns>
        public static double CalculateBeliefValue(this Character character, Belief belief)
        {
            double baseWeight = 0;
            double focusMultiplier = 1.0; // Default: no special focus.

            // Step 1: Find the corresponding Ability to determine its type and check for focus.
            Abilities.AbilityDictionary.TryGetValue(belief.Topic, out Ability matchingAbility);

            if (matchingAbility != null)
            {
                // Step 1a: Determine the base weight by AbilityType.
                baseWeight = matchingAbility.AbilityType == AbilityType.Art ? ART_PRESTIGE_WEIGHT : ABILITY_PRESTIGE_WEIGHT;

                // Step 1b: Check if this Ability is one of the magus's personal focuses.
                if (character.ReputationFocuses.TryGetValue(matchingAbility.AbilityName, out double multiplier))
                {
                    focusMultiplier = multiplier;
                }
            }
            else if (Enum.TryParse<AttributeType>(belief.Topic, out _))
            {
                baseWeight = ATTRIBUTE_PRESTIGE_WEIGHT;
            }
            else if (Enum.TryParse<HexacoFacet>(belief.Topic, out _))
            {
                baseWeight = PERSONALITY_PRESTIGE_WEIGHT;
            }

            // The final value incorporates the belief's strength, its general importance (weight),
            // and the magus's personal investment in the topic (focusMultiplier).
            return belief.Magnitude * baseWeight * focusMultiplier;
        }
    }
}
