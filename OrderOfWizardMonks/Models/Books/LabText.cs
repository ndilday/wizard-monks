using WizardMonks.Models.Spells;

namespace WizardMonks.Models.Books
{
    public class LabText : AWritable
    {
        public Spell SpellContained { get; set; }
        public bool IsShorthand { get; set; }
    }
}
