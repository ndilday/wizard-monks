using WizardMonks.Decisions.Conditions;

namespace WizardMonks.Decisions.Goals
{
    public class AbilityScoreGoal : AGoal
    {
        public AbilityScoreGoal(Character character, uint? ageToCompleteBy, double desire, Ability ability, double level) :
            base(character, ageToCompleteBy, desire)
        {
            Conditions.Add(new AbilityScoreCondition(character, ageToCompleteBy, desire, ability, level));
        }
    }
}
