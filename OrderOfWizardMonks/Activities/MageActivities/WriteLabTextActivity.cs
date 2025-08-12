using WizardMonks.Models.Books;
using WizardMonks.Models.Spells;

namespace WizardMonks.Activities.MageActivities
{
    public class WriteLabTextActivity : AExposingMageActivity
    {
        public Spell SpellToWrite { get; private set; }

        public WriteLabTextActivity(Spell spell, Ability exposure, double desire)
            : base(exposure, desire)
        {
            Action = Activity.WriteLabText;
            SpellToWrite = spell;
        }

        protected override void DoMageAction(Magus mage)
        {
            double progressThisSeason = mage.GetLabTextWritingRate();

            if (progressThisSeason >= SpellToWrite.Level)
            {
                // Finished the lab text
                LabText newText = new LabText
                {
                    Author = mage,
                    SpellContained = SpellToWrite,
                    IsShorthand = false // This is a clean, shareable copy
                };
                mage.AddLabTextToCollection(newText);
                mage.Log.Add($"Completed writing a lab text for '{SpellToWrite.Name}'.");
            }
        }

        public override bool Matches(IActivity action)
        {
            if (action is not WriteLabTextActivity writeAction)
            {
                return false;
            }
            return writeAction.SpellToWrite == this.SpellToWrite;
        }

        public override string Log()
        {
            return $"Writing lab text for '{SpellToWrite.Name}' worth {Desire:0.000}";
        }
    }
}