using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks.Decisions
{
    class InAuraCondition : ACondition
    {
        public InAuraCondition(Character character) : base(character)
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
