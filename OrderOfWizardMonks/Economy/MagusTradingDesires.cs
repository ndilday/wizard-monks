using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Instances;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Spells;
using WizardMonks.Services.Characters;

namespace WizardMonks.Economy
{
    public class MagusTradingDesires
    {
        public VisDesire[] VisDesires { get; private set; }
        public Dictionary<Ability, BookDesire> BookDesires { get; private set; }
        public IEnumerable<BookForTrade> BooksForTrade { get; private set; }
        public IEnumerable<LabTextForTrade> LabTextsForTrade { get; private set; }
        public Dictionary<SpellBase, LabTextDesire> LabTextDesires { get; private set; }
        public Magus Mage { get; private set; }

        public MagusTradingDesires(Magus magus, VisDesire[] visDesires, 
                                   IEnumerable<BookDesire> booksDesired, IEnumerable<BookForTrade> booksForTrade,
                                   IEnumerable<LabTextDesire> labTextsDesired, IEnumerable<LabTextForTrade> labTextsForTrade)
        {
            Mage = magus;
            VisDesires = visDesires;
            BookDesires = booksDesired.ToDictionary(l => l.Ability);
            LabTextsForTrade = labTextsForTrade;
            BooksForTrade = booksForTrade;
            LabTextDesires = labTextsDesired.ToDictionary(l => l.SpellBase);
        }

        public IList<VisTradeOffer> GenerateVisOffers(MagusTradingDesires otherDesires)
        {
            // handle vis trades
            return VisForVis(otherDesires.Mage, otherDesires.VisDesires);
        }

