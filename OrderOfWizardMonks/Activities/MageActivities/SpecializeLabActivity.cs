using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WizardMonks.Models;
using WizardMonks.Models.Characters;

namespace WizardMonks.Activities.MageActivities
{
    public class SpecializeLabActivity : AExposingMageActivity
    {
        public Ability ArtSpecialization { get; private set; }
        public Activity ActivitySpecialization { get; private set; }

        public SpecializeLabActivity(Ability art, Ability exposure, double desire)
            : base(exposure, desire)
        {
            Action = Activity.RefineLaboratory;
            ArtSpecialization = art;
            ActivitySpecialization = Activity.Sundry;
        }

        public SpecializeLabActivity(Activity activity, Ability exposure, double desire)
            : base(exposure, desire)
        {
            Action = Activity.RefineLaboratory;
            ArtSpecialization = null;
            ActivitySpecialization = activity;
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
            if (ArtSpecialization != null)
            {
                mage.Laboratory.Specialize(ArtSpecialization);
                mage.Log.Add($"Specialized the laboratory for {ArtSpecialization.AbilityName}.");
            }
            else
            {
                mage.Laboratory.Specialize(ActivitySpecialization);
                mage.Log.Add($"Specialized the laboratory for {ActivitySpecialization}.");
            }
            
        }

        public override bool Matches(IActivity action)
        {
            if (action is not SpecializeLabActivity  specializeAction)
            {
                return false;
            }
            return specializeAction.ArtSpecialization == this.ArtSpecialization &&
                specializeAction.ActivitySpecialization == this.ActivitySpecialization;
        }

        public override string Log()
        {
            return $"Specializing lab worth {Desire:0.000}";
        }
    }
}
