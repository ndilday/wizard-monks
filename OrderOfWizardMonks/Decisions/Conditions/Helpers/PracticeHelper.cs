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
                double alPracticeDesire = _desireFunc(4, ConditionDepth);
                if (alPracticeDesire > 0.01)
                {
                    log.Add("Practicing Area Lore before finding a new aura worth " + alPracticeDesire.ToString("0.00"));
                    alreadyConsidered.Add(new Practice(_ability, alPracticeDesire));
                }
            }
        }
    }
}
