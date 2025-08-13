using WizardMonks.Decisions.Conditions;
using WizardMonks.Instances;
using WizardMonks.Models.Characters;

namespace WizardMonks.Decisions.Goals
{
    class ApprenticeGoal : AGoal
    {
        public ApprenticeGoal(Magus mage, uint? dueDate, double desire) : base(mage, dueDate, desire)
        {
            foreach (Ability ability in MagicArts.GetEnumerator())
            {
                double effectiveDesire = desire * mage.Personality.GetDesireMultiplier(HexacoFacet.Sociability);
                Conditions.Add(new AbilityScoreCondition(mage, dueDate == null ? 200 : (uint)(dueDate - 1), desire, ability, 5));
            }
        }
    }
}
