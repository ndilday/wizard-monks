using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks
{
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
        public BookDesire(Ability ability, double curLevel = 0)
        {
            Ability = ability;
            CurrentLevel = curLevel;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(BookDesire))
            {
                return false;
            }
            BookDesire bookDesire = (BookDesire)obj;
            return bookDesire.Ability == this.Ability;
        }

        public override int GetHashCode()
        {
            return Ability.GetHashCode();
        }
    }

    public class BookForTrade
    {
        public IBook Book { get; private set; }
        public double MinimumPrice { get; private set; }
        public BookForTrade(IBook book, double minPrice)
        {
            Book = book;
            MinimumPrice = minPrice;
        }
    }

    public class BookTradeOffer
    {
        public Magus Mage { get; private set; }
        public IBook BookOffered { get; private set; }
        public IBook BookDesired { get; private set; }
        public BookTradeOffer(Magus mage, IBook bookOffered, IBook bookDesired)
        {
            Mage = mage;
            BookOffered = bookOffered;
            BookDesired = bookDesired;
        }
    }

    public class BookVisOffer
    {
        public Magus Mage { get; private set; }
        public Ability VisArt { get; private set; }
        public double VisQuantity { get; private set; }
        public IBook BookDesired { get; private set; }
        public BookVisOffer(Magus mage, Ability art, double quantity, IBook bookDesired)
        {
            Mage = mage;
            VisArt = art;
            VisQuantity = quantity;
            BookDesired = bookDesired;
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
            List<BookTradeOffer> tradeList = new List<BookTradeOffer>();
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
                            theirBook.Book.Quality == book.Book.Quality)
                        {
                            tradeList.Add(new BookTradeOffer(tradeDesires.Mage, book.Book, theirBook.Book));
                        }
                    }
                }
            }
            return tradeList;
        }

        public IList<BookVisOffer> GenerateBuyBookOffers(MagusTradingDesires otherDesires)
        {
            List<BookVisOffer> bookTradeOffers = new List<BookVisOffer>();
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
                        double price = bookVisValue + bookForTrade.MinimumPrice / 2;
                        for (int i = 0; i < 15; i++)
                        {
                            if (-(VisDesires[i].Quantity) >= price && otherDesires.VisDesires[i].Quantity >= price)
                            {
                                // we can offer this sort of vis for the book
                                bookTradeOffers.Add(new BookVisOffer(otherDesires.Mage, VisDesires[i].Art, price, bookForTrade.Book));
                            }
                        }
                    }
                }
            }
            return bookTradeOffers;
        }

        public IList<BookVisOffer> GenerateSellBookOffers(MagusTradingDesires otherDesires)
        {
            List<BookVisOffer> bookTradeOffers = new List<BookVisOffer>();
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
                        double price = bookVisValue + bookForTrade.MinimumPrice / 2;
                        for (int i = 0; i < 15; i++)
                        {
                            if (-(otherDesires.VisDesires[i].Quantity) >= price && VisDesires[i].Quantity >= price)
                            {
                                // we can offer this sort of vis for the book
                                bookTradeOffers.Add(new BookVisOffer(otherDesires.Mage, VisDesires[i].Art, price, bookForTrade.Book));
                            }
                        }
                    }
                }
            }
            return bookTradeOffers;
        }

        private IList<VisTradeOffer> VisForVis(Magus mage, VisDesire[] otherVisDesires)
        {
            List<VisOffer> bids = new List<VisOffer>();
            List<VisOffer> asks = new List<VisOffer>();
            for(byte i = 0; i < 15; i++)
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
                List<VisTradeOffer> visTradeOffer = new List<VisTradeOffer>();
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

    public class Economy
    {   
    }
}
