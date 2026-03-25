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
        public static void GenerateAndReviewGoals(HermeticMagus magus)
        {
            // Step 1: Review and remove completed goals
            foreach (IGoal goal in magus.ActiveGoals)
            {
                if (goal.IsComplete())
                {
                    magus.MarkGoalComplete(goal);
                }
            }

            // TODO: Step 2: Generate a list of potential new goals
            var potentialGoals = new List<IGoal>();

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
    }
}
