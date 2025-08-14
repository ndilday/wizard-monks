using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Economy;
using WizardMonks.Instances;
using WizardMonks.Models.Books;
using WizardMonks.Models.Characters;

namespace WizardMonks.Services.Characters
{
    public static class CharacterTradingService
    {
        public static IEnumerable<BookForTrade> EvaluateBookValuesAsSeller(this Magus mage, IEnumerable<ABook> books)
        {
            List<BookForTrade> list = new();
            double distillRate = mage.GetVisDistillationRate();
            foreach (ABook book in books)
            {
                if (book.Level == 1000)
                {
                    list.Add(new BookForTrade(book, distillRate));
                }
                else
                {
                    double writeRate = mage.GetAbility(mage.WritingLanguage).Value + mage.GetAttribute(AttributeType.Communication).Value;
                    double seasons = book.Level / writeRate;
                    if (!MagicArts.IsArt(book.Topic))
                    {
                        seasons *= 5;
                    }
                    list.Add(new BookForTrade(book, distillRate * seasons));
                }
            }
            return list;
        }

        public static IEnumerable<LabTextForTrade> EvaluateLabTextValuesAsSeller(this Magus mage, IEnumerable<LabText> labTexts)
        {
            List<LabTextForTrade> list = [];
            double distillRate = mage.GetVisDistillationRate();
            foreach (LabText labText in labTexts)
            {
                if (labText.IsShorthand)
                {
                    // the lab text was generated for free as part of inventing the spell
                    list.Add(new LabTextForTrade(labText, Math.Min(distillRate, 0.5)));
                }
                else
                {
                    // the lab text was manually written, worth Level * distill rate / (Latin * 20)
                    double writeRate = mage.GetAbility(mage.WritingLanguage).Value * 20;
                    list.Add(new LabTextForTrade(labText, distillRate * labText.SpellContained.Level / writeRate));
                }
            }
            return list;
        }

        public static MagusTradingDesires GenerateTradingDesires(this Magus mage)
        {
            mage.UpdateVisDesiresWithStock();
            var tradeDesires = new MagusTradingDesires(
                mage,
                mage.Desires.VisDesires,
                mage.Desires.BookDesires,
                mage.EvaluateBookValuesAsSeller(mage.GetUnneededBooksFromCollection()),
                mage.Desires.LabTextDesires,
                mage.EvaluateLabTextValuesAsSeller(mage.GetUnneededLabTextsFromCollection())
            );
            if (tradeDesires == null)
            {
                throw new NullReferenceException();
            }
            return tradeDesires;
        }

        public static void UpdateVisDesiresWithStock(this Magus mage)
        {
            foreach (VisDesire visDesire in mage.Desires.VisDesires)
            {
                if (mage.VisStock.ContainsKey(visDesire.Art))
                {
                    visDesire.Quantity -= mage.VisStock[visDesire.Art];
                }
            }
        }

        public static void EvaluateTradingDesires(this Magus mage, IEnumerable<MagusTradingDesires> mageTradeDesires)
        {
            List<VisTradeOffer> visTradeOffers = new();
            List<BookTradeOffer> bookTradeOffers = new();
            //List<VisForBookOffer> buyBookOffers = new();
            List<VisForBookOffer> sellBookOffers = new();
            List<VisForLabTextOffer> sellLabTextOffers = new();
            List<LabTextTradeOffer> labTextTradeOffers = new();
            var thisMageDesires = mageTradeDesires.First(d => d.Mage == mage);
            foreach (MagusTradingDesires tradeDesires in mageTradeDesires)
            {
                if (tradeDesires.Mage == mage)
                {
                    continue;
                }

                var bookTrades = thisMageDesires.GenerateBookTradeOffers(tradeDesires);
                bookTradeOffers.AddRange(bookTrades);

                /*var bookBuyOffers = _tradeDesires.GenerateBuyBookOffers(tradeDesires);
                if (bookBuyOffers != null)
                {
                    buyBookOffers.AddRange(bookBuyOffers);
                }*/

                var bookSellOffers = thisMageDesires.GenerateSellBookOffers(tradeDesires);
                if (bookSellOffers != null)
                {
                    sellBookOffers.AddRange(bookSellOffers);
                }

                var visOffersGenerated = thisMageDesires.GenerateVisOffers(tradeDesires);
                if (visOffersGenerated != null)
                {
                    visTradeOffers.AddRange(visOffersGenerated);
                }

                var labTextSales = thisMageDesires.GenerateSellLabTextOffers(tradeDesires);
                if (labTextSales != null) sellLabTextOffers.AddRange(labTextSales);

                var labTextSwaps = thisMageDesires.GenerateLabTextTradeOffers(tradeDesires);
                if (labTextSwaps != null) labTextTradeOffers.AddRange(labTextSwaps);
            }

            mage.ProcessVisOffers(visTradeOffers, thisMageDesires);
            mage.ProcessBookSwaps(bookTradeOffers);
            mage.ProcessBookSales(sellBookOffers);
            //ProcessBookPurchases(buyBookOffers);
            mage.ProcessLabTextSwaps(labTextTradeOffers);
            mage.ProcessLabTextSales(sellLabTextOffers);
            // figure out book for book
        }

