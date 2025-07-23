namespace WizardMonks.Economy
{
    /// <summary>
    /// Container used in vis trades. Negative Quantities represent vis available to trade away
    /// </summary>
    public class VisDesire
    {
        public Ability Art { get; private set; }
        // negative quantities represent vis available for trade
        public double Quantity { get; set; }
        public VisDesire(Ability art, double quantity = 0)
        {
            Art = art;
            Quantity = quantity;
        }
    }
}
