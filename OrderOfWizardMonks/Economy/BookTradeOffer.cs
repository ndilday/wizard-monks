using WizardMonks.Models;

namespace WizardMonks.Economy
{
    public class BookTradeOffer
    {
        public Magus Mage { get; private set; }
        public ABook BookOffered { get; private set; }
        public ABook BookDesired { get; private set; }
        public BookTradeOffer(Magus mage, ABook bookOffered, ABook bookDesired)
        {
            Mage = mage;
            BookOffered = bookOffered;
            BookDesired = bookDesired;
        }
    }
}