        private static void ProcessVisOffers(this Magus mage, List<VisTradeOffer> offers, MagusTradingDesires desires)
        {
            IEnumerable<VisTradeOffer> internalOffers = offers;
            // now we have to determine which offers to accept
            // as a first pass at an algorithm, 
            // we'll sort according to amount of vis needed, 
            // and attempt to fulfill trades on the vis type we need the most of

            // the front of the desires list should be the type we most want
            // the front of the stocks list should be what we most want to give up in trade
            do
            {
                var prioritizedVisDesires = desires.VisDesires.Where(v => v.Quantity > 0).OrderByDescending(v => v.Quantity);
                var prioritizedVisStocks = desires.VisDesires.Where(v => v.Quantity < 0).OrderBy(v => v.Quantity);
                var bestOffers = from offer in internalOffers
                                 join visDesire in desires.VisDesires on offer.Ask.Art equals visDesire.Art
                                 join visStock in desires.VisDesires on offer.Bid.Art equals visStock.Art
                                 orderby visDesire.Quantity descending, visStock.Quantity, offer.Ask.Quantity
                                 select offer;
                if (bestOffers.Any())
                {
                    var offer = bestOffers.First();
                    VisDesire mostDesired = desires.VisDesires[offer.Ask.Art.AbilityId % 300];
                    VisDesire mostOverstocked = desires.VisDesires[offer.Bid.Art.AbilityId % 300];
                    if (mage.GetVisCount(offer.Bid.Art) >= offer.Bid.Quantity && offer.Mage.GetVisCount(offer.Ask.Art) >= offer.Ask.Quantity)
                    {
                        mage.Log.Add("Executing vis trade with " + offer.Mage.Name);
                        mage.Log.Add("Trading " + offer.Bid.Quantity.ToString("0.000") + " pawns of " + offer.Bid.Art.AbilityName + " vis");
                        mage.Log.Add("for " + offer.Ask.Quantity.ToString("0.000") + " pawns of " + offer.Ask.Art.AbilityName + " vis");
                        offer.Mage.Log.Add("Executing vis trade with " + mage.Name);
                        offer.Mage.Log.Add("Trading " + offer.Ask.Quantity.ToString("0.000") + " pawns of " + offer.Ask.Art.AbilityName + " vis");
                        offer.Mage.Log.Add("for " + offer.Bid.Quantity.ToString("0.000") + " pawns of " + offer.Bid.Art.AbilityName + " vis");

                        offer.Execute();
                        internalOffers = internalOffers.Where(o => o != offer);
                        mage.GainVis(offer.Ask.Art, offer.Ask.Quantity);
                        mostDesired.Quantity -= offer.Ask.Quantity;
                        internalOffers = internalOffers.Where(o => o.Ask.Art != mostDesired.Art || o.Ask.Quantity <= mostDesired.Quantity);
                        mage.UseVis(offer.Bid.Art, offer.Bid.Quantity);
                        mostOverstocked.Quantity += offer.Bid.Quantity;
                        internalOffers = internalOffers.Where(o => o.Bid.Art != mostOverstocked.Art || o.Bid.Quantity <= Math.Abs(mostOverstocked.Quantity));
                    }
                    else
                    {
                        internalOffers = internalOffers.Where(o => o != offer);
                    }
                }
            } while (internalOffers.Any());
        }

