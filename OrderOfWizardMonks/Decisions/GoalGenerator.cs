using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Decisions.Goals;
using WizardMonks.Instances;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Ideas;

namespace WizardMonks.Decisions
{
    /// <summary>
    /// Default implementation of IGoalGenerator.
    ///
    /// For each goal category, evaluates motivational pressure from emotion tokens
    /// and self-beliefs, then compares it to a personality-modulated generation threshold.
    /// A new Intention is created when pressure exceeds the threshold and no equivalent
    /// intention is already active.
    ///
    /// Personality shapes behavior here through two mechanisms only:
    ///   1. Generation thresholds — facets lower or raise the pressure required to form a goal.
    ///   2. Commitment strength — facets influence how durable a formed intention is.
    /// Personality does not modulate emotion token intensity or decay rates.
    /// </summary>
    public sealed class GoalGenerator : IGoalGenerator
    {
        private const float BaseGenerationThreshold = 0.3f;
        private const int BaseStagnationTicks = 8;

        public IEnumerable<Intention> GenerateIntentions(
            Character character,
            EmotionLedger emotions,
            CharacterBeliefStore beliefs,
            IReadOnlyList<Intention> activeIntentions,
            int currentTick)
        {
            var newIntentions = new List<Intention>();

            TryGenerateSelfPreservationIntentions(character, emotions, beliefs, activeIntentions, currentTick, newIntentions);
            TryGenerateResearchIntentions(character, emotions, beliefs, activeIntentions, currentTick, newIntentions);
            TryGenerateSocialIntentions(character, emotions, beliefs, activeIntentions, currentTick, newIntentions);
            TryGenerateRecognitionIntentions(character, emotions, beliefs, activeIntentions, currentTick, newIntentions);
            TryGenerateReactiveIntentions(character, emotions, beliefs, activeIntentions, currentTick, newIntentions);

            return newIntentions;
        }

        // -----------------------------------------------------------------
        // Self-preservation goals
        // Pressure comes from Fear token intensity and self-beliefs about vulnerability.
        // Personality shapes the threshold at which this pressure generates a goal:
        //   - High Prudence lowers the threshold (acts earlier, more cautious)
        //   - High Fearfulness lowers it further (more sensitive to threat cues)
        // Personality does not alter the Fear token itself.
        // -----------------------------------------------------------------
        private static void TryGenerateSelfPreservationIntentions(
            Character character,
            EmotionLedger emotions,
            CharacterBeliefStore beliefs,
            IReadOnlyList<Intention> activeIntentions,
            int currentTick,
            List<Intention> output)
        {
            float fearIntensity = emotions.GetIntensity(EmotionType.Fear);
            float vulnerabilityBelief = beliefs.SelfBeliefs.GetValue("Vulnerability");

            float pressure = fearIntensity * 0.6f + Math.Max(0f, vulnerabilityBelief) * 0.4f;

            float prudence = (float)character.Personality.GetFacet(HexacoFacet.Prudence);
            float fearfulness = (float)character.Personality.GetFacet(HexacoFacet.Fearfulness);

            // Both high Prudence and high Fearfulness lower the threshold,
            // making self-preservation goals generate at lower levels of pressure.
            float threshold = BaseGenerationThreshold / (prudence * fearfulness);

            if (pressure < threshold) return;
            if (HasActiveIntentionOfType<AvoidDecrepitudeGoal>(activeIntentions)) return;

            var goal = new AvoidDecrepitudeGoal(character as HermeticMagus, pressure);
            float commitment = ComputeCommitmentStrength(fearIntensity, character, HexacoFacet.Prudence);
            int stagnation = ComputeStagnationTicks(character);

            output.Add(new Intention(goal, commitment, pressure, currentTick, stagnation));
        }

