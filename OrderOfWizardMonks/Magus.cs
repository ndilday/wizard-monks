using System;
using System.Collections.Generic;
using System.Linq;

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
            //_tractatusGoals = new List<TractatusGoal>();
            //_summaGoals = new List<SummaGoal>();
            _partialSpell = null;
            _partialSpellProgress = 0;
            VisStudyRate = 6;
            House = Houses.Apprentice;
            foreach (Ability art in MagicArts.GetEnumerator())
            {
                _visStock[art] = 0;
            }

            InitializeGoals();
        }

        private void InitializeGoals()
        {
            uint seasonsLeftToAging = 140 - SeasonalAge;

            _goals.Add(new LongevityRitualGoal(this, 0, seasonsLeftToAging));
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
            VisStudyRate = 6 + covenant.Aura.Strength;
        }

        public Covenant FoundCovenant(Aura aura)
        {
            Covenant coventant = new Covenant(aura);
            Join(coventant);
            return coventant;
        }
        #endregion

        #region Book Functions
        public override IEnumerable<IBook> GetBooksFromCollection(Ability ability)
        {
            IEnumerable<IBook> books = _booksOwned.Where(b => b.Topic == ability);
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
                List<BookDesire> bookDesires = new List<BookDesire>();
                IList<BookDesire> bookNeeds;
                foreach (IGoal goal in _goals)
                {
                    bookNeeds = goal.GetBookNeeds(this);
                    if (bookNeeds != null)
                    {
                        bookDesires.AddRange(bookNeeds);
                    }
                }
                return bookDesires;
            }
            return new List<BookDesire>();
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
            double baseDistillVisRate = GetLabTotal(MagicArtPairs.CrVi, Activity.DistillVis) / 10.0;
            double distillVisRate = baseDistillVisRate;
            if (MagicArts.IsTechnique(ability))
            {
                distillVisRate /= 4.0;
            }
            else if (MagicArts.IsForm(ability) && ability != MagicArts.Vim)
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

        protected IEnumerable<BookForTrade> EvaluateBookValuesAsSeller(IEnumerable<IBook> books)
        {
            List<BookForTrade> list = new List<BookForTrade>();
            double distillRate = GetLabTotal(MagicArtPairs.CrVi, Activity.DistillVis) / 10;
            foreach (IBook book in books)
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
        
        private void AddWritingGoals(MagusTradingDesires tradingDesires)
        {
            // TODO: right now, this logic is causing characters to start trying to learn skills
            // they have no interest in so that they can write a book to sell off
            // this may not be inherently wrong, but it should certainly be the case that more attractive choices always exist
            double comm = GetAttributeValue(AttributeType.Communication);
            foreach (BookDesire bookDesire in tradingDesires.BookDesires.Values)
            {
                CharacterAbilityBase charAbility = GetAbility(bookDesire.Ability);
                bool isArt = MagicArts.IsArt(bookDesire.Ability);
                if (!_tractatusGoals.Where(t => !t.IsComplete(this) && t.Topic == bookDesire.Ability).Any() && (isArt || comm > -2))
                {
                    // add tractatus goal to both goal list and writing goal list
                    ushort previouslyWrittenCount = GetTractatiiWrittenOnTopic(bookDesire.Ability);
                    string name = Name + " " + bookDesire.Ability.AbilityName + " T" + previouslyWrittenCount.ToString();
                    TractatusGoal tractGoal = new TractatusGoal(bookDesire.Ability, name, previouslyWrittenCount);
                    _tractatusGoals.Add(tractGoal);
                    _goals.Add(tractGoal);
                }
                double baseLevel = charAbility.Value / 2.0;
                // don't add a summa goal if we already have one for this topic in progress
                if (baseLevel - 1 > bookDesire.CurrentLevel && !_summaGoals.Where(s => !s.IsComplete(this) && s.Topic == charAbility.Ability).Any())
                {
                    // try to constrain the level of the summa to be something doable in an efficient period of time
                    double rate = comm + GetAbility(_writingLanguage).Value;
                    double writingPointsNeeded = isArt ? bookDesire.CurrentLevel + 1 : bookDesire.CurrentLevel * 5 + 5;
                    double efficientLevel = Math.Ceiling(writingPointsNeeded / rate) * rate;
                    if (efficientLevel < baseLevel)
                    {
                        // add summa goal to both goal list and writing goal list
                        string name = Name + " " + bookDesire.Ability.AbilityName + " Summa L" + efficientLevel.ToString("0.00");
                        SummaGoal summaGoal = new SummaGoal(bookDesire.Ability, efficientLevel, name);
                        _goals.Add(summaGoal);
                        _summaGoals.Add(summaGoal);
                    }
                    else if (writingPointsNeeded < rate)
                    {
                        // it's a one-season summa, may as well do it if it's better than practice
                        // but we should make sure that it's worth at least two seasons of study
                        double quality = comm + 6;
                        double xpToLevel = baseLevel * (baseLevel + 1) / 2.0;
                        if(!isArt)
                        {
                            xpToLevel *= 5;
                        }
                        if (xpToLevel >= 2 * quality && (isArt || quality > 4))
                        {
                            string name = Name + " " + bookDesire.Ability.AbilityName + " Summa L" + (charAbility.Value / 2.0).ToString("0.00");
                            SummaGoal summaGoal = new SummaGoal(bookDesire.Ability, baseLevel, name);
                            _goals.Add(summaGoal);
                            _summaGoals.Add(summaGoal);
                        }
                    }
                }
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
            List<VisTradeOffer> offers = new List<VisTradeOffer>();
            List<BookTradeOffer> bookTradeOffers = new List<BookTradeOffer>();
            List<BookVisOffer> buyBookOffers = new List<BookVisOffer>();
            List<BookVisOffer> sellBookOffers = new List<BookVisOffer>();
            foreach (MagusTradingDesires tradeDesires in mageTradeDesires)
            {
                if (tradeDesires.Mage == this)
                {
                    continue;
                }

                AddWritingGoals(tradeDesires);

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
                    offers.AddRange(visOffersGenerated);
                }
            }

            ProcessVisOffers(offers);
            ProcessBookOffers(bookTradeOffers, buyBookOffers, sellBookOffers);
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
                        Log.Add("Trading " + offer.Bid.Quantity.ToString("0.00") + " pawns of " + offer.Bid.Art.AbilityName + " vis");
                        Log.Add("for " + offer.Ask.Quantity.ToString("0.00") + " pawns of " + offer.Ask.Art.AbilityName + " vis");
                        offer.Mage.Log.Add("Executing vis trade with " + Name);
                        offer.Mage.Log.Add("Trading " + offer.Ask.Quantity.ToString("0.00") + " pawns of " + offer.Ask.Art.AbilityName + " vis");
                        offer.Mage.Log.Add("for " + offer.Bid.Quantity.ToString("0.00") + " pawns of " + offer.Bid.Art.AbilityName + " vis");
                        
                        
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

        private void ProcessBookOffers(IEnumerable<BookTradeOffer> bookTradeOffers, IEnumerable<BookVisOffer> bookBuys, IEnumerable<BookVisOffer> bookSales)
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
                    bookSales = bookSales.Where(b => b.BookDesired != tradeOffer.BookOffered);
                    bookBuys = bookBuys.Where(b => b.BookDesired != tradeOffer.BookDesired);
                    trades = trades.Where(bto => bto.BookDesired != tradeOffer.BookDesired &&
                                                 bto.BookOffered != tradeOffer.BookDesired)
                                   .OrderBy(bto => bto.BookDesired.Quality);
                }
                else
                {
                    trades = trades.Where(t => t != tradeOffer).OrderBy(bto => bto.BookDesired.Quality);
                }
            }

            var sales = bookSales.OrderByDescending(bto => bto.VisQuantity);
            while (sales.Any() )
            {
                var sellOffer = sales.First();
                if (GetBooksFromCollection(sellOffer.BookDesired.Topic).Contains(sellOffer.BookDesired) &&
                    sellOffer.Mage.GetVisCount(sellOffer.VisArt) >= sellOffer.VisQuantity)
                {
                    Log.Add("Selling " + sellOffer.BookDesired.Title + " to " + sellOffer.Mage.Name);
                    Log.Add("for " + sellOffer.VisQuantity.ToString("0.00") + " pawns of " + sellOffer.VisArt.AbilityName + " vis");
                    sellOffer.Mage.Log.Add("Buying " + sellOffer.BookDesired.Title + " from " + Name);
                    sellOffer.Mage.Log.Add("for " + sellOffer.VisQuantity.ToString("0.00") + " pawns of " + sellOffer.VisArt.AbilityName + " vis");

                    sellOffer.Mage.AddBookToCollection(sellOffer.BookDesired);
                    RemoveBookFromCollection(sellOffer.BookDesired);
                    sellOffer.Mage.UseVis(sellOffer.VisArt, sellOffer.VisQuantity);
                    GainVis(sellOffer.VisArt, sellOffer.VisQuantity);
                }
                sales = sales.Where(s => s.BookDesired != sellOffer.BookDesired).OrderBy(bto => bto.VisQuantity);
            }

            var buys = bookBuys.OrderBy(bto => bto.BookDesired.Quality).ThenBy(bto => bto.VisQuantity);
            while (buys.Any())
            {
                var buyOffer = buys.First();
                if (GetVisCount(buyOffer.VisArt) >= buyOffer.VisQuantity)
                {
                    Log.Add("Buying " + buyOffer.BookDesired.Title + " from " + buyOffer.Mage.Name);
                    Log.Add("for " + buyOffer.VisQuantity.ToString("0.00") + " pawns of " + buyOffer.VisArt.AbilityName + " vis");
                    buyOffer.Mage.Log.Add("Selling " + buyOffer.BookDesired.Title + " to " + Name);
                    buyOffer.Mage.Log.Add("for " + buyOffer.VisQuantity.ToString("0.00") + " pawns of " + buyOffer.VisArt.AbilityName + " vis");

                    buyOffer.Mage.RemoveBookFromCollection(buyOffer.BookDesired);
                    AddBookToCollection(buyOffer.BookDesired);
                    buyOffer.Mage.GainVis(buyOffer.VisArt, buyOffer.VisQuantity);
                    UseVis(buyOffer.VisArt, buyOffer.VisQuantity);
                }
                buys = buys.Where(b => b.BookDesired != buyOffer.BookDesired).OrderBy(bto => bto.VisQuantity);
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
            VisDesire[] desires = new VisDesire[15];
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
                goal.ModifyVisNeeds(this, desires);
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
            }

            //TODO: foci
            //TODO: lab assistant
            //TODO: familiar
            return labTotal;
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
            _visStock[MagicArts.Vim] += GetLabTotal(MagicArtPairs.CrVi, Activity.DistillVis) / 10;

            // grant exposure experience
            GetAbility(exposureAbility).AddExperience(2);
        }

        public void InventSpell(Spell spell)
        {
            // TODO: multiple spells in a season
            // TODO: foci
            // TODO: Working from Lab Text
            double labTotal = GetLabTotal(spell.BaseArts, Activity.InventSpells);
            if(spell == _partialSpell)
            {
                // continue previous spell work
                _partialSpellProgress += labTotal - spell.Level;
                if (_partialSpellProgress >= _partialSpell.Level)
                {
                    SpellList.Add(_partialSpell);
                    _partialSpell = null;
                    _partialSpellProgress = 0;
                }
            }
            if(labTotal <= spell.Level)
            {
                throw new ArgumentException("This mage cannot invent this spell!");
            }
            else if (labTotal >= spell.Level * 2)
            {
                SpellList.Add(spell);
                _partialSpell = null;
                _partialSpellProgress = 0;
            }
            else
            {
                _partialSpell = spell;
                _partialSpellProgress = labTotal - spell.Level;
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
                IGoal teachingGoal = new TeachingApprenticeGoal(apprentice, 3, 0, dueDate);
                _goals.Add(teachingGoal);
            }
            IGoal gauntletGoal = new GauntletGoal(apprentice, 3, 0, 60);
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
