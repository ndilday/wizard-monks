namespace WizardMonks.Economy
{
    public class BookForTrade
    {
        public ABook Book { get; private set; }
        public double MinimumPrice { get; private set; }
        public BookForTrade(ABook book, double minPrice)
        {
            Book = book;
            MinimumPrice = minPrice;
        }
    }
}
