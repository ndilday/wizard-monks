using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks.Decisions.Conditions
{
    public class HasLabCondition : ACondition
    {
        private Magus _mage;
        public HasLabCondition(Magus magus, uint ageToCompleteBy, double desire, ushort conditionDepth = 1) : base(magus, ageToCompleteBy, desire, conditionDepth)
        {
            _mage = magus;
        }

        public override bool ConditionFulfilled
        {
            get
            {
                return _mage.Laboratory != null;
            }
        }


        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log)
        {
            if(!ConditionFulfilled)
            {

            }
        }
    }
}
