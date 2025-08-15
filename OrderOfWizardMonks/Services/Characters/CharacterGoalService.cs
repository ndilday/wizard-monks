using WizardMonks.Models.Characters;

using WizardMonks.Decisions.Goals;

namespace WizardMonks.Services.Characters
{
    public static class CharacterGoalService
    {
        public static void MarkGoalComplete(this Character character, IGoal goal)
        {
            character.ActiveGoals.Remove(goal);
            character.CompletedGoals.Add(goal);
        }
    }
}