        private static void ProcessBookSales(this Magus mage, IEnumerable<VisForBookOffer> bookSales)
        {
            var sales = bookSales.OrderByDescending(bts => bts.VisValue);
            while (sales.Any())
            {
                var sellOffer = sales.First();
                // Check if seller still owns the book and buyer can afford it.
                if (mage.GetBooksInCollection(sellOffer.BookDesired.Topic).Contains(sellOffer.BookDesired) &&
                    sellOffer.TradingPartner.HasSufficientVis(sellOffer.VisOffers))
                {
                    // Enact trade
                    mage.Log.Add("Selling " + sellOffer.BookDesired.Title + " to " + sellOffer.TradingPartner.Name);
                    mage.Log.Add("for " + sellOffer.VisValue.ToString("0.000") + " worth of vis");
                    sellOffer.TradingPartner.Log.Add("Buying " + sellOffer.BookDesired.Title + " from " + mage.Name);
                    sellOffer.TradingPartner.Log.Add("for " + sellOffer.VisValue.ToString("0.000") + " worth of vis");

                    // State changes are correctly handled by the participants.
                    sellOffer.TradingPartner.AddBookToCollection(sellOffer.BookDesired);
                    mage.RemoveBookFromCollection(sellOffer.BookDesired); // Seller removes from own inventory.
                    sellOffer.TradingPartner.UseVis(sellOffer.VisOffers);
                    mage.GainVis(sellOffer.VisOffers);
                }
                // This logic had a bug. If the 'if' fails, the loop becomes infinite.
                // It must be moved outside the 'if' block.
                sales = sales.Where(s => s.BookDesired != sellOffer.BookDesired).OrderBy(bto => bto.VisValue);
            }
        }

        private static void ProcessBookSwaps(this Magus mage, IEnumerable<BookTradeOffer> bookTradeOffers)
        {
            var trades = bookTradeOffers.OrderBy(bto => bto.BookDesired.Quality);
            while (trades.Any())
            {
                var tradeOffer = trades.First();
                if (mage.GetBooksInCollection(tradeOffer.BookOffered.Topic).Contains(tradeOffer.BookOffered) &&
                   tradeOffer.Mage.GetBooksInCollection(tradeOffer.BookDesired.Topic).Contains(tradeOffer.BookDesired))
                {
                    mage.Log.Add("Trading " + tradeOffer.BookOffered.Title + " to " + tradeOffer.Mage.Name);
                    mage.Log.Add("For " + tradeOffer.BookDesired.Title);
                    tradeOffer.Mage.Log.Add("Trading " + tradeOffer.BookDesired.Title + " to " + mage.Name);
                    tradeOffer.Mage.Log.Add("For " + tradeOffer.BookOffered.Title);

                    mage.AddBookToCollection(tradeOffer.BookDesired);
                    tradeOffer.Mage.RemoveBookFromCollection(tradeOffer.BookDesired);
                    mage.RemoveBookFromCollection(tradeOffer.BookOffered);
                    tradeOffer.Mage.AddBookToCollection(tradeOffer.BookOffered);
                    trades = trades.Where(bto => bto.BookDesired != tradeOffer.BookDesired &&
                                                 bto.BookOffered != tradeOffer.BookDesired)
                                   .OrderBy(bto => bto.BookDesired.Quality);
                }
                else
                {
                    trades = trades.Where(t => t != tradeOffer).OrderBy(bto => bto.BookDesired.Quality);
                }
            }
        }

