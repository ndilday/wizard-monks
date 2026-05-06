using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Decisions.Goals;
using WizardMonks.Instances;
using WizardMonks.Models.Characters;
using WizardMonks.Services.Characters;

namespace WizardMonks.Decisions
{
    /// <summary>
    /// Generates low-commitment background intentions from situational opportunity
    /// rather than emotional pressure.
    ///
    /// Where GoalGenerator asks "what does the character feel driven to do?",
    /// PragmaticGoalGenerator asks "what could the character productively do,
    /// given what they have and who they are?" It runs after GoalGenerator in the
    /// cognitive stack so that it sees the full updated intention list and avoids
    /// regenerating goals that emotional pressure already covers.
    ///
    /// All produced intentions carry low commitment and short stagnation tolerance.
    /// They act as tie-breakers when no emotionally urgent goal is active, and fade
    /// into the background when real goals are present.
    /// </summary>
    public sealed class PragmaticGoalGenerator : IGoalGenerator
    {
        private const float MaxCommitment = 0.3f;
        private const float MaxDesire = 0.45f;
        private const float MinDesire = 0.05f;
        private const int StagnationTicks = 4;

        // How many weak-art goals to generate per pass; keeps the intention list tidy.
        private const int MaxArtGoals = 5;

        public IEnumerable<Intention> GenerateIntentions(
            Character character,
            EmotionLedger emotions,
            CharacterBeliefStore beliefs,
            IReadOnlyList<Intention> activeIntentions,
            int currentTick)
        {
            if (character is not HermeticMagus magus)
                return [];

            var output = new List<Intention>();

            TrySuggestLabImprovement(magus, activeIntentions, currentTick, output);
            TrySuggestVisDistillation(magus, activeIntentions, currentTick, output);
            TrySuggestArtStudy(magus, activeIntentions, currentTick, output);
            TrySuggestWriting(magus, activeIntentions, currentTick, output);
            TrySuggestAuraExploration(magus, activeIntentions, currentTick, output);

            return output;
        }

        // -----------------------------------------------------------------
        // Lab improvement
        // Organized, perfectionistic mages always see room to improve their lab.
        // -----------------------------------------------------------------
        private static void TrySuggestLabImprovement(
            HermeticMagus magus, IReadOnlyList<Intention> active, int tick, List<Intention> output)
        {
            if (magus.Laboratory == null) return;
            if (HasActiveIntentionOfType<ImproveLabGoal>(active)) return;

            double desire = Clamp(
                magus.Personality.GetFacet(HexacoFacet.Perfectionism)
                * magus.Personality.GetFacet(HexacoFacet.Organization)
                * 0.10);

            output.Add(MakeIntention(new ImproveLabGoal(magus, desire), tick));
        }

        // -----------------------------------------------------------------
        // Vis distillation
        // Prudent mages keep a stockpile. Desire scales with how far below
        // the target stock the current inventory sits.
        // -----------------------------------------------------------------
        private static void TrySuggestVisDistillation(
            HermeticMagus magus, IReadOnlyList<Intention> active, int tick, List<Intention> output)
        {
            if (magus.Covenant == null) return;
            if (HasActiveIntentionOfType<DistillVisGoal>(active)) return;

            double distillRate = magus.GetVisDistillationRate();
            if (distillRate <= 0) return;

            double totalVis = MagicArts.GetEnumerator().Sum(art => magus.GetVisCount(art));
            double targetStock = distillRate * 8.0 * magus.Personality.GetFacet(HexacoFacet.Prudence);
            double shortfall = Math.Max(0, targetStock - totalVis);
            if (shortfall <= 0) return;

            double desire = Clamp(
                (shortfall / targetStock)
                * magus.Personality.GetFacet(HexacoFacet.Prudence)
                * 0.40);

            output.Add(MakeIntention(new DistillVisGoal(magus, desire), tick));
        }

