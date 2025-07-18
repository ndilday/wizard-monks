
namespace WizardMonks.Activities.MageActivities
{
    public class BuildLaboratoryActivity : AExposingMageActivity
    {
        private readonly Aura _aura;

        public BuildLaboratoryActivity(Ability exposure, double desire)
            : base(exposure, desire)
        {
            Action = Activity.BuildLaboratory;
        }

        public BuildLaboratoryActivity(Aura aura, Ability exposure, double desire)
            : base(exposure, desire)
        {
            Action = Activity.BuildLaboratory;
            _aura = aura;
        }

        protected override void DoMageAction(Magus mage)
        {
            // TODO: build size
            // TODO: pre-existing conditions
            mage.Log.Add("Built laboratory");
            if (_aura == null)
            {
                mage.BuildLaboratory();
            }
            else
            {
                mage.BuildLaboratory(_aura);
            }
        }

        public override bool Matches(IActivity action)
        {
            return action.Action == Activity.BuildLaboratory;
        }

        public override string Log()
        {
            return "Building lab worth " + Desire.ToString("0.000");
        }
    }

}
