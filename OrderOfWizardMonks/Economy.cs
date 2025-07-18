using System;
using System.Collections.Generic;
using System.Linq;

using WizardMonks.Instances;

namespace WizardMonks
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

    public class VisTradeOffer
    {
        public Magus Mage { get; private set; }
        public VisOffer Bid { get; private set; }
        public VisOffer Ask { get; private set; }
        public VisTradeOffer(Magus mage, VisOffer bid, VisOffer ask)
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

    public class BookDesire
    {
        public Ability Ability { get; private set; }
        public double CurrentLevel { get; private set; }
        public Character Character { get; private set; }
        public BookDesire(Character character, Ability ability, double curLevel = 0)
        {
            Ability = ability;
            CurrentLevel = curLevel;
            Character = character;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(BookDesire))
            {
                return false;
            }
            BookDesire bookDesire = (BookDesire)obj;
            return bookDesire.Ability == this.Ability && bookDesire.Character == this.Character;
        }

        public override int GetHashCode()
        {
            return Ability.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Ability.ToString()} {CurrentLevel.ToString()}";
        }
    }

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
            foreach(VisOffer offer in VisOffers)
            {
                if(MagicArts.IsTechnique(offer.Art))
                {
                    total += offer.Quantity * 4.0;
                }
                else if(offer.Art != MagicArts.Vim)
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

    public class MagusTradingDesires
    {
        public VisDesire[] VisDesires { get; private set; }
        public Dictionary<Ability, BookDesire> BookDesires { get; private set; }
        public IEnumerable<BookForTrade> BooksForTrade { get; private set; }
        public Magus Mage { get; private set; }

        public MagusTradingDesires(Magus magus, VisDesire[] visDesires, IEnumerable<BookDesire> booksDesired, IEnumerable<BookForTrade> booksForTrade)
        {
            Mage = magus;
            VisDesires = visDesires;
            BookDesires = booksDesired.ToDictionary(l => l.Ability);
            BooksForTrade = booksForTrade;
        }

        public IList<VisTradeOffer> GenerateVisOffers(MagusTradingDesires otherDesires)
        {
            // handle vis trades
            return VisForVis(otherDesires.Mage, otherDesires.VisDesires);
        }

        public IList<BookTradeOffer> GenerateBookTradeOffers(MagusTradingDesires tradeDesires)
        {
            List<BookTradeOffer> tradeList = new();
            foreach(BookForTrade book in BooksForTrade)
            {
                if (tradeDesires.BookDesires.ContainsKey(book.Book.Topic) &&
                    tradeDesires.Mage.ValidToRead(book.Book))
                {
                    // we have a book they want, see if they have a book we want
                    foreach (BookForTrade theirBook in tradeDesires.BooksForTrade)
                    {
                        if (BookDesires.ContainsKey(theirBook.Book.Topic) &&
                            Mage.ValidToRead(theirBook.Book) &&
                            theirBook.Book.Quality == book.Book.Quality &&
                            theirBook.Book.Level <= book.Book.Level + 1.0 &&
                            theirBook.Book.Level >= book.Book.Level - 1.0)
                        {
                            tradeList.Add(new BookTradeOffer(tradeDesires.Mage, book.Book, theirBook.Book));
                        }
                    }
                }
            }
            return tradeList;
        }

        public IList<VisForBookOffer> GenerateBuyBookOffers(MagusTradingDesires otherDesires)
        {
            List<VisForBookOffer> bookTradeOffers = new();
            if (BookDesires.Any() && otherDesires.BooksForTrade.Any())
            {
                // they have books, we want books
                foreach (BookForTrade bookForTrade in otherDesires.BooksForTrade)
                {
                    // if we're interested in the topic of this book and it's of sufficient level
                    if (BookDesires.ContainsKey(bookForTrade.Book.Topic) && 
                        BookDesires[bookForTrade.Book.Topic].CurrentLevel < bookForTrade.Book.Level &&
                        Mage.ValidToRead(bookForTrade.Book))
                    {
                        // evaluate the value of the book to us
                        double bookVisValue = Mage.RateLifetimeBookValue(bookForTrade.Book);
                        // TODO: improve pricing mechanics
                        // rounding up to the nearest half-vis
                        double price = Math.Round((bookVisValue + bookForTrade.MinimumPrice) + 0.5,0) / 2.0;
                        var visOffers = GenerateVisOffer(price, otherDesires.VisDesires, VisDesires);

                        if(visOffers != null)
                        {
                            // we can offer this sort of vis for the book
                            bookTradeOffers.Add(new VisForBookOffer(otherDesires.Mage, visOffers, price, bookForTrade.Book));
                        }
                    }
                }
            }
            return bookTradeOffers;
        }

        public IList<VisForBookOffer> GenerateSellBookOffers(MagusTradingDesires otherDesires)
        {
            List<VisForBookOffer> bookTradeOffers = new();
            if (BooksForTrade.Any() && otherDesires.BookDesires.Any())
            {
                // we have books, they want books
                foreach (BookForTrade bookForTrade in BooksForTrade)
                {
                    // if we're interested in the topic of this book and it's of sufficient level
                    if (otherDesires.BookDesires.ContainsKey(bookForTrade.Book.Topic) && 
                        otherDesires.BookDesires[bookForTrade.Book.Topic].CurrentLevel < bookForTrade.Book.Level &&
                        otherDesires.Mage.ValidToRead(bookForTrade.Book))
                    {
                        // evaluate the value of the book to them
                        double bookVisValue = otherDesires.Mage.RateLifetimeBookValue(bookForTrade.Book);
                        // TODO: improve pricing mechanics
                        // rounding up to the nearest half-vis
                        double price = Math.Round((bookVisValue + bookForTrade.MinimumPrice) + 0.5, 0) / 2.0;
                        var offer = GenerateVisOffer(price, otherDesires.VisDesires, VisDesires);
                        if(offer != null)
                        {
                            bookTradeOffers.Add(new VisForBookOffer(otherDesires.Mage, offer, price, bookForTrade.Book));
                        }
                    }
                }
            }
            return bookTradeOffers;
        }

        /// <summary>
        /// Looks at the giver and receiver vis desires, and sees what combination of vis can meet the desires
        /// </summary>
        /// <param name="price">the agreed upon price, in Vim vis</param>
        /// <param name="giverVisDesires"></param>
        /// <param name="receiverVisDesires"></param>
        /// <returns></returns>
        private IEnumerable<VisOffer> GenerateVisOffer(double price, VisDesire[] giverVisDesires, VisDesire[] receiverVisDesires)
        {
            double remainingPrice = price;
            List<VisOffer> offerSegments = new();
            // order the receiver's desires from largest to smallest
            foreach(VisDesire desire in receiverVisDesires.Where(d => d.Quantity > 0).OrderByDescending(d => d.Quantity))
            {
                var giverArt = giverVisDesires.First(d => d.Art == desire.Art);
                // see if the giver can supply this type of vis
                if (giverArt.Quantity < 0 )
                {
                    // they have some to trade
                    double maxNeed = remainingPrice;
                    if(MagicArts.IsTechnique(desire.Art))
                    {
                        maxNeed /= 4.0;
                        if(giverArt.Quantity * -1 >= maxNeed)
                        {
                            offerSegments.Add(new VisOffer(desire.Art, maxNeed));
                            remainingPrice = 0;
                            break;
                        }
                        else
                        {
                            // we're going to need more than this, so use all of this vis and move on
                            offerSegments.Add(new VisOffer(desire.Art, giverArt.Quantity * -1));
                            remainingPrice -= giverArt.Quantity * -4;
                        }
                    }
                    else if(desire.Art != MagicArts.Vim)
                    {
                        maxNeed /= 2.0;
                        if (giverArt.Quantity * -1 >= maxNeed)
                        {
                            offerSegments.Add(new VisOffer(desire.Art, maxNeed));
                            remainingPrice = 0;
                            break;
                        }
                        else
                        {
                            // we're going to need more than this, so use all of this vis and move on
                            offerSegments.Add(new VisOffer(desire.Art, giverArt.Quantity * -1));
                            remainingPrice -= giverArt.Quantity * -2;
                        }
                    }
                }
            }
            if(remainingPrice > 0)
            {
                return null;
            }
            return offerSegments;
        }

        private IList<VisTradeOffer> VisForVis(Magus mage, VisDesire[] otherVisDesires)
        {
            // TODO: need to take Vis type into account
            List<VisOffer> bids = new();
            List<VisOffer> asks = new();
            for(byte i = 0; i < MagicArts.Count; i++)
            {
                if(VisDesires[i].Quantity < 0 && otherVisDesires[i].Quantity > 0)
                {
                    // we have a surplus, they want it
                    if(Math.Abs(VisDesires[i].Quantity) > otherVisDesires[i].Quantity)
                    {
                        // our surplus is larger than their need; fulfill the need
                        bids.Add(new VisOffer(otherVisDesires[i].Art, otherVisDesires[i].Quantity));
                    }
                    else
                    {
                        // our surplus is smaller than their need; empty our stocks
                        bids.Add(new VisOffer(otherVisDesires[i].Art, Math.Abs(VisDesires[i].Quantity)));
                    }

                }
                if(VisDesires[i].Quantity > 0 && otherVisDesires[i].Quantity < 0)
                {
                    // they have a surplus, we want it
                    if (Math.Abs(otherVisDesires[i].Quantity) > VisDesires[i].Quantity)
                    {
                        // their surplus is larger than our need; fulfill the need
                        asks.Add(new VisOffer(otherVisDesires[i].Art, VisDesires[i].Quantity));
                    }
                    else
                    {
                        // our surplus is smaller than their need; empty our stocks
                        asks.Add(new VisOffer(otherVisDesires[i].Art, Math.Abs(otherVisDesires[i].Quantity)));
                    }
                }
            }
            // we should now have a list of bids and asks
            // we need at least one bid and one ask to make a deal
            if (bids.Count() > 0 && asks.Count() > 0)
            {
                List<VisTradeOffer> visTradeOffer = new();
                foreach (VisOffer bid in bids)
                {
                    foreach (VisOffer ask in asks)
                    {
                        // figure out which quantity is smaller, and make the larger one equal to that
                        if (bid.Quantity > ask.Quantity)
                        {
                           visTradeOffer.Add(new VisTradeOffer(mage, new VisOffer(bid.Art, ask.Quantity), ask));
                        }
                        else
                        {
                            visTradeOffer.Add(new VisTradeOffer(mage, bid, new VisOffer(ask.Art, bid.Quantity)));
                        }
                    }
                }
                return visTradeOffer;
            }
            return null;
        }
    }

    // in the future, we should probably have ad hoc micro-economies, 
    // and the possibility of someone profiting by trading among unconnected micro-economies
    public static class GlobalEconomy
    {
        // needs to know about all books available for trade
        public static Dictionary<Ability, List<BookForTrade>> BooksForTradeByTopicMap = new();
        // needs to know about all topics people have expressed wanting a book for
        public static List<BookDesire> DesiredBooksList = new();
        // needs to know about all vis desires
        public static double[] GlobalVisDemandMap = new double[MagicArts.Count];
        // needs to have some sense of the average value of a tractatus
        public static double GlobalTractatusValue = 2;

        public static IEnumerable<Ability> DesiredBookTopics
        {
            get
            {
                return DesiredBooksList.Select(b => b.Ability).Distinct();
            }
        }
    }
}
