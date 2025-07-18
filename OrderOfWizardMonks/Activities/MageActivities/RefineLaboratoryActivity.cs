
namespace WizardMonks.Activities.MageActivities
{
    public class RefineLaboratoryActivity : AExposingMageActivity
    {
        public RefineLaboratoryActivity(Ability exposure, double desire)
            : base(exposure, desire)
        {
            Action = Activity.RefineLaboratory;
        }

        protected override void DoMageAction(Magus mage)
        {
            // we may want to do the check here as well to be safe
            mage.RefineLaboratory();
        }

        public override bool Matches(IActivity action)
        {
            return action.Action == Activity.RefineLaboratory;
        }

        public override string Log()
        {
            return "Refining lab worth " + Desire.ToString("0.000");
        }
    }

}
