using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WizardMonks.Instances;
using WizardMonks.Models.Characters;

namespace WizardMonks.Activities.MageActivities
{
    [Serializable]
    public class ExtractVisActivity : AExposingMageActivity
    {
        public ExtractVisActivity(Ability exposure, double desire) : base(exposure, desire)
        {
            Action = Activity.DistillVis;
        }

        protected override void DoMageAction(Magus mage)
        {
            if (mage.Covenant == null)
            {
                throw new ArgumentNullException("Magi can only extract vis in an aura!");
            }
            double amount = mage.GetVisDistillationRate();
            mage.Log.Add("Extracted " + amount.ToString("0.00") + " pawns of vis from aura");
            mage.GainVis(MagicArts.Vim, amount);
        }

        public override bool Matches(IActivity action)
        {
            return action.Action == Activity.DistillVis;
        }

        public override string Log()
        {
            return "Extracting vis worth " + Desire.ToString("0.000");
        }
    }

}