        // -----------------------------------------------------------------
        // Research and mastery goals
        // Pressure comes from Pride, Envy, and self-beliefs about competence.
        // High Inquisitiveness lowers the generation threshold.
        // -----------------------------------------------------------------
        // Research and mastery goals
        // Pressure comes from Pride, Envy, and self-beliefs about competence.
        // High Inquisitiveness lowers the generation threshold.
        //
        // Multiple research intentions are allowed — a high-Inquisitiveness,
        // low-Prudence mage naturally accumulates more than they finish.
        // The only guard is against duplicating an intention for an idea
        // already being actively pursued.
        //
        // Goal selection:
        //   1. Find ideas not already covered by an active PursueIdeaGoal.
        //      SpellIdeas scored by combined art values (achievability).
        //      BreakthroughIdeas boosted by Envy intensity (competitive drive).
        //      Each qualifying idea above the threshold gets its own Intention.
        //   2. If no ideas exist, improve the character's highest-scored art
        //      by one level — but only if no AbilityScoreGoal targeting a
        //      magic art is already active.
        // -----------------------------------------------------------------
        private static void TryGenerateResearchIntentions(
            Character character,
            EmotionLedger emotions,
            CharacterBeliefStore beliefs,
            IReadOnlyList<Intention> activeIntentions,
            int currentTick,
            List<Intention> output)
        {
            if (character is not HermeticMagus magus) return;

            float prideIntensity = emotions.GetIntensity(EmotionType.Pride);
            float envyIntensity = emotions.GetIntensity(EmotionType.Envy);
            float competenceBelief = beliefs.SelfBeliefs.GetValue("MagicalCompetence");
            float inquisitiveness = (float)character.Personality.GetFacet(HexacoFacet.Inquisitiveness);

            float pressure = prideIntensity * 0.4f
                           + envyIntensity * 0.3f
                           + Math.Max(0f, competenceBelief) * 0.3f;

            float threshold = BaseGenerationThreshold / inquisitiveness;

            if (pressure < threshold) return;

            float dominantEmotion = Math.Max(prideIntensity, envyIntensity);
            float commitment = ComputeCommitmentStrength(dominantEmotion, character, HexacoFacet.Inquisitiveness);
            int stagnation = ComputeStagnationTicks(character);

            var pursuedIdeaIds = activeIntentions
                .Select(i => i.UnderlyingGoal)
                .OfType<PursueIdeaGoal>()
                .Select(g => g.Idea.Id)
                .ToHashSet();

            bool generatedAny = false;
            foreach (var idea in magus.GetInspirations())
            {
                if (pursuedIdeaIds.Contains(idea.Id)) continue;

                float ideaScore = ScoreIdea(idea, magus, envyIntensity);
                if (ideaScore <= 0f) continue;

                output.Add(new Intention(new PursueIdeaGoal(magus, idea), commitment, pressure, currentTick, stagnation));
                generatedAny = true;
            }

            if (!generatedAny && !HasActiveArtImprovementGoal(activeIntentions))
            {
                var artGoal = SelectArtImprovementGoal(magus, pressure);
                if (artGoal != null)
                    output.Add(new Intention(artGoal, commitment, pressure, currentTick, stagnation));
            }
        }

        private static float ScoreIdea(AIdea idea, HermeticMagus magus, float envyIntensity)
        {
            if (idea is SpellIdea spellIdea)
            {
                // Higher combined art score = more achievable = higher confidence payoff.
                double techScore = magus.Arts.GetAbility(spellIdea.Arts.Technique).Value;
                double formScore = magus.Arts.GetAbility(spellIdea.Arts.Form).Value;
                return (float)(techScore + formScore);
            }
            if (idea is BreakthroughIdea)
            {
                // Breakthroughs are ambitious; Envy (competitive drive) amplifies their appeal.
                return 5f + envyIntensity * 10f;
            }
            return 0f;
        }

        // An AbilityScoreGoal targeting a magic art is already serving as
        // a research intention; don't stack another one on top.
        private static bool HasActiveArtImprovementGoal(IReadOnlyList<Intention> intentions)
            => intentions
                .Select(i => i.UnderlyingGoal)
                .OfType<AbilityScoreGoal>()
                .Any(g => MagicArts.IsArt(g.Ability));

        private static AbilityScoreGoal SelectArtImprovementGoal(HermeticMagus magus, float pressure)
        {
            var best = MagicArts.GetEnumerator()
                .Select(art => (art, score: magus.Arts.GetAbility(art).Value))
                .OrderByDescending(x => x.score)
                .FirstOrDefault();

            if (best.art == null) return null;
            return new AbilityScoreGoal(magus, null, pressure, best.art, best.score + 1);
        }