        // -----------------------------------------------------------------
        // Art study
        // Arts that lag well behind Magic Theory are the biggest lab total
        // bottlenecks. Generates up to MaxArtGoals for the weakest arts where
        // vis or books are actually available — no point suggesting an activity
        // the character has no means to pursue.
        // -----------------------------------------------------------------
        private static void TrySuggestArtStudy(
            HermeticMagus magus, IReadOnlyList<Intention> active, int tick, List<Intention> output)
        {
            double magicTheory = magus.GetAbility(Abilities.MagicTheory).Value;
            double inquisitiveness = magus.Personality.GetFacet(HexacoFacet.Inquisitiveness);

            var alreadyCoveredArts = active
                .Select(i => i.UnderlyingGoal)
                .OfType<AbilityScoreGoal>()
                .Where(g => MagicArts.IsArt(g.Ability))
                .Select(g => g.Ability)
                .ToHashSet();

            var candidates = MagicArts.GetEnumerator()
                .Where(art => !alreadyCoveredArts.Contains(art))
                .Select(art =>
                {
                    double artScore = magus.Arts.GetAbility(art).Value;
                    double gap = Math.Max(0, magicTheory - artScore);
                    double stockpile = magus.GetVisCount(art);
                    bool hasVis = stockpile > 0.5 + artScore / 10.0;
                    bool hasBook = magus.GetBestBookToRead(art) != null;
                    return (art, artScore, gap, hasVis, hasBook);
                })
                .Where(x => x.gap > 0 && (x.hasVis || x.hasBook))
                .OrderByDescending(x => x.gap)
                .Take(MaxArtGoals);

            foreach (var (art, artScore, gap, _, _) in candidates)
            {
                double desire = Clamp(gap * inquisitiveness * 0.025);

                // Set a modest target rather than chasing the full Magic Theory level in one shot.
                double targetScore = Math.Min(artScore + 5, magicTheory);
                output.Add(MakeIntention(new AbilityScoreGoal(magus, null, desire, art, targetScore), tick));
            }
        }

        // -----------------------------------------------------------------
        // Writing
        // Characters with high Communication and mastered arts recognize the value
        // of recording their knowledge, even without anyone to share it with yet.
        // -----------------------------------------------------------------
        private static void TrySuggestWriting(
            HermeticMagus magus, IReadOnlyList<Intention> active, int tick, List<Intention> output)
        {
            if (HasActiveIntentionOfType<WriteTextGoal>(active)) return;

            double communication = magus.GetAttribute(AttributeType.Communication).Value;
            double bestArt = MagicArts.GetEnumerator()
                .Max(art => magus.Arts.GetAbility(art).Value);

            // Shift Communication into a positive range; the attribute runs roughly -3 to +5.
            double desire = Clamp(
                (communication + 3.0) * bestArt
                * magus.Personality.GetFacet(HexacoFacet.SocialSelfEsteem)
                * 0.003);

            if (desire < MinDesire) return;

            output.Add(MakeIntention(new WriteTextGoal(magus, desire), tick));
        }

        // -----------------------------------------------------------------
        // Aura exploration
        // Lively, curious mages naturally want to know what's beyond their sanctum.
        // Area Lore gates whether exploration is plausible at all.
        // -----------------------------------------------------------------
        private static void TrySuggestAuraExploration(
            HermeticMagus magus, IReadOnlyList<Intention> active, int tick, List<Intention> output)
        {
            if (HasActiveIntentionOfType<ExploreAuraGoal>(active)) return;

            double areaLore = magus.GetAbility(Abilities.AreaLore).Value;
            if (areaLore <= 0) return;

            double desire = Clamp(
                areaLore
                * magus.Personality.GetFacet(HexacoFacet.Liveliness)
                * 0.03);

            if (desire < MinDesire) return;

            output.Add(MakeIntention(new ExploreAuraGoal(magus, desire), tick));
        }

        // -----------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------

        private static Intention MakeIntention(IGoal goal, int currentTick)
        {
            float commitment = (float)Math.Min(goal.Desire * 0.5, MaxCommitment);
            return new Intention(goal, commitment, (float)goal.Desire, currentTick, StagnationTicks);
        }

        private static double Clamp(double score)
            => Math.Clamp(score, MinDesire, MaxDesire);

        private static bool HasActiveIntentionOfType<TGoal>(IReadOnlyList<Intention> intentions)
            where TGoal : IGoal
            => intentions.Any(i => i.UnderlyingGoal is TGoal);
    }
}
