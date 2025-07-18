using System;
using WizardMonks.Instances;

namespace WizardMonks.Activities.MageActivities
{
    public class InventLongevityRitualActivity : AExposingMageActivity
    {
        public InventLongevityRitualActivity(Ability exposure, double desire) : base(exposure, desire)
        {
            Action = Activity.LongevityRitual;
        }

        protected override void DoMageAction(Magus mage)
        {
            uint strength = Convert.ToUInt16(mage.GetLabTotal(MagicArtPairs.CrVi, Activity.LongevityRitual));
            mage.Log.Add("Created a longevity ritual of strength " + strength);
            mage.ApplyLongevityRitual(Convert.ToUInt16(mage.GetLabTotal(MagicArtPairs.CrVi, Activity.LongevityRitual)));
        }

        public override bool Matches(IActivity action)
        {
            return action.Action == Activity.LongevityRitual;
        }

        public override string Log()
        {
            return "Longevity Ritual worth " + Desire.ToString("0.000");
        }
    }
}