        // -----------------------------------------------------------------
        // Social and relationship goals
        // Pressure comes from Gratitude, Admiration, and Anger tokens.
        // Personality shapes the threshold (Sociability, Fairness) and the
        // form the goal takes once generated — it does not alter the tokens.
        // -----------------------------------------------------------------
        private static void TryGenerateSocialIntentions(
            Character character,
            EmotionLedger emotions,
            CharacterBeliefStore beliefs,
            IReadOnlyList<Intention> activeIntentions,
            int currentTick,
            List<Intention> output)
        {
            float gratitude = emotions.GetIntensity(EmotionType.Gratitude);
            float anger = emotions.GetIntensity(EmotionType.Anger);
            float admiration = emotions.GetIntensity(EmotionType.Admiration);
            float sociability = (float)character.Personality.GetFacet(HexacoFacet.Sociability);
            float fairness = (float)character.Personality.GetFacet(HexacoFacet.Fairness);

            float pressure = (gratitude + admiration) * 0.5f - anger * 0.3f;

            // High Sociability and Fairness lower the threshold for social goal generation.
            float threshold = BaseGenerationThreshold / (sociability * fairness);

            if (pressure < threshold) return;

            character.Log.Add(
                $"[GoalGenerator] Social pressure {pressure:F2} — " +
                $"social goal type selection not yet implemented.");
        }

        // -----------------------------------------------------------------
        // Recognition and reputation goals
        // Pressure comes from Pride and self-beliefs about reputation gap.
        // Low Modesty and high Social Self-Esteem lower the generation threshold.
        // -----------------------------------------------------------------
        private static void TryGenerateRecognitionIntentions(
            Character character,
            EmotionLedger emotions,
            CharacterBeliefStore beliefs,
            IReadOnlyList<Intention> activeIntentions,
            int currentTick,
            List<Intention> output)
        {
            float pride = emotions.GetIntensity(EmotionType.Pride);
            float socialSelfEsteem = (float)character.Personality.GetFacet(HexacoFacet.SocialSelfEsteem);
            float modesty = (float)character.Personality.GetFacet(HexacoFacet.Modesty);

            float pressure = pride * 0.7f;

            // High Social Self-Esteem and low Modesty lower the threshold.
            // 2f - modesty inverts it: Modesty 0 → factor 2.0; Modesty 2 → factor 0.0.
            float threshold = BaseGenerationThreshold / (socialSelfEsteem * (2f - modesty));

            if (pressure < threshold) return;

            character.Log.Add(
                $"[GoalGenerator] Recognition pressure {pressure:F2} — " +
                $"recognition goal type selection not yet implemented.");
        }

        // -----------------------------------------------------------------
        // Reactive and crisis goals
        // Any emotion token above a high threshold triggers reconsideration.
        // No personality modulation on the threshold — crisis response is universal.
        // -----------------------------------------------------------------
        private static void TryGenerateReactiveIntentions(
            Character character,
            EmotionLedger emotions,
            CharacterBeliefStore beliefs,
            IReadOnlyList<Intention> activeIntentions,
            int currentTick,
            List<Intention> output)
        {
            float maxIntensity = emotions.Active.Values
                .Select(t => t.Intensity)
                .DefaultIfEmpty(0f)
                .Max();

            if (maxIntensity < 0.7f) return;

            character.Log.Add(
                $"[GoalGenerator] Reactive pressure {maxIntensity:F2} exceeds threshold — " +
                $"reactive goal type selection not yet implemented.");
        }

        // -----------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------

        private static bool HasActiveIntentionOfType<TGoal>(IReadOnlyList<Intention> intentions)
            where TGoal : IGoal
            => intentions.Any(i => i.UnderlyingGoal is TGoal);

        /// <summary>
        /// Computes commitment strength from the generating emotion intensity,
        /// scaled by the anchor personality facet. Higher facet score → more durable commitment.
        /// </summary>
        private static float ComputeCommitmentStrength(
            float emotionIntensity,
            Character character,
            HexacoFacet anchorFacet)
        {
            float facetScore = (float)character.Personality.GetFacet(anchorFacet);
            return Math.Clamp(emotionIntensity * facetScore, 0.1f, 0.95f);
        }

        /// <summary>
        /// Computes the stagnation tolerance for an intention.
        /// High Prudence characters tolerate longer periods without progress before reconsidering.
        /// </summary>
        private static int ComputeStagnationTicks(Character character)
        {
            float prudence = (float)character.Personality.GetFacet(HexacoFacet.Prudence);
            return (int)Math.Clamp(BaseStagnationTicks * prudence, 2, 24);
        }
    }
}