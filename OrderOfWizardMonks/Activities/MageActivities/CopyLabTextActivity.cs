
namespace WizardMonks.Activities.MageActivities
{
    public class CopyLabTextActivity : AExposingMageActivity
    {
        public LabText TextToCopy { get; private set; }

        public CopyLabTextActivity(LabText labText, Ability exposure, double desire) : base(exposure, desire)
        {
            Action = Activity.CopyLabText;
            TextToCopy = labText;
        }

        protected override void DoMageAction(Magus mage)
        {
            double progressThisSeason = mage.GetLabTextCopyingRate();

            if (progressThisSeason >= TextToCopy.SpellContained.Level)
            {
                // Finished the copy
                LabText newCopy = new LabText
                {
                    Author = TextToCopy.Author, // The original author is preserved
                    SpellContained = TextToCopy.SpellContained,
                    IsShorthand = false // Copies are always clean
                };
                mage.AddLabTextToCollection(newCopy);
                mage.Log.Add($"Completed copying the lab text for '{TextToCopy.SpellContained.Name}'.");
            }
        }

        public override bool Matches(IActivity action)
        {
            if (action is not CopyLabTextActivity copyAction)
            {
                return false;
            }
            return copyAction.TextToCopy == this.TextToCopy;
        }

        public override string Log()
        {
            return $"Copying lab text '{TextToCopy.SpellContained.Name}' worth {Desire:0.000}";
        }
    }
}