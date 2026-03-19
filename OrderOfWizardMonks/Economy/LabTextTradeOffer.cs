using WizardMonks.Models.Books;
using WizardMonks.Models.Characters;

namespace WizardMonks.Economy
{
    public class LabTextTradeOffer
    {
        public HermeticMagus Mage { get; private set; }
        public LabText LabTextOffered { get; private set; }
        public LabText LabTextDesired { get; private set; }

        public LabTextTradeOffer(HermeticMagus mage, LabText labTextOffered, LabText labTextDesired)
        {
            Mage = mage;
            LabTextOffered = labTextOffered;
            LabTextDesired = labTextDesired;
        }
    }
}