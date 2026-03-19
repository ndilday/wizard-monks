using WizardMonks.Models.Books;
using WizardMonks.Models.Characters;

namespace WizardMonks.Economy
{
    public class BookTradeOffer
    {
        public HermeticMagus Mage { get; private set; }
        public ABook BookOffered { get; private set; }
        public ABook BookDesired { get; private set; }
        public BookTradeOffer(HermeticMagus mage, ABook bookOffered, ABook bookDesired)
        {
            Mage = mage;
            BookOffered = bookOffered;
            BookDesired = bookDesired;
        }
    }
}
