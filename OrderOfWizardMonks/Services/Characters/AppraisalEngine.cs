using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Decisions;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Events;

namespace WizardMonks.Services.Characters
{
    /// <summary>
    /// Default implementation of the OCC appraisal model.
    ///
    /// Appraises events along three OCC axes:
    ///   1. Goal relevance  → Joy / Distress / Hope / Fear
    ///   2. Attribution     → Pride / Shame / Admiration / Reproach / Gratitude / Anger
    ///   3. Standard conformity → Envy / Gloating
    ///
    /// Emotion intensity is determined by the event's relevance to the character's active
    /// goals and beliefs — how much is at stake, how central the affected goal is, and how
    /// directly the event bears on the character's self-model. Personality does not modulate
    /// intensity. Decay rates are fixed per emotion type and are not personality variables.
    /// </summary>
    public sealed class AppraisalEngine : IAppraisalEngine
    {
        /// <summary>
        /// Events whose computed importance falls below this threshold produce no MemoryEntry.
        /// </summary>
        private const float MinImportanceThreshold = 0.05f;

        /// <summary>
        /// Per-emotion-type base decay rates. These reflect the natural persistence
        /// of each emotion type and are tunable configuration, not personality variables.
        /// Values represent the fraction of intensity lost per tick.
        /// </summary>
        private static readonly IReadOnlyDictionary<EmotionType, float> BaseDecayRates =
            new Dictionary<EmotionType, float>
            {
                // Prospect emotions decay quickly once the anticipated event resolves or recedes.
                [EmotionType.Hope] = 0.30f,
                [EmotionType.Fear] = 0.20f,  // Fear lingers longer than hope

                // Well-being emotions decay at a moderate rate.
                [EmotionType.Joy] = 0.30f,
                [EmotionType.Distress] = 0.20f,  // Distress lingers longer than joy

                // Self-attribution emotions are tied to identity and decay slowly.
                [EmotionType.Pride] = 0.15f,
                [EmotionType.Shame] = 0.10f,  // Shame is the most persistent

                // Other-attribution emotions decay at a moderate rate.
                [EmotionType.Admiration] = 0.25f,
                [EmotionType.Reproach] = 0.20f,

                // Interaction emotions vary: Gratitude fades; Anger lingers.
                [EmotionType.Gratitude] = 0.25f,
                [EmotionType.Anger] = 0.15f,

                // Comparison emotions decay moderately.
                [EmotionType.Envy] = 0.20f,
                [EmotionType.Gloating] = 0.30f,
            };

        /// <summary>
        /// Base intensity applied for attribution-axis tokens (Pride, Shame, Admiration,
        /// Reproach, Gratitude, Anger). The attribution axis does not have a goal-relevance
        /// score to drive intensity, so a fixed base is used. Tunable.
        /// </summary>
        private const float AttributionBaseIntensity = 0.4f;

        /// <summary>
        /// Base intensity for standard-conformity tokens (Envy, Gloating).
        /// These are weaker by default — comparison is a secondary appraisal.
        /// </summary>
        private const float ConformityBaseIntensity = 0.2f;

        // No constructor — no dependencies.

        /// <inheritdoc/>
        public MemoryEntry? Appraise(
            Character character,
            WorldEvent worldEvent,
            IReadOnlyList<Intention> activeIntentions,
            EmotionLedger currentLedger)
        {
            var (goalTokens, relevantIntentionIds) =
                EvaluateGoalRelevance(character, worldEvent, activeIntentions);

            var attributionTokens = EvaluateAttribution(character, worldEvent);
            var conformityTokens = EvaluateStandardConformity(character, worldEvent);

            var allTokens = goalTokens
                .Concat(attributionTokens)
                .Concat(conformityTokens)
                .ToList();

            if (allTokens.Count == 0) return null;

            float importanceWeight = allTokens.Max(t => t.Intensity);
            if (relevantIntentionIds.Count > 0)
            {
                importanceWeight = Math.Clamp(
                    importanceWeight * (1f + 0.1f * relevantIntentionIds.Count),
                    0f, 1f);
            }

            if (importanceWeight < MinImportanceThreshold) return null;

            return new MemoryEntry(
                tick: worldEvent.Tick,
                sourceEvent: worldEvent,
                importanceWeight: importanceWeight,
                relevantIntentionIds: relevantIntentionIds,
                emotionSnapshot: allTokens.Select(t => t.Snapshot()).ToList());
        }

