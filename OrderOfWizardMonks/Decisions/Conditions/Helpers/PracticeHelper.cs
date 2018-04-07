using System.Collections.Generic;

namespace WizardMonks.Decisions.Conditions.Helpers
{
    class PracticeHelper : AHelper
    {
        private Ability _ability;
        public PracticeHelper(Ability ability, Magus mage, uint ageToCompleteBy, double desire, ushort conditionDepth, CalculateDesireFunc desireFunc = null) :
            base(mage, ageToCompleteBy, desire, conditionDepth, desireFunc)
        {
            _ability = ability;
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log)
        {
            if (_desireFunc != null)
            {
                double gain = this.Mage.GetAbility(_ability).GetValueGain(4);
                double practiceDesire = _desireFunc(gain, ConditionDepth);
                log.Add("Practicing " + _ability.AbilityName + " worth " + practiceDesire.ToString("0.000"));
                alreadyConsidered.Add(new Practice(_ability, practiceDesire));
            }
        }
    }
}
