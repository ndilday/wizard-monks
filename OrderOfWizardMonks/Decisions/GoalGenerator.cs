using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Decisions.Goals;
using WizardMonks.Instances;
using WizardMonks.Models.Beliefs;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Covenants;
using WizardMonks.Services.Characters;

namespace WizardMonks.Decisions
{
    public static class GoalGenerator
    {
        public static void GenerateAndReviewGoals(Magus magus)
        {
            // Step 1: Review and remove completed goals
           foreach(IGoal goal in magus.ActiveGoals)
            {
                if(goal.IsComplete())
                {
                    magus.MarkGoalComplete(goal);
                }
            }

            // Step 2: Generate a list of potential new goals
            var potentialGoals = new List<IGoal>();
            potentialGoals.AddRange(GenerateFoundationalGoals(magus));
            potentialGoals.AddRange(GenerateGoalsFromState(magus));
            potentialGoals.AddRange(GenerateGoalsFromPersonality(magus));
            potentialGoals.AddRange(GenerateGoalsFromBeliefs(magus));
            potentialGoals.AddRange(GenerateGoalsFromIdeas(magus)); // From your IdeaManager

            // Step 3: Filter out goals that are already being pursued
            var existingGoalTypes = new HashSet<Type>(magus.ActiveGoals.Select(g => g.GetType()));
            var newGoals = potentialGoals
                .Where(pg => !existingGoalTypes.Contains(pg.GetType()))
                .GroupBy(g => g.GetType()) // Ensure we don't add multiple of the same goal type
                .Select(g => g.OrderByDescending(goal => goal.Desire).First());

            // Step 4: Adopt new goals
            foreach (var goal in newGoals)
            {
                if (goal.Desire > 0.1) // Desire threshold to avoid clutter
                {
                    magus.AddGoal(goal);
                    magus.Log.Add($"[Goal Added] New Goal: {goal.GetType().Name} with Desire {goal.Desire:F2}");
                }
            }
        }

        private static IEnumerable<IGoal> GenerateFoundationalGoals(Magus magus)
        {
            // Every magus needs these for survival and basic function.
            if (magus.LongevityRitual == 0 && magus.SeasonalAge >= 120) // age 30
            {
                yield return new AvoidDecrepitudeGoal(magus, 100.0); // High, non-negotiable desire
            }
            // Add other universals here, like "Build a Lab if you don't have one"
        }

        private static IEnumerable<IGoal> GenerateGoalsFromState(Magus magus)
        {
            // Goals based on "gaps" in the character's status.

            // The "Leaving the Nest" Goal for the newly-minted Hermetic Magus
            bool hasLearnedHermeticBasics = magus.GetAbility(Abilities.ParmaMagica).Value > 0 && magus.GetAbility(Abilities.MagicTheory).Value > 0;
            if (hasLearnedHermeticBasics && magus.Covenant != null && magus.Covenant.GetRoleForMagus(magus) == CovenantRole.Visitor)
            {
                // Desire is very high - they have what they came for and want autonomy.
                yield return new FoundCovenantGoal(magus, 200.0);
            }

            // 1. Check if the magus needs to FIND an apprentice.
            if (magus.Apprentice == null && magus.SeasonalAge > 160 && !magus.ActiveGoals.OfType<FindApprenticeGoal>().Any())
            {
                double desire = (magus.SeasonalAge - 160) / 4.0;
                yield return new FindApprenticeGoal(magus, desire);
            }

            // 2. Manage the TRAINING of an existing apprentice.
            var existingTrainingGoal = magus.ActiveGoals.OfType<TrainApprenticeGoal>().FirstOrDefault();

            if (magus.Apprentice != null)
            {
                // Calculate the current training year and its deadline
                uint seasonsSinceStart = magus.SeasonalAge - magus.ApprenticeTrainingStartSeason;
                uint currentTrainingYearIndex = seasonsSinceStart / 4; // Year 1 is index 0, Year 2 is index 1, etc.
                uint deadlineForThisYear = magus.ApprenticeTrainingStartSeason + ((currentTrainingYearIndex + 1) * 4) - 1;
                uint startOfThisYear = deadlineForThisYear - 3;

                // Check if training has been done for the current year block
                bool trainingIsDoneForThisYear = magus.LastSeasonTrainedApprentice >= startOfThisYear;

                if (trainingIsDoneForThisYear)
                {
                    // If training is done, any existing goal for this period is obsolete.
                    if (existingTrainingGoal != null)
                    {
                        existingTrainingGoal.MarkAsSeasonallyComplete();
                    }
                }
                else if (existingTrainingGoal == null)
                {
                    // A goal is needed, but none exists. Create one.
                    yield return new TrainApprenticeGoal(magus, deadlineForThisYear, 100.0);
                }
            }
            else if (existingTrainingGoal != null)
            {
                // Cleanup: If there's no apprentice, the training goal is obsolete.
                existingTrainingGoal.MarkAsSeasonallyComplete();
            }
        }

        private static IEnumerable<IGoal> GenerateGoalsFromPersonality(Magus magus)
        {
            // This is where personality drives ambition.
            // Example: A proud magus (low Modesty) wants to write books.
            double prideFactor = magus.Personality.GetInverseDesireMultiplier(HexacoFacet.Modesty);
           yield return new GainReputationGoal(magus, prideFactor);

            // Example: An inquisitive magus wants to do original research
            double curiosityFactor = magus.Personality.GetDesireMultiplier(HexacoFacet.Inquisitiveness);
            if (curiosityFactor > 1.1)
            {
                // a simple goal to improve their highest art
                var bestArt = magus.Arts.OrderByDescending(a => a.Value).First();
                double desire = curiosityFactor * 10;
                yield return new AbilityScoreGoal(magus, null, desire, bestArt.Ability, bestArt.Value + 1);
            }
        }

        private static IEnumerable<IGoal> GenerateGoalsFromBeliefs(Magus magus)
        {
            // This is the key for the Founding scenario.
            // A hedge wizard arrives with a strong belief in Bonisagus.
            var bonisagusBelief = magus.GetBeliefProfile(Founders.Bonisgaus).GetBeliefMagnitude(BeliefTopics.MagicTheory.Name);

            if (bonisagusBelief > 10 && magus.GetAbility(Abilities.ParmaMagica).Value < 1)
            {
                // They believe Bonisagus is a master, and they lack the key Hermetic skills.
                // This generates the goal to learn from him. The desire is proportional to the belief strength.
                yield return new LearnFromMasterGoal(magus, Founders.Bonisgaus, bonisagusBelief * 5);
            }
        }

        private static IEnumerable<IGoal> GenerateGoalsFromIdeas(Magus magus)
        {
            foreach (var idea in magus.GetInspirations())
            {
                yield return new PursueIdeaGoal(magus, idea);
            }
        }
    }
}
