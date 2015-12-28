using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks.Decisions.Conditions
{
    public class HasLabCondition : ACondition
    {
        public HasLabCondition(Character character, uint? ageToCompleteBy, double desire) : base(character, ageToCompleteBy, desire)
        {

        }

        public override bool ConditionFulfilled
        {
            get
            {
                return typeof(Magus) == Character.GetType() && ((Magus)Character).Laboratory != null;
            }
        }


        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log)
        {
            throw new NotImplementedException();
        }
    }
}
