using WizardMonks.Decisions.Conditions;

namespace WizardMonks.Decisions.Goals
{
    public class AbilityScoreGoal : AGoal
    {
        public AbilityScoreGoal(Character character, uint? dueDate, double desire, Ability ability, double level) :
            base(character, dueDate, desire)
        {
            Conditions.Add(new AbilityScoreCondition(character, ability, level));
        }
    }
}
