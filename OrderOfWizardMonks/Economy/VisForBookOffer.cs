using System.Collections.Generic;
using System.Linq;
using WizardMonks.Instances;

namespace WizardMonks.Economy
{
    public class VisForBookOffer
    {
        public Magus TradingPartner { get; private set; }
        public List<VisOffer> VisOffers { get; private set; }
        public double VisValue { get; private set; }
        public ABook BookDesired { get; private set; }
        public VisForBookOffer(Magus buyer, IEnumerable<VisOffer> visOffers, double quantity, ABook bookDesired)
        {
            TradingPartner = buyer;
            VisOffers = visOffers.ToList();
            BookDesired = bookDesired;
            VisValue = CalculateVisValue();
        }

        private double CalculateVisValue()
        {
            double total = 0;
            foreach (VisOffer offer in VisOffers)
            {
                if (MagicArts.IsTechnique(offer.Art))
                {
                    total += offer.Quantity * 4.0;
                }
                else if (offer.Art != MagicArts.Vim)
                {
                    total += offer.Quantity * 2.0;
                }
                else
                {
                    total += offer.Quantity;
                }
            }
            return total;
        }
    }
}
