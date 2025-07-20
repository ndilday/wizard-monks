using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Activities;
using WizardMonks.Decisions;
using WizardMonks.Decisions.Goals;
using WizardMonks.Instances;

namespace WizardMonks
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
        private Ability _magicAbility;
        private Spell _partialSpell;
        private double _partialSpellProgress;
        private Dictionary<Ability, double> _visStock;
        private List<LabText> _labTextsOwned;
        private MagusTradingDesires _tradeDesires;
        //private List<SummaGoal> _summaGoals;
        //private List<TractatusGoal> _tractatusGoals;
        private Houses _house;
        #endregion

        #region Public Properties
        public Magus Apprentice { get; private set; }
        public Laboratory Laboratory { get; private set; }
        public List<Spell> SpellList { get; private set; }
        public Houses House
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
        public Magus() : this(Abilities.MagicTheory, Abilities.Latin, Abilities.ArtesLiberales, Abilities.AreaLore, 80) { }
        public Magus(uint age) : this(Abilities.MagicTheory, Abilities.Latin, Abilities.ArtesLiberales, Abilities.AreaLore, age) { }
        public Magus(Ability magicAbility, Ability writingLanguage, Ability writingAbility, Ability areaAbility, uint baseAge = 20)
            : base(writingLanguage, writingAbility, areaAbility, baseAge)
        {
            _magicAbility = magicAbility;
            Arts = new Arts();
            Covenant = null;
            Laboratory = null;
            _visStock = new Dictionary<Ability, double>();
            SpellList = new List<Spell>();
            _labTextsOwned = new List<LabText>();
            //_tractatusGoals = new List<TractatusGoal>();
            //_summaGoals = new List<SummaGoal>();
            _partialSpell = null;
            _partialSpellProgress = 0;
            VisStudyRate = 6.75;
            House = Houses.Apprentice;
            foreach (Ability art in MagicArts.GetEnumerator())
            {
                _visStock[art] = 0;
            }

            InitializeGoals();
        }

        private void InitializeGoals()
        {
            _goals.Add(new LongevityRitualGoal(this, 140, 1));
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

        #region Book Functions
        public override IEnumerable<ABook> GetBooksFromCollection(Ability ability)
        {
            IEnumerable<ABook> books = _booksOwned.Where(b => b.Topic == ability);
            if (Covenant != null)
            {
                books = books.Union(Covenant.GetLibrary(ability));
            }
            return books;
        }

        private List<BookDesire> GetBookDesires()
        {
            if (GetAbility(_writingAbility).Value >= 1.0 && GetAbility(_writingLanguage).Value >= 4.0)
            {
                List<BookDesire> bookDesires = new();
                IList<BookDesire> bookNeeds;
                foreach (IGoal goal in _goals)
                {
                    bookNeeds = goal.GetBookDesires();
                    if (bookNeeds != null)
                    {
                        bookDesires.AddRange(bookNeeds);
                    }
                }
                return bookDesires;
            }
            return new List<BookDesire>();
        }

        public override ABook GetBestBookToWrite()
        {
            double currentBestBookValue = 0;
            ABook bestBook = null;
            HashSet<int> consideredTopics = new();

            // since the value of a tractatus is independent of topic,
            // calculate the value of writing a tractatus now, so that we don't have to keep doing it
            double tractatusValue = (6 + GetAttributeValue(AttributeType.Communication)) * GlobalEconomy.GlobalTractatusValue / 6;
            double writingRate = GetAttributeValue(AttributeType.Communication) + GetAbility(_writingLanguage).Value;
            var unneededBookTopics = GetUnneededBooksFromCollection().Select(b => b.Topic).Distinct();
            foreach (BookDesire bookDesire in GlobalEconomy.DesiredBooksList)
            {
                // if we already have a suitable book for this topic, let's not try to write another
                if (unneededBookTopics.Contains(bookDesire.Ability) || bookDesire.Character == this) continue;
                // check to see if we could even write a summa of a level that would meet this desire
                // TODO: make sure our book level is sufficiently higher than the current level that our quality will be worthwhile
                CharacterAbilityBase ability = GetAbility(bookDesire.Ability);
                if (ability.Value > bookDesire.CurrentLevel * 2 && ability.Experience >= 15)
                {
                    CharacterAbilityBase buyerAbility;
                    if (bookDesire.Ability.AbilityType != AbilityType.Art)
                    {
                        buyerAbility = new CharacterAbility(bookDesire.Ability);
                    }
                    else
                    {
                        buyerAbility = new AcceleratedAbility(bookDesire.Ability);
                    }
                    buyerAbility.Experience = buyerAbility.GetExperienceUntilLevel(bookDesire.CurrentLevel);

                    // see if we have started a summa on this topic
                    var relatedIncompleteBooks = _incompleteBooks.Where(b => b.Topic == bookDesire.Ability && b.Level > bookDesire.CurrentLevel);
                    if (relatedIncompleteBooks.Any())
                    {
                        foreach (Summa incompleteBook in relatedIncompleteBooks)
                        {
                            // the effective value is based on time to finish, not time already invested
                            double experienceValue = buyerAbility.GetExperienceUntilLevel(incompleteBook.Level);
                            double seasonsOfStudy = Math.Ceiling(experienceValue / incompleteBook.Quality);
                            double effectiveQuality = experienceValue / seasonsOfStudy;
                            // at a minimum, the book is worth the vis it would take, on average, to gain that experience
                            double visUsedPerStudySeason = 0.5 + ((buyerAbility.Value + (buyerAbility.GetValueGain(experienceValue) / 2)) / 10.0);
                            double studySeasons = experienceValue / VisStudyRate;
                            double visNeeded = studySeasons * visUsedPerStudySeason;
                            // scale visNeeded according to vis type
                            if(MagicArts.IsTechnique(bookDesire.Ability))
                            {
                                visNeeded *= 4;
                            }
                            else if(MagicArts.IsArt(bookDesire.Ability) && bookDesire.Ability != MagicArts.Vim)
                            {
                                visNeeded *= 2;
                            }

                            // for now, scale vis according to quality of book vs. quality of vis study
                            visNeeded *= incompleteBook.Quality / VisStudyRate;

                            // divide this visNeed valuation by how many seasons are left for writing
                            double writingNeeded = incompleteBook.GetWritingPointsNeeded() - incompleteBook.PointsComplete;
                            double seasonsLeft = Math.Ceiling(writingNeeded / writingRate);
                            double writingValue = visNeeded / seasonsLeft;
                            if(writingValue > currentBestBookValue)
                            {
                                // continue writing this summa
                                bestBook = incompleteBook;
                                currentBestBookValue = writingValue;
                            }
                        }
                    }
                    else
                    {
                        // NOTE: this could lead us down a strange rabbit hole of starting a bunch of 
                        // summae on a subject of varying levels, but I think that's unlikely enough
                        // to not try and protect from for now
                        double maxLevel = GetAbility(bookDesire.Ability).Value / 2.0;

                        for (double l = maxLevel; l > bookDesire.CurrentLevel; l--)
                        {
                            double q = 6 + GetAttributeValue(AttributeType.Communication) + maxLevel - l;
                            // the effective value is based on time to finish, not time already invested
                            double experienceValue = buyerAbility.GetExperienceUntilLevel(l);
                            double seasonsOfStudy = Math.Ceiling(experienceValue / q);
                            double effectiveQuality = experienceValue / seasonsOfStudy;
                            // at a minimum, the book is worth the vis it would take, on average, to gain that experience
                            double visUsedPerStudySeason = 0.5 + ((buyerAbility.Value + (buyerAbility.GetValueGain(experienceValue) / 2)) / 10.0);
                            double studySeasons = experienceValue / VisStudyRate;
                            double visNeeded = studySeasons * visUsedPerStudySeason;
                            // scale visNeeded according to vis type
                            if (MagicArts.IsTechnique(bookDesire.Ability))
                            {
                                visNeeded *= 4;
                            }
                            else if (MagicArts.IsArt(bookDesire.Ability) && bookDesire.Ability != MagicArts.Vim)
                            {
                                visNeeded *= 2;
                            }

                            // for now, scale vis according to quality of book vs. quality of vis study
                            visNeeded *= q / VisStudyRate;

                            // divide this visNeed valuation by how many seasons are left for writing
                            double seasonsLeft = Math.Ceiling(l / writingRate);
                            double writingValue = visNeeded / seasonsLeft;
                            if (writingValue > currentBestBookValue)
                            {
                                // write this summa
                                bestBook = new Summa
                                {
                                    Quality = q,
                                    Level = l,
                                    Topic = bookDesire.Ability,
                                    Title = bookDesire.Ability.AbilityName + l.ToString("0.0") + " Summa " + SeasonalAge + " by " + Name,
                                    Value = writingValue
                                };
                                currentBestBookValue = writingValue;
                            }
                        }
                    }
                }

                // consider the value of writing a tractatus
                var charAbility = GetAbility(bookDesire.Ability);
                if (!consideredTopics.Contains(bookDesire.Ability.AbilityId) && tractatusValue > currentBestBookValue && CanWriteTractatus(charAbility))
                {
                    ushort previouslyWrittenCount = GetTractatiiWrittenOnTopic(bookDesire.Ability);
                    string name = Name + " " + bookDesire.Ability.AbilityName + " T" + previouslyWrittenCount.ToString();
                    bestBook = new Tractatus
                    {
                        Topic = bookDesire.Ability,
                        Title = name,
                        Value = tractatusValue
                    };
                    currentBestBookValue = tractatusValue;
                }
                consideredTopics.Add(bookDesire.Ability.AbilityId);
            }

            return bestBook;
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
            double baseDistillVisRate = GetVisDistillationRate();
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
            double visUsedPerStudySeason = 0.5 + ((charAbility.Value + (charAbility.GetValueGain(gain)/2)) / 10.0);
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
            double visValueOfExposure = 0.5 + ((exposureAbility.Value + (exposureAbility.GetValueGain(exposureGained)/2)) / 10.0) * exposureSeasonsOfVis;
            return totalVisEquivalent - visValueOfExposure;
        }

        protected IEnumerable<BookForTrade> EvaluateBookValuesAsSeller(IEnumerable<ABook> books)
        {
            List<BookForTrade> list = new();
            double distillRate = GetVisDistillationRate();
            foreach (ABook book in books)
            {
                if (book.Level == 1000)
                {
                    list.Add(new BookForTrade(book, distillRate));
                }
                else
                {
                    double writeRate = GetAbility(_writingLanguage).Value + _attributes[(int)AttributeType.Communication].Value;
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
        
        public override void ReprioritizeGoals()
        {
        }

        IActivity DecideSeasonalActivity(IList<MagusTradingDesires> tradeDesiresList)
        {
            if (IsCollaborating)
            {
                return _mandatoryAction;
            }
            else
            {
                ConsideredActions actions = new();
                _verboseLog.Add("----------");
                foreach (IGoal goal in _goals)
                {
                    if (!goal.IsComplete())
                    {
                        // TODO: it should probably be an error case for a goal to still be here
                        // for now, ignore
                        //List<string> dummy = new List<string>();
                        goal.AddActionPreferencesToList(actions, _verboseLog);
                    }
                }
                Log.AddRange(actions.Log());
                return actions.GetBestAction();
            }
        }

        public MagusTradingDesires GenerateTradingDesires()
        {
            _tradeDesires = new MagusTradingDesires(
                this,
                GetVisDesires(),
                GetBookDesires().Distinct(),
                EvaluateBookValuesAsSeller(GetUnneededBooksFromCollection())
            );
            if (_tradeDesires == null)
            {
                throw new NullReferenceException();
            }
            return _tradeDesires;
        }

        public void EvaluateTradingDesires(IEnumerable<MagusTradingDesires> mageTradeDesires)
        {
            List<VisTradeOffer> visTradeOffers = new();
            List<BookTradeOffer> bookTradeOffers = new();
            List<VisForBookOffer> buyBookOffers = new();
            List<VisForBookOffer> sellBookOffers = new();
            foreach (MagusTradingDesires tradeDesires in mageTradeDesires)
            {
                if (tradeDesires.Mage == this)
                {
                    continue;
                }

                var bookTrades = _tradeDesires.GenerateBookTradeOffers(tradeDesires);
                bookTradeOffers.AddRange(bookTrades);

                var bookBuyOffers = _tradeDesires.GenerateBuyBookOffers(tradeDesires);
                if (bookBuyOffers != null)
                {
                    buyBookOffers.AddRange(bookBuyOffers);
                }

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
            }

            ProcessVisOffers(visTradeOffers);
            ProcessBookSwaps(bookTradeOffers);
            ProcessBookSales(sellBookOffers);
            ProcessBookPurchases(buyBookOffers);
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
                    if (GetVisCount(offer.Bid.Art) >= offer.Bid.Quantity && offer.Mage.GetVisCount(offer.Ask.Art) >= offer.Ask.Quantity)
                    {
                        Log.Add("Executing vis trade with " + offer.Mage.Name);
                        Log.Add("Trading " + offer.Bid.Quantity.ToString("0.000") + " pawns of " + offer.Bid.Art.AbilityName + " vis");
                        Log.Add("for " + offer.Ask.Quantity.ToString("0.000") + " pawns of " + offer.Ask.Art.AbilityName + " vis");
                        offer.Mage.Log.Add("Executing vis trade with " + Name);
                        offer.Mage.Log.Add("Trading " + offer.Ask.Quantity.ToString("0.000") + " pawns of " + offer.Ask.Art.AbilityName + " vis");
                        offer.Mage.Log.Add("for " + offer.Bid.Quantity.ToString("0.000") + " pawns of " + offer.Bid.Art.AbilityName + " vis");
                        
                        offer.Execute();
                        internalOffers = internalOffers.Where(o => o != offer);
                        GainVis(offer.Ask.Art, offer.Ask.Quantity);
                        mostDesired.Quantity -= offer.Ask.Quantity;
                        internalOffers = internalOffers.Where(o => o.Ask.Art != mostDesired.Art || o.Ask.Quantity <= mostDesired.Quantity);
                        UseVis(offer.Bid.Art, offer.Bid.Quantity);
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

        private void ProcessBookPurchases(IEnumerable<VisForBookOffer> bookBuys)
        {
            var buys = bookBuys.OrderBy(bto => bto.BookDesired.Quality).ThenBy(bto => bto.VisValue);
            while (buys.Any())
            {
                var buyOffer = buys.First();
                if (HasSufficientVis(buyOffer.VisOffers))
                {
                    Log.Add("Buying " + buyOffer.BookDesired.Title + " from " + buyOffer.TradingPartner.Name);
                    Log.Add("for " + buyOffer.VisValue.ToString("0.000") + " worth of vis");
                    buyOffer.TradingPartner.Log.Add("Selling " + buyOffer.BookDesired.Title + " to " + Name);
                    buyOffer.TradingPartner.Log.Add("for " + buyOffer.VisValue.ToString("0.000") + " worth of vis");

                    buyOffer.TradingPartner.RemoveBookFromCollection(buyOffer.BookDesired);
                    AddBookToCollection(buyOffer.BookDesired);
                    buyOffer.TradingPartner.GainVis(buyOffer.VisOffers);
                    UseVis(buyOffer.VisOffers);
                }
                buys = buys.Where(b => b.BookDesired != buyOffer.BookDesired).OrderBy(bto => bto.VisValue);
            }
        }

        private void ProcessBookSales(IEnumerable<VisForBookOffer> bookSales)
        {
            var sales = bookSales.OrderByDescending(bts => bts.VisValue);
            while (sales.Any())
            {
                var sellOffer = sales.First();
                if (GetBooksFromCollection(sellOffer.BookDesired.Topic).Contains(sellOffer.BookDesired) &&
                    sellOffer.TradingPartner.HasSufficientVis(sellOffer.VisOffers))
                {
                    // enact trade
                    Log.Add("Selling " + sellOffer.BookDesired.Title + " to " + sellOffer.TradingPartner.Name);
                    Log.Add("for " + sellOffer.VisValue.ToString("0.000") + " worth of vis");
                    sellOffer.TradingPartner.Log.Add("Buying " + sellOffer.BookDesired.Title + " from " + Name);
                    sellOffer.TradingPartner.Log.Add("for " + sellOffer.VisValue.ToString("0.000") + " worth of vis");

                    sellOffer.TradingPartner.AddBookToCollection(sellOffer.BookDesired);
                    RemoveBookFromCollection(sellOffer.BookDesired);
                    sellOffer.TradingPartner.UseVis(sellOffer.VisOffers);
                    GainVis(sellOffer.VisOffers);
                }
                sales = sales.Where(s => s.BookDesired != sellOffer.BookDesired).OrderBy(bto => bto.VisValue);
            }
        }

        private void ProcessBookSwaps(IEnumerable<BookTradeOffer> bookTradeOffers)
        {
            var trades = bookTradeOffers.OrderBy(bto => bto.BookDesired.Quality);
            while (trades.Any())
            {
                var tradeOffer = trades.First();
                if (GetBooksFromCollection(tradeOffer.BookOffered.Topic).Contains(tradeOffer.BookOffered) &&
                   tradeOffer.Mage.GetBooksFromCollection(tradeOffer.BookDesired.Topic).Contains(tradeOffer.BookDesired))
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
        #endregion

        #region Magic Functions
        public double GetCastingTotal(ArtPair artPair)
        {
            double techValue = Arts.GetAbility(artPair.Technique).Value;
            double formValue = Arts.GetAbility(artPair.Form).Value;
            return techValue + formValue + GetAttribute(AttributeType.Stamina).Value;
        }

        public Spell GetBestSpell(SpellBase spellBase)
        {
            return SpellList.Where(s => s.Base == spellBase).OrderByDescending(s => s.Level).FirstOrDefault();
        }

        public IEnumerable<LabText> GetLabTexts(SpellBase spellBase)
        {
            return _labTextsOwned.Where(t => t.SpellContained.Base == spellBase);
        }

        public double GetSpontaneousCastingTotal(ArtPair artPair)
        {
            // TODO: make the Diedne hack better
            double divisor = Name == "Diedne" ? 2.0 : 5.0;
            return GetCastingTotal(artPair) / divisor;
        }

        public double GetVisDistillationRate()
        {
            // TODO: One day, we'll make this more complicated
            return GetLabTotal(MagicArtPairs.CrVi, Activity.DistillVis) / 10;
        }

        public double GetAverageAuraFound()
        {
            double auraCount = KnownAuras.Count;
            double areaLore = GetAbility(Abilities.AreaLore).Value;
            areaLore += GetCastingTotal(MagicArtPairs.InVi) / 10;
            areaLore += GetAttribute(AttributeType.Perception).Value;

            double minRoll = (auraCount + 1) / areaLore;
            double multiplier = Math.Sqrt(areaLore / (auraCount + 1)) * 2 / 3;
            double areaUnder = (11.180339887498948482045868343656 - Math.Pow(minRoll, 1.5)) * multiplier;
            return areaUnder / 5;
        }

        protected void CheckTwilight()
        {
        }

        public double GetVisCount(Ability visArt)
        {
            double total = 0;
            if (Covenant != null)
            {
                total += Covenant.GetVis(visArt);
            }
            if(_visStock.ContainsKey(visArt))
            {
                total += _visStock[visArt];
            }
            return total;
        }

        private VisDesire[] GetVisDesires()
        {
            // start by making all of the character's vis stockpiles available
            VisDesire[] desires = new VisDesire[MagicArts.Count];
            desires[0] = new VisDesire(MagicArts.Creo, -this.GetVisCount(MagicArts.Creo));
            desires[1] = new VisDesire(MagicArts.Intellego, -this.GetVisCount(MagicArts.Intellego));
            desires[2] = new VisDesire(MagicArts.Muto, -this.GetVisCount(MagicArts.Muto));
            desires[3] = new VisDesire(MagicArts.Perdo, -this.GetVisCount(MagicArts.Perdo));
            desires[4] = new VisDesire(MagicArts.Rego, -this.GetVisCount(MagicArts.Rego));
            desires[5] = new VisDesire(MagicArts.Animal, -this.GetVisCount(MagicArts.Animal));
            desires[6] = new VisDesire(MagicArts.Aquam, -this.GetVisCount(MagicArts.Aquam));
            desires[7] = new VisDesire(MagicArts.Auram, -this.GetVisCount(MagicArts.Auram));
            desires[8] = new VisDesire(MagicArts.Corpus, -this.GetVisCount(MagicArts.Corpus));
            desires[9] = new VisDesire(MagicArts.Herbam, -this.GetVisCount(MagicArts.Herbam));
            desires[10] = new VisDesire(MagicArts.Ignem, -this.GetVisCount(MagicArts.Ignem));
            desires[11] = new VisDesire(MagicArts.Imaginem, -this.GetVisCount(MagicArts.Imaginem));
            desires[12] = new VisDesire(MagicArts.Mentem, -this.GetVisCount(MagicArts.Mentem));
            desires[13] = new VisDesire(MagicArts.Terram, -this.GetVisCount(MagicArts.Terram));
            desires[14] = new VisDesire(MagicArts.Vim, -this.GetVisCount(MagicArts.Vim));
            foreach (IGoal goal in _goals)
            {
                goal.ModifyVisDesires(this, desires);
            }
            foreach (VisDesire desire in desires)
            {
                desire.Quantity = Math.Truncate(desire.Quantity * 2.0) / 2.0;
            }

            return desires;
        }

        public double UseVis(Ability visType, double amount)
        {
            if (!MagicArts.IsArt(visType))
            {
                throw new ArgumentException("Only magic arts have vis!");
            }
            if (_visStock[visType] + (Covenant == null ? 0 : Covenant.GetVis(visType)) < amount)
            {
                throw new ArgumentException("Insufficient vis available!");
            }
            double covVis = Covenant == null ? 0 : Covenant.GetVis(visType);
            if (covVis >= amount)
            {
                Covenant.RemoveVis(visType, amount);
            }
            else
            {
                if (Covenant != null)
                {
                    amount -= covVis;
                    Covenant.RemoveVis(visType, covVis);
                }
                _visStock[visType] -= amount;
            }
            return _visStock[visType];
        }

        public void UseVis(List<VisOffer> visOffers)
        {
            foreach(VisOffer offer in visOffers)
            {
                UseVis(offer.Art, offer.Quantity);
            }
        }

        public double GainVis(Ability visType, double amount)
        {
            if (!MagicArts.IsArt(visType))
            {
                throw new ArgumentException("Only magic arts have vis!");
            }
            if (_visStock.ContainsKey(visType))
            {
                _visStock[visType] += amount;
            }
            else
            {
                _visStock[visType] = amount;
            }
            return _visStock[visType];
        }

        public void GainVis(List<VisOffer> visOffers)
        {
            foreach(VisOffer offer in visOffers)
            {
                GainVis(offer.Art, offer.Quantity);
            }
        }
        
        private bool HasSufficientVis(List<VisOffer> visOffers)
        {
            foreach(VisOffer offer in visOffers)
            {
                if(GetVisCount(offer.Art) < offer.Quantity)
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region Lab Activities
        public double GetLabTotal(ArtPair artPair, Activity activity)
        {
            double magicTheory = GetAbility(_magicAbility).Value;
            double techValue = Arts.GetAbility(artPair.Technique).Value;
            double formValue = Arts.GetAbility(artPair.Form).Value;
            double labTotal = magicTheory + techValue + formValue + GetAttribute(AttributeType.Intelligence).Value;
            if (Covenant != null)
            {
                labTotal += Covenant.Aura.Strength;

                if (Laboratory != null)
                {
                    labTotal += Laboratory.GetModifier(artPair, activity);
                }
                if(Apprentice != null)
                {
                    labTotal += Apprentice.GetAbility(_magicAbility).Value + Apprentice.GetAttributeValue(AttributeType.Intelligence);
                }
            }

            //TODO: foci
            //TODO: lab assistant
            //TODO: familiar
            return labTotal;
        }

        public double GetSpellLabTotal(Spell spell)
        {
            double total = GetLabTotal(spell.Base.ArtPair, Activity.InventSpells);
            // see if the mage knows a sell with the same base effect
            Spell similarSpell = GetBestSpell(spell.Base);
            if(similarSpell != null)
            {
                // if so, add the level of that spell to the lab total
                total += similarSpell.Level / 5.0;
            }
            return total;
        }

        public void BuildLaboratory()
        {
            // TODO: flesh out laboratory specialization
            Laboratory = new Laboratory(this, Covenant.Aura, 0);
        }

        public void BuildLaboratory(Aura aura)
        {
            Laboratory = new Laboratory(this, aura, 0);
        }

        public void RefineLaboratory()
        {
            if (Laboratory == null)
            {
                throw new NullReferenceException("The mage has no laboratory!");
            }
            if (GetAbility(_magicAbility).Value - 4 < Laboratory.Refinement)
            {
                throw new ArgumentOutOfRangeException("The mage's magical understanding is not high enough to refine this laboratory any further.");
            }
            Laboratory.Refine();
        }

        public void AddFeatureToLaboratory(Feature feature)
        {
            if (Laboratory == null)
            {
                throw new NullReferenceException("The mage has no laboratory!");
            }
            // TODO: Implement
        }

        public void ExtractVis(Ability exposureAbility)
        {
            // add vis to personal inventory or covenant inventory
            _visStock[MagicArts.Vim] += GetVisDistillationRate();

            // grant exposure experience
            GetAbility(exposureAbility).AddExperience(2);
        }

        public void InventSpell(Spell spell)
        {
            // TODO: multiple spells in a season
            // TODO: foci
            double labTotal = GetSpellLabTotal(spell);
            if (labTotal <= spell.Level)
            {
                throw new ArgumentException("This mage cannot invent this spell!");
            }

            if (spell == _partialSpell)
            {
                // continue previous spell work
                _partialSpellProgress += labTotal - spell.Level;
                if (_partialSpellProgress >= _partialSpell.Level)
                {
                    LearnSpell(_partialSpell);
                }
            }
            
            else if (labTotal >= spell.Level * 2)
            {
                LearnSpell(spell);
            }
            else
            {
                _partialSpell = spell;
                _partialSpellProgress = labTotal - spell.Level;
            }
        }

        public void LearnSpellFromLabText(LabText text)
        {
            // TODO: multiple spells in a season
            // TODO: foci
            Spell spell = text.SpellContained;
            double labTotal = GetSpellLabTotal(spell);
            if (labTotal < spell.Level)
            {
                throw new ArgumentException("This mage cannot invent this spell!");
            }
            else
            {
                LearnSpell(spell);
            }
        }

        private void LearnSpell(Spell spell)
        {
            SpellList.Add(spell);
            _partialSpell = null;
            _partialSpellProgress = 0;
            LabText newLabText = new LabText
            {
                Author = this,
                IsShorthand = true,
                SpellContained = spell
            };
            _labTextsOwned.Add(newLabText);

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
                IGoal teachingGoal = new TeachApprenticeGoal(this, this.SeasonalAge + i - 1, 1);
                _goals.Add(teachingGoal);
            }
            IGoal gauntletGoal = new GauntletApprenticeGoal(this, this.SeasonalAge + 60, 1);
            _goals.Add(gauntletGoal);
        }

        public void GauntletApprentice()
        {
            Apprentice.House = House;
            Apprentice = null;
        }
        #endregion

        #region Seasonal Functions
        public override void Advance()
        {
            // harvest vis
            foreach (Aura aura in KnownAuras)
            {
                foreach (VisSource source in aura.VisSources)
                {
                    if ((CurrentSeason & source.Seasons) == CurrentSeason)
                    {
                        _visStock[source.Art] += source.Amount;
                    }
                }
            }
            Log.Add("VIS STOCK");
            foreach(Ability art in MagicArts.GetEnumerator())
            {
                if(_visStock[art] > 0)
                {
                    Log.Add(art.AbilityName + ": " + _visStock[art].ToString("0.00"));
                }
            }
            base.Advance();
        }
        #endregion

        #region object Overrides
        public override string ToString()
        {
            return this.Name + " ex " + this.House.ToString();
        }
        #endregion
    }
}
