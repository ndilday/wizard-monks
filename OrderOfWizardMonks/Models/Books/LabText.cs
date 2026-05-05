using WizardMonks.Models.Projects;
using WizardMonks.Models.Spells;

namespace WizardMonks.Models.Books
{
    public class LabText : AWritable
    {
        public Spell SpellContained { get; set; }
        public bool IsShorthand { get; set; }

        /// <summary>
        /// Set when this lab text was produced during original research.
        /// A mage who learns this spell gains breakthrough points toward the
        /// linked breakthrough, provided it is not already integrated into
        /// their tradition.
        /// </summary>
        public BreakthroughDefinition LinkedBreakthrough { get; set; }
    }
}
