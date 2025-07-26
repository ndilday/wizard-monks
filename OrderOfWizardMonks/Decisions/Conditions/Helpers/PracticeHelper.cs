using System.Collections.Generic;
using WizardMonks.Activities;

namespace WizardMonks.Decisions.Conditions.Helpers
{
    class PracticeHelper : AHelper
    {
        private Ability _ability;
        public PracticeHelper(Ability ability, Magus mage, uint ageToCompleteBy, ushort conditionDepth, CalculateDesireFunc desireFunc = null) :
            base(mage, ageToCompleteBy, conditionDepth, desireFunc)
        {
            _ability = ability;
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, Desires desires, IList<string> log)
        {
            if (_desireFunc != null)
            {
                double gain = this._mage.GetAbility(_ability).GetValueGain(4);
                double practiceDesire = _desireFunc(gain, _conditionDepth);
                log.Add("Practicing " + _ability.AbilityName + " worth " + practiceDesire.ToString("0.000"));
                alreadyConsidered.Add(new PracticeActivity(_ability, practiceDesire));
            }
        }
    }
}
