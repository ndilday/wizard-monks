using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks.Decisions.Conditions
{
    class InAuraCondition : ACondition
    {
        public InAuraCondition(Character character, uint? ageToCompleteBy, double desire) : base(character, ageToCompleteBy, desire)
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
