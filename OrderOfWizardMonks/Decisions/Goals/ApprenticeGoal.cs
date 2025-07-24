using WizardMonks.Decisions.Conditions;
using WizardMonks.Economy;
using WizardMonks.Instances;

namespace WizardMonks.Decisions.Goals
{
    class ApprenticeGoal : AGoal
    {
        public ApprenticeGoal(Character character, uint? dueDate, double desire) : base(character, dueDate, desire)
        {
            foreach (Ability ability in MagicArts.GetEnumerator())
            {
                Conditions.Add(new AbilityScoreCondition(character, dueDate == null ? 200 : (uint)(dueDate - 1), desire, ability, 5));
            }
        }
    }
}
