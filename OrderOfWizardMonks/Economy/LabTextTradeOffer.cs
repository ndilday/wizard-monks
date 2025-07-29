namespace WizardMonks.Economy
{
    public class LabTextTradeOffer
    {
        public Magus Mage { get; private set; }
        public LabText LabTextOffered { get; private set; }
        public LabText LabTextDesired { get; private set; }

        public LabTextTradeOffer(Magus mage, LabText labTextOffered, LabText labTextDesired)
        {
            Mage = mage;
            LabTextOffered = labTextOffered;
            LabTextDesired = labTextDesired;
        }
    }
}