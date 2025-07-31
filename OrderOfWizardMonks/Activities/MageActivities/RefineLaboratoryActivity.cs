
using WizardMonks.Core;
using WizardMonks.Instances;

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
            if (mage.Laboratory == null) return;

            mage.Log.Add("Spent the season refining laboratory organization and efficiency.");
            mage.RefineLaboratory();

            // Per Covenants p.110, roll for gaining organizational Virtues/Flaws
            double rollTotal = Die.Instance.RollStressDie(0, out _) + mage.GetAttributeValue(AttributeType.Intelligence) + mage.GetAbility(Abilities.MagicTheory).Value;

            if (rollTotal >= 15)
            {
                // Logic to check for and remove Hidden Defect Flaw
            }
            if (rollTotal >= 12)
            {
                // Logic to add Highly Organized Virtue
            }
            // Handle botches to add Hidden Defect Flaw
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
