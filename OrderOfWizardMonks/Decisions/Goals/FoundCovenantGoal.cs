using WizardMonks.Decisions.Conditions;
using WizardMonks.Models.Characters;

namespace WizardMonks.Decisions.Goals
{
    public class FoundCovenantGoal : AGoal
    {
        public FoundCovenantGoal(Magus magus, double desire)
            : base(magus, magus.SeasonalAge + 20, desire) // Find a place within 5 years
        {
            // The condition is simply to have a covenant.
            Conditions.Add(new IsCovenantFounderCondition(magus, (uint)AgeToCompleteBy, desire));
        }
    }
}