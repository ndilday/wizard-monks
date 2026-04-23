using WizardMonks.Decisions.Conditions;
using WizardMonks.Instances;
using WizardMonks.Models.Characters;

namespace WizardMonks.Decisions.Goals
{
    public class AbilityScoreGoal : AGoal
    {
        public Ability Ability { get; }
        public AbilityScoreGoal(Character character, uint? ageToCompleteBy, double desire, Ability ability, double level) :
            base(character, ageToCompleteBy, desire)
        {
            Ability = ability;
            uint modifiedAge = ageToCompleteBy == null ? 200 : (uint)ageToCompleteBy;
            Conditions.Add(new AbilityScoreCondition(character, modifiedAge, desire, Ability, level));
        }
    }
}
