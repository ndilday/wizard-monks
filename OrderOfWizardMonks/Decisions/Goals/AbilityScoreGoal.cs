using WizardMonks.Characters;
using WizardMonks.Decisions.Conditions;


namespace WizardMonks.Decisions.Goals
{
    public class AbilityScoreGoal : AGoal
    {
        public AbilityScoreGoal(Character character, uint? ageToCompleteBy, double desire, Ability ability, double level) :
            base(character, ageToCompleteBy, desire)
        {
            uint modifiedAge = ageToCompleteBy == null ? 200 : (uint)ageToCompleteBy;
            Conditions.Add(new AbilityScoreCondition(character, modifiedAge, desire, ability, level));
        }
    }
}