        // -----------------------------------------------------------------
        // Axis 1: Goal Relevance
        // Intensity is the goal-relevance score — how central this event is to
        // what the character is currently trying to accomplish.
        // -----------------------------------------------------------------
        private static (List<EmotionToken> tokens, IReadOnlyList<Guid> intentionIds)
            EvaluateGoalRelevance(
                Character character,
                WorldEvent worldEvent,
                IReadOnlyList<Intention> activeIntentions)
        {
            var tokens = new List<EmotionToken>();
            var relevantIds = new List<Guid>();

            bool isProspective = worldEvent.Category is WorldEventCategory.RecruitmentAttempted;

            foreach (var intention in activeIntentions)
            {
                float relevance = ComputeGoalRelevance(intention, worldEvent);
                if (relevance < 0.05f) continue;

                relevantIds.Add(intention.Id);

                var type = isProspective
                    ? (worldEvent.IsPositiveOutcome == true ? EmotionType.Hope : EmotionType.Fear)
                    : (worldEvent.IsPositiveOutcome == true ? EmotionType.Joy : EmotionType.Distress);

                tokens.Add(new EmotionToken(type, relevance, DecayRate(type), worldEvent.Tick));
            }

            return (tokens, relevantIds);
        }

        // -----------------------------------------------------------------
        // Axis 2: Attribution
        // Fixed base intensity — the attribution axis is not goal-relevance driven.
        // -----------------------------------------------------------------
        private static List<EmotionToken> EvaluateAttribution(
            Character character,
            WorldEvent worldEvent)
        {
            var tokens = new List<EmotionToken>();
            bool outcome = worldEvent.IsPositiveOutcome ?? false;

            if (worldEvent.Subject?.Id == character.Id)
            {
                // Self-attribution
                var type = outcome ? EmotionType.Pride : EmotionType.Shame;
                tokens.Add(new EmotionToken(type, AttributionBaseIntensity, DecayRate(type), worldEvent.Tick));
            }
            else if (worldEvent.Subject != null && worldEvent.Participants.Any(p => p.Id == character.Id))
            {
                // Other-attribution: was this action directed at the observing character?
                bool directedAtSelf = worldEvent.Category is
                    WorldEventCategory.BookReceived or
                    WorldEventCategory.LabTextReceived;

                EmotionType type;
                float intensity;

                if (directedAtSelf)
                {
                    type = outcome ? EmotionType.Gratitude : EmotionType.Anger;
                    intensity = AttributionBaseIntensity;
                }
                else
                {
                    type = outcome ? EmotionType.Admiration : EmotionType.Reproach;
                    intensity = AttributionBaseIntensity * 0.6f;
                }

                tokens.Add(new EmotionToken(type, intensity, DecayRate(type), worldEvent.Tick));
            }

            return tokens;
        }

        // -----------------------------------------------------------------
        // Axis 3: Standard Conformity
        // Only fires when observing another character's outcome in a domain the
        // observing character has demonstrated investment in.
        // -----------------------------------------------------------------
        private static List<EmotionToken> EvaluateStandardConformity(
            Character character,
            WorldEvent worldEvent)
        {
            var tokens = new List<EmotionToken>();

            if (worldEvent.Subject == null || worldEvent.Subject.Id == character.Id) return tokens;
            if (!worldEvent.IsPositiveOutcome.HasValue) return tokens;

            bool isDomainRelevant = worldEvent.Category is
                WorldEventCategory.LabSuccess or
                WorldEventCategory.BreakthroughMade or
                WorldEventCategory.SpellInvented;

            if (!isDomainRelevant) return tokens;

            var type = worldEvent.IsPositiveOutcome.Value ? EmotionType.Envy : EmotionType.Gloating;
            tokens.Add(new EmotionToken(type, ConformityBaseIntensity, DecayRate(type), worldEvent.Tick));
            return tokens;
        }

        // -----------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------

        /// <summary>
        /// Returns the decay rate for the given emotion type from the fixed table.
        /// </summary>
        private static float DecayRate(EmotionType type)
            => BaseDecayRates.TryGetValue(type, out var rate) ? rate : 0.25f;

        /// <summary>
        /// Computes how relevant this event is to a specific intention.
        /// Returns a value in [0, 1]. 0 = not relevant; 1 = directly relevant.
        /// This drives both emotion type selection and token intensity on the goal-relevance axis.
        /// </summary>
        private static float ComputeGoalRelevance(Intention intention, WorldEvent worldEvent)
            => worldEvent.Category switch
            {
                WorldEventCategory.LabSuccess or
                WorldEventCategory.LabFailure or
                WorldEventCategory.SpellInvented or
                WorldEventCategory.BreakthroughMade => 0.6f,

                WorldEventCategory.RecruitmentSucceeded or
                WorldEventCategory.RecruitmentFailed or
                WorldEventCategory.RecruitmentAttempted => 0.5f,

                WorldEventCategory.AgingCrisis => 0.8f,

                WorldEventCategory.ApprenticeFound or
                WorldEventCategory.ApprenticeGauntleted or
                WorldEventCategory.ApprenticeLost => 0.7f,

                _ => 0f
            };
    }
}