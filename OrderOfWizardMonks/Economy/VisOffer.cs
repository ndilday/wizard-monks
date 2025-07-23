namespace WizardMonks.Economy
{
    public class VisOffer
    {
        public Ability Art { get; private set; }
        public double Quantity { get; private set; }
        public VisOffer(Ability art, double amount)
        {
            Art = art;
            Quantity = amount;
        }
    }
}
