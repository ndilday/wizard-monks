using WizardMonks.Models.Spells;

namespace WizardMonks.Models
{
    public class LabText : AWritable
    {
        public Spell SpellContained { get; set; }
        public bool IsShorthand { get; set; }
    }
}
