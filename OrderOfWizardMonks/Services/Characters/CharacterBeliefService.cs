using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Instances;
using WizardMonks.Models;
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
            character.Beliefs.TryGetValue(subject, out var profile);
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



        public static IEnumerable<Aura> GetKnownAuras(this Character character)
        {
            return character.Beliefs
                .Where(kvp => kvp.Value.Type == SubjectType.Aura && kvp.Value.Confidence > 0.5) // Only return "known" auras
                .Select(kvp => (Aura)kvp.Key);
        }

        public static IEnumerable<Aura> GetOwnedAuras(this Character character)
        {
            int charHash = character.Id.GetHashCode();
            bool hasCov = false;
            int covHash = 0;
            if (character is Magus mage && mage.Covenant != null)
            {
                hasCov = true;
                covHash = mage.Covenant.Id.GetHashCode();
            }
            return character.Beliefs
                .Where(kvp => kvp.Value.Type == SubjectType.Aura 
                    && kvp.Value.Confidence > 0.5 
                    && (kvp.Value.GetBeliefMagnitude(BeliefTopics.Owner.Name) == charHash || (hasCov && kvp.Value.GetBeliefMagnitude(BeliefTopics.Owner.Name) == covHash))) // Only return "known" auras
                .Select(kvp => (Aura)kvp.Key);
        }

        public static IEnumerable<HedgeMagus> GetKnownHedgeMagi(this Character character)
        {
            return character.Beliefs
                .Where(kvp => kvp.Value.Type == SubjectType.Character && kvp.Value.GetAllBeliefs().Any(b => b.Topic=="HedgeMage") && kvp.Value.Confidence > 0.5)
                .Select(kvp => (HedgeMagus)kvp.Key);
        }

        // Universal method to "learn of" something
        public static void AddOrUpdateKnowledge(this Character character, IBeliefSubject subject, BeliefProfile profile)
        {
            character.Beliefs[subject] = profile;
        }
    }
}
