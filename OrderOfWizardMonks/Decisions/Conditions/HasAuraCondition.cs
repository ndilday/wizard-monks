using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks.Decisions.Conditions
{
    class HasAuraCondition : ACondition
    {
        public HasAuraCondition(Character character, uint ageToCompleteBy, double desire, ushort conditionDepth = 1) : base(character, ageToCompleteBy, desire, conditionDepth)
        {

        }

        public override bool ConditionFulfilled
        {
            get
            {
                return Character.KnownAuras != null && Character.KnownAuras.Count > 0;
            }
        }


        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log)
        {
            throw new NotImplementedException();
        }
    }
}
