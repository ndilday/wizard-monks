using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Decisions.Goals;
using WizardMonks.Economy;
using WizardMonks.Instances;
using WizardMonks.Models.Books;
using WizardMonks.Models.Ideas;
using WizardMonks.Models.Laboratories;
using WizardMonks.Models.Spells;
using WizardMonks.Services.Characters;

namespace WizardMonks.Models.Characters
{
    public class TwilightEventArgs : EventArgs
    {
        DateTime _duration;
        ushort _extraWarping;

        public TwilightEventArgs(DateTime duration, ushort extraWarping)
        {
            _duration = duration;
            _extraWarping = extraWarping;
        }
    }

	[Serializable]
	public partial class Magus : Character
    {
        #region Private Fields
        private MagusTradingDesires _tradeDesires;
        private HousesEnum _house;
        private List<AIdea> _ideas;
        #endregion

        #region Public Properties
        public Spell PartialSpell { get; set; }
        public double PartialSpellProgress { get; set; }
        public List<LabText> LabTextsOwned { get; private set; }
        public Ability MagicAbility { get; private set; }
        public Magus Apprentice { get; private set; }
        public Laboratory Laboratory { get; set; }
        public List<Spell> SpellList { get; private set; }
        public Dictionary<Ability, double> VisStock { get; private set; }
        public Dictionary<Character, ushort> DecipheredShorthandLevels { get; private set; }
        public Dictionary<LabText, double> ShorthandTranslationProgress { get; private set; }
        public HousesEnum House
        {
            get
            {
                return _house;
            }
            set
            {
                _house = value;
                WantsToFollow = false;
            }
        }
        public Covenant Covenant { get; private set; }
        public Arts Arts { get; private set; }
        public double VisStudyRate { get; private set; }
        #endregion

        #region Initialization Functions
        public Magus() : this(Abilities.MagicTheory, Abilities.Latin, Abilities.ArtesLiberales, Abilities.AreaLore, HousesEnum.Apprentice, 80, null, null) { }
        public Magus(HousesEnum house, uint age, Personality personality, Dictionary<string, double> reputationFocuses) : this(Abilities.MagicTheory, Abilities.Latin, Abilities.ArtesLiberales, Abilities.AreaLore, house, age, personality, reputationFocuses) { }
        public Magus(Ability magicAbility, Ability writingLanguage, Ability writingAbility, Ability areaAbility, HousesEnum house = HousesEnum.Apprentice, uint baseAge = 20, Personality personality = null, Dictionary<string, double> reputationFocuses = null)
            : base(writingLanguage, writingAbility, areaAbility, baseAge, personality, reputationFocuses)
        {
            MagicAbility = magicAbility;
            Arts = new Arts(this.InvalidateWritableTopicsCache);
            Covenant = null;
            Laboratory = null;
            VisStock = [];
            SpellList = [];
            LabTextsOwned = [];
            DecipheredShorthandLevels = [];
            ShorthandTranslationProgress = [];
            PartialSpell = null;
            PartialSpellProgress = 0;
            _ideas = [];
            VisStudyRate = 6.75;
            House = house;
            foreach (Ability art in MagicArts.GetEnumerator())
            {
                VisStock[art] = 0;
            }

            InitializeGoals();
        }

        private void InitializeGoals()
        {
            Goals.Add(new AvoidDecrepitudeGoal(this, 1.0));
        }
        #endregion

        #region Ability Functions
        public override CharacterAbilityBase GetAbility(Ability ability)
        {
            if (MagicArts.IsArt(ability))
            {
                return Arts.GetAbility(ability);
            }
            else
            {
                return base.GetAbility(ability);
            }
        }
        #endregion

        #region Covenant Functions
        public void Join(Covenant covenant)
        {
            if (Covenant != null)
            {
                Covenant.RemoveMagus(this);
            }
            Covenant = covenant;
            covenant.AddMagus(this);
            VisStudyRate = 6.75 + covenant.Aura.Strength;
        }

        public Covenant FoundCovenant(Aura aura)
        {
            Covenant coventant = new(aura);
            Join(coventant);
            return coventant;
        }
        #endregion

