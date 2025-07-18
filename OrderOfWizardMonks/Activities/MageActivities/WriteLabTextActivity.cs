using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardMonks.Activities.MageActivities
{
    public class WriteLabTextActivity : AExposingMageActivity
    {
        public WriteLabTextActivity(Ability exposure, double desire) : base(exposure, desire)
        {
            Action = Activity.WriteLabText;
        }

        protected override void DoMageAction(Magus mage)
        {
            //TODO: implement
        }

        public override bool Matches(IActivity action)
        {
            return action.Action == Activity.WriteLabText;
        }

        public override string Log()
        {
            throw new NotImplementedException();
        }
    }

}
