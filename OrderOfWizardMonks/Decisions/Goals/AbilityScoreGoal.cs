using WizardMonks.Decisions.Conditions;
using WizardMonks.Instances;

namespace WizardMonks.Decisions.Goals
{
    public class AbilityScoreGoal : AGoal
    {
        Ability _ability;
        public AbilityScoreGoal(Character character, Ability ability, double level, double desire, uint ageToCompleteBy = 400) :
            base(character, desire, ageToCompleteBy)
        {
            _ability = ability;
            Conditions.Add(new AbilityScoreCondition(character, ageToCompleteBy, desire, ability, level));
        }
    }
}