        #region Idea Functions
        public IEnumerable<AIdea> GetInspirations()
        {
            return _ideas;
        }

        public void AddIdea(AIdea idea)
        {
            // Prevent adding duplicate ideas, if we later decide ideas can be shared
            if (!_ideas.Any(i => i.Id == idea.Id))
            {
                _ideas.Add(idea);
                Log.Add($"Gained a new idea: {idea.Description}");

                // Add a new goal to pursue this inspiration
                Goals.Add(new PursueIdeaGoal(this, idea));
            }
        }
        #endregion

        #region Goal/Preference Functions

        /// <summary>
        /// Determines the value of an experience gain in terms of the value of vis, 
        /// and the amount of time it would take the character to produce and learn from
        /// that amount of vis
        /// </summary>
        /// <param name="ability"></param>
        /// <param name="gain"></param>
        /// <returns>the vis equivalence (vis savings) of this gain relative to vis study</returns>
        public override double RateSeasonalExperienceGain(Ability ability, double gain)
        {
            if (!MagicArts.IsArt(ability))
            {
                return base.RateSeasonalExperienceGain(ability, gain);
            }
            double baseDistillVisRate = this.GetVisDistillationRate();
            double distillVisRate = baseDistillVisRate;
            if (MagicArts.IsTechnique(ability))
            {
                distillVisRate /= 4.0;
            }
            else if (ability != MagicArts.Vim)
            {
                distillVisRate /= 2.0;
            }

            CharacterAbilityBase charAbility = GetAbility(ability);
            double visUsedPerStudySeason = 0.5 + (charAbility.Value + charAbility.GetValueGain(gain)/2) / 10.0;
            // the gain per season depends on how the character views vis
            double studySeasons = gain / VisStudyRate;
            double visNeeded = studySeasons * visUsedPerStudySeason;
            // compare to the number of seasons we would need to extract the vis
            // plus the number of seasons we would need to study the extracted vis
            double extractTime = visNeeded / distillVisRate;
            double totalVisEquivalent = (extractTime + studySeasons) * baseDistillVisRate;

            // credit back the value of the exposure gained in the process of distilling
            double exposureGained = 2.0 * extractTime;
            double exposureSeasonsOfVis = exposureGained / VisStudyRate;
            CharacterAbilityBase vim = GetAbility(MagicArts.Vim);
            CharacterAbilityBase creo = GetAbility(MagicArts.Creo);
            CharacterAbilityBase exposureAbility = creo.Value < vim.Value ? creo : vim;
            double visValueOfExposure = 0.5 + (exposureAbility.Value + exposureAbility.GetValueGain(exposureGained)/2) / 10.0 * exposureSeasonsOfVis;
            return totalVisEquivalent - visValueOfExposure;
        }

