using WizardMonks.Models.Characters;
using WizardMonks.Services.Characters;

namespace WizardMonks.Economy
{
    public class VisTradeOffer
    {
        public HermeticMagus Mage { get; private set; }
        public VisOffer Bid { get; private set; }
        public VisOffer Ask { get; private set; }
        public VisTradeOffer(HermeticMagus mage, VisOffer bid, VisOffer ask)
        {
            Mage = mage;
            Bid = bid;
            Ask = ask;
        }

        public void Execute()
        {
            Mage.GainVis(Bid.Art, Bid.Quantity);
            Mage.UseVis(Ask.Art, Ask.Quantity);
        }
    }
}
