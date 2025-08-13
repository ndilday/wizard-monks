using WizardMonks.Core;
using WizardMonks.Instances;
using WizardMonks.Models.Characters;
using WizardMonks.Services.Characters;

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
            double rollTotal = Die.Instance.RollStressDie(0, out byte botches) + mage.GetAttributeValue(AttributeType.Intelligence) + mage.GetAbility(Abilities.MagicTheory).Value;

            if (botches > 0)
            {
                // BOTCH: Gain Hidden Defect
                if (!mage.Laboratory.HasFeature(LabFeatures.HiddenDefect))
                {
                    mage.Log.Add("Disaster! A subtle but dangerous flaw has crept into the lab's design.");
                    mage.Laboratory.AddFeature(LabFeatures.HiddenDefect);
                }
                return; // A botch precludes any positive outcomes
            }

            // SUCCESS: Check for positive outcomes
            if (rollTotal >= 15)
            {
                if (mage.Laboratory.HasFeature(LabFeatures.HiddenDefect))
                {
                    mage.Log.Add("Success! A previously unknown flaw in the lab has been discovered and corrected.");
                    mage.Laboratory.RemoveFeature(LabFeatures.HiddenDefect);
                }
            }

            if (rollTotal >= 12)
            {
                if (!mage.Laboratory.HasFeature(LabFeatures.HighlyOrganized))
                {
                    mage.Log.Add("Through meticulous work, the lab is now Highly Organized.");
                    mage.Laboratory.AddFeature(LabFeatures.HighlyOrganized);
                }
            }

            // Bonus: As per the book, also check for the Spotless Virtue based on a Personality Trait roll.
            // This requires adding a "Tidy" trait or similar to the character model.
            // For now, we will simulate it with an Intelligence roll.
            if (Die.Instance.RollSimpleDie() + mage.GetAttributeValue(AttributeType.Intelligence) >= 9)
            {
                if (!mage.Laboratory.HasFeature(LabFeatures.Spotless))
                {
                    mage.Log.Add("The lab is now kept in a state of immaculate cleanliness.");
                    mage.Laboratory.AddFeature(LabFeatures.Spotless);
                }
            }
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
