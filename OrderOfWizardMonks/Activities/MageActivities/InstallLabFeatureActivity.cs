using WizardMonks.Models.Laboratories;

namespace WizardMonks.Activities.MageActivities
{
    public class InstallLabFeatureActivity : AExposingMageActivity
    {
        public LabFeature FeatureToInstall { get; private set; }

        public InstallLabFeatureActivity(LabFeature feature, Ability exposure, double desire)
            : base(exposure, desire)
        {
            Action = Activity.InstallLaboratoryFeature;
            FeatureToInstall = feature;
        }

        protected override void DoMageAction(Magus mage)
        {
            if (mage.Laboratory == null)
            {
                mage.Log.Add("Cannot install a feature without a laboratory.");
                return;
            }

            // The book notes this can take 1 season for Minor and 2 for Major.
            // For our one-action-per-season model, we will simplify this to 1 season for any installation.
            // The higher "space" cost of Major Virtues already represents their greater investment.
            mage.Laboratory.AddFeature(FeatureToInstall);
            mage.Log.Add($"Installed the '{FeatureToInstall.Name}' feature in the laboratory.");
        }

        public override bool Matches(IActivity action)
        {
            if (action is not InstallLabFeatureActivity installAction)
            {
                return false;
            }
            return installAction.FeatureToInstall.Name == this.FeatureToInstall.Name;
        }

        public override string Log()
        {
            return $"Installing '{FeatureToInstall.Name}' worth {Desire:0.000}";
        }
    }
}