        public IList<BookTradeOffer> GenerateBookTradeOffers(MagusTradingDesires tradeDesires)
        {
            List<BookTradeOffer> tradeList = new();
            foreach (BookForTrade book in BooksForTrade)
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
                        double price = Math.Round(bookVisValue + bookForTrade.MinimumPrice + 0.5, 0) / 2.0;
                        var visOffers = GenerateVisOffer(price, otherDesires.VisDesires, VisDesires);

                        if (visOffers != null)
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
                        double price = Math.Round(bookVisValue + bookForTrade.MinimumPrice + 0.5, 0) / 2.0;
                        var offer = GenerateVisOffer(price, otherDesires.VisDesires, VisDesires);
                        if (offer != null)
                        {
                            bookTradeOffers.Add(new VisForBookOffer(otherDesires.Mage, offer, price, bookForTrade.Book));
                        }
                    }
                }
            }
            return bookTradeOffers;
        }

        public IList<VisForLabTextOffer> GenerateBuyLabTextOffers(MagusTradingDesires otherDesires)
        {
            List<VisForLabTextOffer> offers = new();
            if (LabTextDesires.Any() && otherDesires.LabTextsForTrade.Any())
            {
                // They have lab texts, we want lab texts
                foreach (LabTextForTrade labTextForTrade in otherDesires.LabTextsForTrade)
                {
                    // If we're interested in the spell base of this lab text...
                    if (LabTextDesires.TryGetValue(labTextForTrade.LabText.SpellContained.Base, out var desire) &&
                        desire.MinimumLevel <= labTextForTrade.LabText.SpellContained.Level &&
                        desire.MaximumLevel >= labTextForTrade.LabText.SpellContained.Level)
                    {
                        // ...evaluate its value to us.
                        double labTextVisValue = Mage.RateLifetimeLabTextValue(labTextForTrade.LabText);
                        if (labTextVisValue > 0)
                        {
                            // Improve pricing mechanics later, for now value + minimum price
                            double price = Math.Round(labTextVisValue + labTextForTrade.MinimumPrice + 0.5, 0) / 2.0;
                            var visPayment = GenerateVisOffer(price, otherDesires.VisDesires, this.VisDesires);

                            if (visPayment != null)
                            {
                                // We can afford it, so make an offer.
                                offers.Add(new VisForLabTextOffer(otherDesires.Mage, visPayment, price, labTextForTrade.LabText));
                            }
                        }
                    }
                }
            }
            return offers;
        }

        /// <summary>
        /// Generates offers to sell lab texts from this magus to another magus who desires them.
        /// </summary>
        /// <param name="otherDesires">The trading desires of the potential buyer.</param>
        /// <returns>A list of VisForLabTextOffer where this magus is the seller and the otherDesires.Mage is the buyer.</returns>
        public IList<VisForLabTextOffer> GenerateSellLabTextOffers(MagusTradingDesires otherDesires)
        {
            List<VisForLabTextOffer> offers = new();
            if (LabTextsForTrade.Any() && otherDesires.LabTextDesires.Any())
            {
                // We have lab texts to sell (LabTextsForTrade), and they (otherDesires) desire lab texts.
                foreach (LabTextForTrade myLabTextForTrade in LabTextsForTrade)
                {
                    // Check if the other magus is interested in this specific lab text's spell base.
                    if (otherDesires.LabTextDesires.TryGetValue(myLabTextForTrade.LabText.SpellContained.Base, out var theirDesire))
                    {
                        // Ensure the lab text offered is actually better than what they currently know/desire.
                        if (theirDesire.MinimumLevel <= myLabTextForTrade.LabText.SpellContained.Level && theirDesire.MaximumLevel >= myLabTextForTrade.LabText.SpellContained.Level)
                        {
                            // Evaluate the value of this lab text to the *other* magus (the potential buyer).
                            double theirValuation = otherDesires.Mage.RateLifetimeLabTextValue(myLabTextForTrade.LabText);

                            // Only proceed if the lab text has real value to the buyer.
                            if (theirValuation > 0)
                            {
                                // Calculate a proposed price based on their valuation and our minimum acceptable price.
                                // We round to the nearest half-vis to make trade quantities more manageable.
                                double price = Math.Round(theirValuation + myLabTextForTrade.MinimumPrice + 0.5, 0) / 2.0;

                                // Generate the vis payment offer from the buyer's (otherDesires.Mage) perspective.
                                // We ask what vis they can offer that we desire (this.VisDesires) in exchange for the vis they want to use (otherDesires.VisDesires).
                                var visPayment = otherDesires.GenerateVisOffer(price, this.VisDesires, otherDesires.VisDesires);

                                if (visPayment != null)
                                {
                                    // If they can make a valid vis payment, add this as a sell offer.
                                    // The 'buyer' in this offer is the 'otherDesires.Mage'.
                                    offers.Add(new VisForLabTextOffer(otherDesires.Mage, visPayment, price, myLabTextForTrade.LabText));
                                }
                            }
                        }
                    }
                }
            }
            return offers;
        }

        public IList<LabTextTradeOffer> GenerateLabTextTradeOffers(MagusTradingDesires otherDesires)
        {
            List<LabTextTradeOffer> offers = new();
            if (LabTextsForTrade.Any() && otherDesires.LabTextsForTrade.Any())
            {
                foreach (var myTextForTrade in LabTextsForTrade)
                {
                    // Check if the other party desires our text
                    if (otherDesires.LabTextDesires.TryGetValue(myTextForTrade.LabText.SpellContained.Base, out var theirDesire) &&
                        theirDesire.MinimumLevel <= myTextForTrade.LabText.SpellContained.Level &&
                        theirDesire.MaximumLevel >= myTextForTrade.LabText.SpellContained.Level)
                    {
                        foreach (var theirTextForTrade in otherDesires.LabTextsForTrade)
                        {
                            // Check if we desire their text
                            if (LabTextDesires.TryGetValue(theirTextForTrade.LabText.SpellContained.Base, out var myDesire) &&
                                myDesire.MinimumLevel <= theirTextForTrade.LabText.SpellContained.Level &&
                                myDesire.MaximumLevel >= theirTextForTrade.LabText.SpellContained.Level)
                            {
                                // Both parties are interested. Evaluate if the trade is equitable.
                                double myValuationOfTheirText = Mage.RateLifetimeLabTextValue(theirTextForTrade.LabText);
                                double theirValuationOfMyText = otherDesires.Mage.RateLifetimeLabTextValue(myTextForTrade.LabText);

                                // If the values are within 25% of each other, consider it a fair swap.
                                if (Math.Abs(myValuationOfTheirText - theirValuationOfMyText) < (myValuationOfTheirText * 0.25))
                                {
                                    offers.Add(new LabTextTradeOffer(otherDesires.Mage, myTextForTrade.LabText, theirTextForTrade.LabText));
                                }
                            }
                        }
                    }
                }
            }
            return offers;
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
            foreach (VisDesire desire in receiverVisDesires.Where(d => d.Quantity > 0).OrderByDescending(d => d.Quantity))
            {
                var giverArt = giverVisDesires.First(d => d.Art == desire.Art);
                // see if the giver can supply this type of vis
                if (giverArt.Quantity < 0)
                {
                    // they have some to trade
                    double maxNeed = remainingPrice;
                    if (MagicArts.IsTechnique(desire.Art))
                    {
                        maxNeed /= 4.0;
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
                            remainingPrice -= giverArt.Quantity * -4;
                        }
                    }
                    else if (desire.Art != MagicArts.Vim)
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
            if (remainingPrice > 0)
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
            for (byte i = 0; i < MagicArts.Count; i++)
            {
                if (VisDesires[i].Quantity < 0 && otherVisDesires[i].Quantity > 0)
                {
                    // we have a surplus, they want it
                    if (Math.Abs(VisDesires[i].Quantity) > otherVisDesires[i].Quantity)
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
                if (VisDesires[i].Quantity > 0 && otherVisDesires[i].Quantity < 0)
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
}