        protected IEnumerable<BookForTrade> EvaluateBookValuesAsSeller(IEnumerable<ABook> books)
        {
            List<BookForTrade> list = new();
            double distillRate = this.GetVisDistillationRate();
            foreach (ABook book in books)
            {
                if (book.Level == 1000)
                {
                    list.Add(new BookForTrade(book, distillRate));
                }
                else
                {
                    double writeRate = GetAbility(WritingLanguage).Value + _attributes[(int)AttributeType.Communication].Value;
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

        protected IEnumerable<LabTextForTrade> EvaluateLabTextValuesAsSeller(IEnumerable<LabText> labTexts)
        {
            List<LabTextForTrade> list = [];
            double distillRate = this.GetVisDistillationRate();
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
                    double writeRate = GetAbility(WritingLanguage).Value * 20;
                    list.Add(new LabTextForTrade(labText, distillRate * labText.SpellContained.Level / writeRate));
                }
            }
            return list;
        }

        public MagusTradingDesires GenerateTradingDesires()
        {
            UpdateVisDesiresWithStock();
            _tradeDesires = new MagusTradingDesires(
                this,
                Desires.VisDesires,
                Desires.BookDesires,
                EvaluateBookValuesAsSeller(this.GetUnneededBooksFromCollection()),
                Desires.LabTextDesires,
                EvaluateLabTextValuesAsSeller(this.GetUnneededLabTextsFromCollection())
            );
            if (_tradeDesires == null)
            {
                throw new NullReferenceException();
            }
            return _tradeDesires;
        }

        private void UpdateVisDesiresWithStock()
        {
            foreach (VisDesire visDesire in Desires.VisDesires)
            {
                if (VisStock.ContainsKey(visDesire.Art))
                {
                    visDesire.Quantity -= VisStock[visDesire.Art];
                }
            }
        }

        public void EvaluateTradingDesires(IEnumerable<MagusTradingDesires> mageTradeDesires)
        {
            List<VisTradeOffer> visTradeOffers = new();
            List<BookTradeOffer> bookTradeOffers = new();
            //List<VisForBookOffer> buyBookOffers = new();
            List<VisForBookOffer> sellBookOffers = new();
            List<VisForLabTextOffer> sellLabTextOffers = new();
            List<LabTextTradeOffer> labTextTradeOffers = new();

            foreach (MagusTradingDesires tradeDesires in mageTradeDesires)
            {
                if (tradeDesires.Mage == this)
                {
                    continue;
                }

                var bookTrades = _tradeDesires.GenerateBookTradeOffers(tradeDesires);
                bookTradeOffers.AddRange(bookTrades);

                /*var bookBuyOffers = _tradeDesires.GenerateBuyBookOffers(tradeDesires);
                if (bookBuyOffers != null)
                {
                    buyBookOffers.AddRange(bookBuyOffers);
                }*/

                var bookSellOffers = _tradeDesires.GenerateSellBookOffers(tradeDesires);
                if (bookSellOffers != null)
                {
                    sellBookOffers.AddRange(bookSellOffers);
                }

                var visOffersGenerated = _tradeDesires.GenerateVisOffers(tradeDesires);
                if (visOffersGenerated != null)
                {
                    visTradeOffers.AddRange(visOffersGenerated);
                }

                var labTextSales = _tradeDesires.GenerateSellLabTextOffers(tradeDesires);
                if (labTextSales != null) sellLabTextOffers.AddRange(labTextSales);

                var labTextSwaps = _tradeDesires.GenerateLabTextTradeOffers(tradeDesires);
                if (labTextSwaps != null) labTextTradeOffers.AddRange(labTextSwaps);
            }

            ProcessVisOffers(visTradeOffers);
            ProcessBookSwaps(bookTradeOffers);
            ProcessBookSales(sellBookOffers);
            //ProcessBookPurchases(buyBookOffers);
            ProcessLabTextSwaps(labTextTradeOffers);
            ProcessLabTextSales(sellLabTextOffers);
            // figure out book for book
        }

        private void ProcessVisOffers(List<VisTradeOffer> offers)
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
                var prioritizedVisDesires = _tradeDesires.VisDesires.Where(v => v.Quantity > 0).OrderByDescending(v => v.Quantity);
                var prioritizedVisStocks = _tradeDesires.VisDesires.Where(v => v.Quantity < 0).OrderBy(v => v.Quantity);
                var bestOffers = from offer in internalOffers
                                 join visDesire in _tradeDesires.VisDesires on offer.Ask.Art equals visDesire.Art
                                 join visStock in _tradeDesires.VisDesires on offer.Bid.Art equals visStock.Art
                                 orderby visDesire.Quantity descending, visStock.Quantity, offer.Ask.Quantity
                                 select offer;
                if (bestOffers.Any())
                {
                    var offer = bestOffers.First();
                    VisDesire mostDesired = _tradeDesires.VisDesires[offer.Ask.Art.AbilityId % 300];
                    VisDesire mostOverstocked = _tradeDesires.VisDesires[offer.Bid.Art.AbilityId % 300];
                    if (this.GetVisCount(offer.Bid.Art) >= offer.Bid.Quantity && offer.Mage.GetVisCount(offer.Ask.Art) >= offer.Ask.Quantity)
                    {
                        Log.Add("Executing vis trade with " + offer.Mage.Name);
                        Log.Add("Trading " + offer.Bid.Quantity.ToString("0.000") + " pawns of " + offer.Bid.Art.AbilityName + " vis");
                        Log.Add("for " + offer.Ask.Quantity.ToString("0.000") + " pawns of " + offer.Ask.Art.AbilityName + " vis");
                        offer.Mage.Log.Add("Executing vis trade with " + Name);
                        offer.Mage.Log.Add("Trading " + offer.Ask.Quantity.ToString("0.000") + " pawns of " + offer.Ask.Art.AbilityName + " vis");
                        offer.Mage.Log.Add("for " + offer.Bid.Quantity.ToString("0.000") + " pawns of " + offer.Bid.Art.AbilityName + " vis");
                        
                        offer.Execute();
                        internalOffers = internalOffers.Where(o => o != offer);
                        this.GainVis(offer.Ask.Art, offer.Ask.Quantity);
                        mostDesired.Quantity -= offer.Ask.Quantity;
                        internalOffers = internalOffers.Where(o => o.Ask.Art != mostDesired.Art || o.Ask.Quantity <= mostDesired.Quantity);
                        this.UseVis(offer.Bid.Art, offer.Bid.Quantity);
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

        private void ProcessBookSales(IEnumerable<VisForBookOffer> bookSales)
        {
            var sales = bookSales.OrderByDescending(bts => bts.VisValue);
            while (sales.Any())
            {
                var sellOffer = sales.First();
                // Check if seller still owns the book and buyer can afford it.
                if (this.GetBooksInCollection(sellOffer.BookDesired.Topic).Contains(sellOffer.BookDesired) &&
                    sellOffer.TradingPartner.HasSufficientVis(sellOffer.VisOffers))
                {
                    // Enact trade
                    Log.Add("Selling " + sellOffer.BookDesired.Title + " to " + sellOffer.TradingPartner.Name);
                    Log.Add("for " + sellOffer.VisValue.ToString("0.000") + " worth of vis");
                    sellOffer.TradingPartner.Log.Add("Buying " + sellOffer.BookDesired.Title + " from " + Name);
                    sellOffer.TradingPartner.Log.Add("for " + sellOffer.VisValue.ToString("0.000") + " worth of vis");

                    // State changes are correctly handled by the participants.
                    sellOffer.TradingPartner.AddBookToCollection(sellOffer.BookDesired);
                    RemoveBookFromCollection(sellOffer.BookDesired); // Seller removes from own inventory.
                    sellOffer.TradingPartner.UseVis(sellOffer.VisOffers);
                    this.GainVis(sellOffer.VisOffers);
                }
                // This logic had a bug. If the 'if' fails, the loop becomes infinite.
                // It must be moved outside the 'if' block.
                sales = sales.Where(s => s.BookDesired != sellOffer.BookDesired).OrderBy(bto => bto.VisValue);
            }
        }

        private void ProcessBookSwaps(IEnumerable<BookTradeOffer> bookTradeOffers)
        {
            var trades = bookTradeOffers.OrderBy(bto => bto.BookDesired.Quality);
            while (trades.Any())
            {
                var tradeOffer = trades.First();
                if (this.GetBooksInCollection(tradeOffer.BookOffered.Topic).Contains(tradeOffer.BookOffered) &&
                   tradeOffer.Mage.GetBooksInCollection(tradeOffer.BookDesired.Topic).Contains(tradeOffer.BookDesired))
                {
                    Log.Add("Trading " + tradeOffer.BookOffered.Title + " to " + tradeOffer.Mage.Name);
                    Log.Add("For " + tradeOffer.BookDesired.Title);
                    tradeOffer.Mage.Log.Add("Trading " + tradeOffer.BookDesired.Title + " to " + Name);
                    tradeOffer.Mage.Log.Add("For " + tradeOffer.BookOffered.Title);

                    AddBookToCollection(tradeOffer.BookDesired);
                    tradeOffer.Mage.RemoveBookFromCollection(tradeOffer.BookDesired);
                    RemoveBookFromCollection(tradeOffer.BookOffered);
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
        private void ProcessLabTextSales(IEnumerable<VisForLabTextOffer> offers)
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
                bool weStillHaveTheText = this.GetLabTextsFromCollection(sellOffer.LabTextDesired.SpellContained.Base).Contains(sellOffer.LabTextDesired);
                bool buyerCanAfford = sellOffer.TradingPartner.HasSufficientVis(sellOffer.VisOffers);

                // Buyer still needs logic: Check if they don't have this spell base, or if they have it but at a lower level.
                bool buyerStillNeedsText = !sellOffer.TradingPartner.GetLabTextsFromCollection(sellOffer.LabTextDesired.SpellContained.Base).Any(l => l.SpellContained.Level >= sellOffer.LabTextDesired.SpellContained.Level);

                if (weStillHaveTheText && buyerCanAfford && buyerStillNeedsText)
                {
                    // Execute the trade:
                    Log.Add($"Selling Lab Text for '{sellOffer.LabTextDesired.SpellContained.Name}' to {sellOffer.TradingPartner.Name} for {sellOffer.VisValue:0.00} vis value.");
                    sellOffer.TradingPartner.Log.Add($"Buying Lab Text for '{sellOffer.LabTextDesired.SpellContained.Name}' from {Name}.");

                    // Transfer ownership of the lab text:
                    sellOffer.TradingPartner.LabTextsOwned.Add(sellOffer.LabTextDesired);
                    LabTextsOwned.Remove(sellOffer.LabTextDesired); // Seller removes from their own inventory.

                    // Transfer vis:
                    sellOffer.TradingPartner.UseVis(sellOffer.VisOffers); // Buyer spends vis.
                    this.GainVis(sellOffer.VisOffers); // Seller gains vis.

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

        private void ProcessLabTextSwaps(IEnumerable<LabTextTradeOffer> offers)
        {
            var sortedOffers = offers.OrderBy(o => this.RateLifetimeLabTextValue(o.LabTextDesired));

            foreach (var offer in sortedOffers)
            {
                // Check if we still need their text and have our text to offer
                bool needTheirText = !this.GetLabTextsFromCollection(offer.LabTextDesired.SpellContained.Base).Any();
                bool haveOurText = this.GetLabTextsFromCollection(offer.LabTextOffered.SpellContained.Base).Contains(offer.LabTextOffered);
                bool theyHaveTheirText = offer.Mage.GetLabTextsFromCollection(offer.LabTextDesired.SpellContained.Base).Contains(offer.LabTextDesired);

                if (needTheirText && haveOurText && theyHaveTheirText)
                {
                    Log.Add($"Swapping Lab Text '{offer.LabTextOffered.SpellContained.Name}' with {offer.Mage.Name} for '{offer.LabTextDesired.SpellContained.Name}'.");
                    offer.Mage.Log.Add($"Swapping Lab Text '{offer.LabTextDesired.SpellContained.Name}' with {Name} for '{offer.LabTextOffered.SpellContained.Name}'.");

                    // Execute swap
                    LabTextsOwned.Add(offer.LabTextDesired);
                    LabTextsOwned.Remove(offer.LabTextOffered);
                    offer.Mage.LabTextsOwned.Add(offer.LabTextOffered);
                    offer.Mage.LabTextsOwned.Remove(offer.LabTextDesired);
                }
            }
        }
        #endregion

        #region Apprentice Functions
        public void TakeApprentice(Magus apprentice)
        {
            // TODO: what sort of error checking should go here?
            Apprentice = apprentice;
            // add a teaching goal for each year
            for (byte i = 2; i < 16; i++)
            {
                uint dueDate = (uint)(i * 4);
                IGoal teachingGoal = new TeachApprenticeGoal(this, SeasonalAge + i - 1, 1);
                Goals.Add(teachingGoal);
            }
            IGoal gauntletGoal = new GauntletApprenticeGoal(this, SeasonalAge + 60, 1);
            Goals.Add(gauntletGoal);
        }

        public void GauntletApprentice()
        {
            Apprentice.House = House;
            Apprentice = null;
        }
        #endregion

        #region object Overrides
        public override string ToString()
        {
            return Name + " ex " + House.ToString();
        }
        #endregion
    }
}