        /// <summary>
        /// Processes offers from other magi to buy lab texts from this magus.
        /// This method is authoritative for executing lab text sales.
        /// </summary>
        /// <param name="offers">A collection of VisForLabTextOffer where this magus is the potential seller.</param>
        private static void ProcessLabTextSales(this Magus mage, IEnumerable<VisForLabTextOffer> offers)
        {
            // Prioritize offers that provide the most vis, as this is a direct gain for us.
            var sortedOffers = offers.OrderByDescending(o => o.VisValue).ToList();

            // Iterate through offers, attempting to sell.
            // Use a 'while' loop with explicit removal to handle concurrent offers and changing inventory.
            while (sortedOffers.Any())
            {
                var sellOffer = sortedOffers.First(); // Get the current best offer

                // Check if:
                // 1. We (the seller) still possess the specific lab text being offered.
                // 2. The buyer (sellOffer.TradingPartner) still has enough vis to pay.
                // 3. The buyer still needs/desires this specific lab text (e.g., they haven't acquired a better one elsewhere).
                bool weStillHaveTheText = mage.GetLabTextsFromCollection(sellOffer.LabTextDesired.SpellContained.Base).Contains(sellOffer.LabTextDesired);
                bool buyerCanAfford = sellOffer.TradingPartner.HasSufficientVis(sellOffer.VisOffers);

                // Buyer still needs logic: Check if they don't have this spell base, or if they have it but at a lower level.
                bool buyerStillNeedsText = !sellOffer.TradingPartner.GetLabTextsFromCollection(sellOffer.LabTextDesired.SpellContained.Base).Any(l => l.SpellContained.Level >= sellOffer.LabTextDesired.SpellContained.Level);

                if (weStillHaveTheText && buyerCanAfford && buyerStillNeedsText)
                {
                    // Execute the trade:
                    mage.Log.Add($"Selling Lab Text for '{sellOffer.LabTextDesired.SpellContained.Name}' to {sellOffer.TradingPartner.Name} for {sellOffer.VisValue:0.00} vis value.");
                    sellOffer.TradingPartner.Log.Add($"Buying Lab Text for '{sellOffer.LabTextDesired.SpellContained.Name}' from {mage.Name}.");

                    // Transfer ownership of the lab text:
                    sellOffer.TradingPartner.LabTextsOwned.Add(sellOffer.LabTextDesired);
                    mage.LabTextsOwned.Remove(sellOffer.LabTextDesired); // Seller removes from their own inventory.

                    // Transfer vis:
                    sellOffer.TradingPartner.UseVis(sellOffer.VisOffers); // Buyer spends vis.
                    mage.GainVis(sellOffer.VisOffers); // Seller gains vis.

                    // Remove this specific offer and any other offers that involve the now-sold lab text.
                    sortedOffers = sortedOffers.Where(o => o.LabTextDesired != sellOffer.LabTextDesired).ToList();
                }
                else
                {
                    // If the offer cannot be executed (e.g., text was already sold, buyer ran out of vis, buyer no longer needs),
                    // remove this specific offer and proceed to the next best one.
                    sortedOffers.Remove(sellOffer);
                }
            }
        }

        private static void ProcessLabTextSwaps(this Magus mage, IEnumerable<LabTextTradeOffer> offers)
        {
            var sortedOffers = offers.OrderBy(o => mage.RateLifetimeLabTextValue(o.LabTextDesired));

            foreach (var offer in sortedOffers)
            {
                // Check if we still need their text and have our text to offer
                bool needTheirText = !mage.GetLabTextsFromCollection(offer.LabTextDesired.SpellContained.Base).Any();
                bool haveOurText = mage.GetLabTextsFromCollection(offer.LabTextOffered.SpellContained.Base).Contains(offer.LabTextOffered);
                bool theyHaveTheirText = offer.Mage.GetLabTextsFromCollection(offer.LabTextDesired.SpellContained.Base).Contains(offer.LabTextDesired);

                if (needTheirText && haveOurText && theyHaveTheirText)
                {
                    mage.Log.Add($"Swapping Lab Text '{offer.LabTextOffered.SpellContained.Name}' with {offer.Mage.Name} for '{offer.LabTextDesired.SpellContained.Name}'.");
                    offer.Mage.Log.Add($"Swapping Lab Text '{offer.LabTextDesired.SpellContained.Name}' with {mage.Name} for '{offer.LabTextOffered.SpellContained.Name}'.");

                    // Execute swap
                    mage.LabTextsOwned.Add(offer.LabTextDesired);
                    mage.LabTextsOwned.Remove(offer.LabTextOffered);
                    offer.Mage.LabTextsOwned.Add(offer.LabTextOffered);
                    offer.Mage.LabTextsOwned.Remove(offer.LabTextDesired);
                }
            }
        }
    }
}
