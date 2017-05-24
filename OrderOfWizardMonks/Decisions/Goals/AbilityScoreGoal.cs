using WizardMonks.Decisions.Conditions;
using WizardMonks.Instances;

namespace WizardMonks.Decisions.Goals
{
    public class AbilityScoreGoal : AGoal
    {
        Ability _ability;
        public AbilityScoreGoal(Character character, uint? ageToCompleteBy, double desire, Ability ability, double level) :
            base(character, ageToCompleteBy, desire)
        {
            _ability = ability;
            uint modifiedAge = ageToCompleteBy == null ? 100 : (uint)ageToCompleteBy;
            Conditions.Add(new AbilityScoreCondition(character, modifiedAge, desire, ability, level));
        }
    }
}
