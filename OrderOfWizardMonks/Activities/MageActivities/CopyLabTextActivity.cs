using System;

namespace WizardMonks.Activities.MageActivities
{
    public class CopyLabTextActivity : AExposingMageActivity
    {
        public CopyLabTextActivity(Ability exposure, double desire) : base(exposure, desire)
        {
            Action = Activity.CopyLabText;
        }

        protected override void DoMageAction(Magus mage)
        {
            //TODO: implement
        }

        public override bool Matches(IActivity action)
        {
            return action.Action == Activity.CopyLabText;
        }

        public override string Log()
        {
            throw new NotImplementedException();
        }
    }

}
