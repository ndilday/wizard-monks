using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Activities;
using WizardMonks.Beliefs;
using WizardMonks.Decisions.Goals;
using WizardMonks.Economy;
using WizardMonks.Instances;
using WizardMonks.Models;
using WizardMonks.Models.Spells;

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
        private HousesEnum _house;
        private bool _isBestBookCached;
        private ABook _bestBookCache;
        private Dictionary<Character, ushort> _decipheredShorthandLevels;
        private Dictionary<LabText, double> _shorthandTranslationProgress;
        private List<Idea> _ideas;
        #endregion

        #region Public Properties
        public Magus Apprentice { get; private set; }
        public Laboratory Laboratory { get; private set; }
        public List<Spell> SpellList { get; private set; }
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
            _magicAbility = magicAbility;
            Arts = new Arts(InvalidateWritableTopicsCache);
            Covenant = null;
            Laboratory = null;
            _visStock = [];
            SpellList = [];
            _labTextsOwned = [];
            _decipheredShorthandLevels = [];
            _shorthandTranslationProgress = [];
            //_tractatusGoals = new List<TractatusGoal>();
            //_summaGoals = new List<SummaGoal>();
            _partialSpell = null;
            _partialSpellProgress = 0;
            _ideas = [];
            VisStudyRate = 6.75;
            House = house;
            foreach (Ability art in MagicArts.GetEnumerator())
            {
                _visStock[art] = 0;
            }

            InitializeGoals();
        }

        private void InitializeGoals()
        {
            _goals.Add(new AvoidDecrepitudeGoal(this, 1.0));
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

        public override ABook GetBestBookToWrite()
        {
            if (_isBestBookCached) return _bestBookCache;
            if (GlobalEconomy.MostDesiredBookTopics == null) return null;

            // --- Step 1: Initialize variables for the search ---
            ABook bestBook = null;
            double currentBestBookValue = 0;
            double writingRate = GetAttributeValue(AttributeType.Communication) + GetAbility(_writingLanguage).Value;
            double prestigeMotivation = Personality.GetPrestigeMotivation();

            // --- Step 2: Ensure the cache of writable topics is populated ---
            if (!_isWritableTopicsCacheValid)
            {
                _writableTopicsCache = GetAbilities().Where(a => a.Experience >= 15).ToHashSet();
                _isWritableTopicsCacheValid = true;
            }

            // --- Step 3: Iterate through topics with known market demand ---
            foreach (Ability ability in GlobalEconomy.MostDesiredBookTopics)
            {
                //var charAbility = _writableTopicsCache.FirstOrDefault(a => a.Ability == ability);
                var charAbility = GetAbility(ability);
                if (charAbility == null) continue;

                var desiresForTopic = GlobalEconomy.DesiredBooksByTopic[ability];
                var highestDemandDesire = desiresForTopic.OrderBy(d => d.CurrentLevel).FirstOrDefault(d => d.Character != this);
                if (highestDemandDesire == null) continue;

                // --- Step 4A: Evaluate writing a Summa for this topic ---
                double maxLevel = GetAbility(highestDemandDesire.Ability).Value / 2.0;
                for (double l = Math.Floor(maxLevel); l > highestDemandDesire.CurrentLevel; l--)
                {
                    double q = 6 + GetAttributeValue(AttributeType.Communication) + maxLevel - l;

                    var prospectiveSumma = new Summa
                    {
                        Author = this,
                        Quality = q,
                        Level = l,
                        Topic = highestDemandDesire.Ability,
                        Title = $"{highestDemandDesire.Ability.AbilityName} L{l:0.0}Q{q:0.0} Summa"
                    };

                    // Calculate Economic Value (based on vis equivalence)
                    double seasonsLeft = Math.Ceiling(l / writingRate);
                    if (!MagicArts.IsArt(prospectiveSumma.Topic)) seasonsLeft *= 5;
                    double economicValue = RateLifetimeBookValue(prospectiveSumma) / seasonsLeft;

                    // Calculate Prestige Value
                    var payload = GenerateProspectiveBeliefPayload(prospectiveSumma);
                    double prestigeValue = payload.Sum(b => CalculateBeliefValue(b));

                    // Calculate Total Value, modulated by personality
                    double totalValue = economicValue + (prestigeValue * prestigeMotivation);

                    if (totalValue > currentBestBookValue)
                    {
                        currentBestBookValue = totalValue;
                        prospectiveSumma.Value = totalValue; // Store the combined value
                        bestBook = prospectiveSumma;
                    }
                }

                // --- Step 4B: Evaluate writing a Tractatus for this topic ---
                if (CanWriteTractatus(charAbility))
                {
                    ushort previouslyWrittenCount = GetTractatiiWrittenOnTopic(highestDemandDesire.Ability);
                    var prospectiveTractatus = new Tractatus
                    {
                        Author = this,
                        Quality = 6 + GetAttributeValue(AttributeType.Communication),
                        Topic = charAbility.Ability,
                        Title = $"{Name} {charAbility.Ability.AbilityName} T{previouslyWrittenCount}"
                    };

                    // Calculate Economic Value (vis value of 1 season of study)
                    double economicValue = RateSeasonalExperienceGain(prospectiveTractatus.Topic, prospectiveTractatus.Quality);

                    // Calculate Prestige Value
                    var payload = GenerateProspectiveBeliefPayload(prospectiveTractatus);
                    double prestigeValue = payload.Sum(b => CalculateBeliefValue(b));

                    // Calculate Total Value, modulated by personality
                    double totalValue = economicValue + (prestigeValue * prestigeMotivation);

                    if (totalValue > currentBestBookValue)
                    {
                        currentBestBookValue = totalValue;
                        prospectiveTractatus.Value = totalValue;
                        bestBook = prospectiveTractatus;
                    }
                }
            }

            // --- Step 5: Cache and return the result ---
            _isBestBookCached = true;
            _bestBookCache = bestBook;
            return bestBook;
        }

        /// <summary>
        /// Generates the prospective BeliefPayload for a book that has not yet been written.
        /// This is used for valuation purposes in the GetBestBookToWrite method.
        /// </summary>
        private List<Belief> GenerateProspectiveBeliefPayload(ABook book)
        {
            var payload = new List<Belief>();
            var authorComm = GetAttributeValue(AttributeType.Communication);

            if (book is Tractatus tract)
            {
                payload.Add(new Belief(tract.Topic.AbilityName, 5));
            }
            else if (book is Summa summa)
            {
                // For a Summa, the prestige is derived from both its Quality and its Level.
                double magnitude = BeliefToReputationNormalizer.ArtFromSumma(summa.Quality, summa.Level);
                payload.Add(new Belief(summa.Topic.AbilityName, magnitude));
            }

            // All books reflect on the author's Communication skill.
            payload.Add(new Belief("Communication", BeliefToReputationNormalizer.FromAttributeScore(authorComm)));

            return payload;
        }
        #endregion

        #region Lab Text Functions
        public IEnumerable<LabText> GetLabTextsFromCollection(SpellBase spellBase)
        {
            return _labTextsOwned.Where(t => t.SpellContained.Base == spellBase);
        }

        public void AddLabTextToCollection(LabText labText)
        {
            _labTextsOwned.Add(labText);
        }

        public void RemoveLabTextFromCollection(LabText labText)
        {
            _labTextsOwned.Remove(labText);
        }

        public IEnumerable<LabText> GetUnneededLabTextsFromCollection()
        {
            List<LabText> unneededLabTexts = [];
            foreach(LabText labText in _labTextsOwned)
            {
                bool unneeded = false;
                foreach(Spell spell in this.SpellList)
                {
                    // if the mage already knows the spell, the lab text is unneeded
                    if (spell == labText.SpellContained)
                    {
                        unneeded = true;
                        break;
                    }
                    // if the mage already knows a better version of the spell, the lab text is unneeded
                    else if (spell.Base == labText.SpellContained.Base && spell.Level > labText.SpellContained.Level)
                    {
                        unneeded = true;
                    }
                }
                if (unneeded)
                {
                    unneededLabTexts.Add(labText);
                }

            }
            return unneededLabTexts;
        }

        /// <summary>
        /// Determines the value of a given lab text to this magus in an equivalent pawn-value of Vim vis.
        /// The value is based on the number of seasons the magus would save by learning from the text
        /// instead of inventing the spell from scratch.
        /// </summary>
        /// <param name="labText">The lab text to evaluate.</param>
        /// <returns>The value of the lab text in pawns of Vim vis. Returns 0 if the text is unusable or not beneficial.</returns>
        public double RateLifetimeLabTextValue(LabText labText)
        {
            // If we already know this spell or a better version of it, the text has no value.
            if (this.SpellList.Any(s => s.Base == labText.SpellContained.Base && s.Level >= labText.SpellContained.Level))
            {
                return 0;
            }

            // To learn from a lab text, the magus's Lab Total must be greater than the spell's level.
            double labTotal = this.GetSpellLabTotal(labText.SpellContained);
            if (labTotal < labText.SpellContained.Level)
            {
                return 0;
            }

            // --- Step 2: Calculate the seasons required to invent the spell from scratch ---

            // Invention requires accumulating 'Level' points of progress.
            double inventionPointsNeeded = labText.SpellContained.Level;

            // Progress per season is the amount the Lab Total exceeds the spell's level.
            double inventionProgressPerSeason = labTotal - labText.SpellContained.Level;

            // This case should be caught by the labTotal check above, but as a safeguard against division by zero.
            if (inventionProgressPerSeason == 0)
            {
                inventionProgressPerSeason = 1;
            }

            // Calculate how many full seasons it would take to gain the required points.
            // A fraction of a season's work still consumes the entire season.
            double seasonsToInvent = Math.Ceiling(inventionPointsNeeded / inventionProgressPerSeason);

            // --- Step 3: Calculate seasons saved and convert to vis value ---

            // Learning from a lab text takes a single season.
            int seasonsToLearnFromText = labText.IsShorthand ? 2 : 1;
            double seasonsSaved = seasonsToInvent - seasonsToLearnFromText;

            // If it takes 1 or fewer seasons to invent, the lab text provides no time savings and has no value.
            if (seasonsSaved <= 0)
            {
                return 0;
            }
            if(seasonsSaved > 2)
            {
                // decrement by one to account for how the inventing mage's Magic Theory will increase along the way
                seasonsSaved -= 1;
            }

            // The value of a saved season is equivalent to the amount of Vim vis that could be distilled in that time.
            // This serves as our universal "opportunity cost" currency for the AI.
            double visDistilledPerSeason = this.GetVisDistillationRate();
            double visValue = seasonsSaved * visDistilledPerSeason;

            return visValue;
        }

        public bool CanUseLabText(LabText text)
        {
            if (!text.IsShorthand)
            {
                return true; // Not shorthand, so it's usable by anyone with Magic Theory.
            }

            // If it's our own shorthand, we can always use it.
            if (text.Author == this)
            {
                return true;
            }

            // Check if we've deciphered this author's shorthand to a sufficient level.
            if (_decipheredShorthandLevels.TryGetValue(text.Author, out ushort decipheredLevel))
            {
                return text.SpellContained.Level <= decipheredLevel;
            }

            return false; // We have no understanding of this author's shorthand.
        }

        public ushort? GetDeciperedLabTextLevel(Character author)
        {
            if (_decipheredShorthandLevels.TryGetValue(author, out ushort currentProgress)) return currentProgress;
            return null;
        }

        public double? GetLabTextTranslationProgress(LabText text)
        {
            if(_shorthandTranslationProgress.TryGetValue(text, out double currentProgress)) return currentProgress;
            return null;
        }

        public void SetLabTextTranslationProgress(LabText text, double progress)
        {
            _shorthandTranslationProgress[text] = progress;
        }

        public void AddDecipheredLabTextLevel(Character author, ushort level)
        {
            // remove any partial translations of this author of this level or below
            foreach(var kvp in _shorthandTranslationProgress)
            {
                if(kvp.Key.Author == author && kvp.Key.SpellContained.Level <= level)
                {
                    _shorthandTranslationProgress.Remove(kvp.Key);
                }
            }
            if(!_decipheredShorthandLevels.ContainsKey(author) || _decipheredShorthandLevels[author] < level)
            {
                _decipheredShorthandLevels[author] = level;
            }
        }

        public double GetLabTextWritingRate()
        {
            // Latin skill * 20 levels per season
            return GetAbility(_writingLanguage).Value * 20;
        }

        public double GetLabTextCopyingRate()
        {
            // Profession: Scribe skill * 60 levels per season
            return GetAbility(Abilities.Scribing).Value * 60;
        }
        #endregion

        #region Idea Functions
        public IEnumerable<Idea> GetInspirations()
        {
            return _ideas;
        }

        public void AddIdea(Idea idea)
        {
            // Prevent adding duplicate ideas, if we later decide ideas can be shared
            if (!_ideas.Any(i => i.Id == idea.Id))
            {
                _ideas.Add(idea);
                Log.Add($"Gained a new idea: {idea.Description}");

                // Add a new goal to pursue this inspiration
                _goals.Add(new PursueIdeaGoal(this, idea));
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

        protected IEnumerable<LabTextForTrade> EvaluateLabTextValuesAsSeller(IEnumerable<LabText> labTexts)
        {
            List<LabTextForTrade> list = [];
            double distillRate = GetVisDistillationRate();
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
                    double writeRate = GetAbility(_writingLanguage).Value * 20;
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
                _desires.VisDesires,
                _desires.BookDesires,
                EvaluateBookValuesAsSeller(GetUnneededBooksFromCollection()),
                _desires.LabTextDesires,
                EvaluateLabTextValuesAsSeller(GetUnneededLabTextsFromCollection())
            );
            if (_tradeDesires == null)
            {
                throw new NullReferenceException();
            }
            return _tradeDesires;
        }

        private void UpdateVisDesiresWithStock()
        {
            foreach (VisDesire visDesire in _desires.VisDesires)
            {
                if (_visStock.ContainsKey(visDesire.Art))
                {
                    visDesire.Quantity -= _visStock[visDesire.Art];
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
                // Check if seller still owns the book and buyer can afford it.
                if (GetBooksFromCollection(sellOffer.BookDesired.Topic).Contains(sellOffer.BookDesired) &&
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
                    GainVis(sellOffer.VisOffers);
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
                bool weStillHaveTheText = GetLabTextsFromCollection(sellOffer.LabTextDesired.SpellContained.Base).Contains(sellOffer.LabTextDesired);
                bool buyerCanAfford = sellOffer.TradingPartner.HasSufficientVis(sellOffer.VisOffers);

                // Buyer still needs logic: Check if they don't have this spell base, or if they have it but at a lower level.
                bool buyerStillNeedsText = !sellOffer.TradingPartner.GetLabTextsFromCollection(sellOffer.LabTextDesired.SpellContained.Base).Any(l => l.SpellContained.Level >= sellOffer.LabTextDesired.SpellContained.Level);

                if (weStillHaveTheText && buyerCanAfford && buyerStillNeedsText)
                {
                    // Execute the trade:
                    Log.Add($"Selling Lab Text for '{sellOffer.LabTextDesired.SpellContained.Name}' to {sellOffer.TradingPartner.Name} for {sellOffer.VisValue:0.00} vis value.");
                    sellOffer.TradingPartner.Log.Add($"Buying Lab Text for '{sellOffer.LabTextDesired.SpellContained.Name}' from {this.Name}.");

                    // Transfer ownership of the lab text:
                    sellOffer.TradingPartner.AddLabTextToCollection(sellOffer.LabTextDesired);
                    RemoveLabTextFromCollection(sellOffer.LabTextDesired); // Seller removes from their own inventory.

                    // Transfer vis:
                    sellOffer.TradingPartner.UseVis(sellOffer.VisOffers); // Buyer spends vis.
                    GainVis(sellOffer.VisOffers); // Seller gains vis.

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
                    offer.Mage.Log.Add($"Swapping Lab Text '{offer.LabTextDesired.SpellContained.Name}' with {this.Name} for '{offer.LabTextOffered.SpellContained.Name}'.");

                    // Execute swap
                    this.AddLabTextToCollection(offer.LabTextDesired);
                    this.RemoveLabTextFromCollection(offer.LabTextOffered);
                    offer.Mage.AddLabTextToCollection(offer.LabTextOffered);
                    offer.Mage.RemoveLabTextFromCollection(offer.LabTextDesired);
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

        public void AddFeatureToLaboratory(LabFeature feature)
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
                foreach (var belief in text.BeliefPayload)
                {
                    // Update belief about the author
                    GetBeliefProfile(text.Author).AddOrUpdateBelief(new Belief(belief.Topic, belief.Magnitude));

                    // Update stereotype about the author's house
                    if (text.Author is Magus magus)
                    {
                        var houseSubject = Houses.GetSubject(magus.House);
                        GetBeliefProfile(houseSubject).AddOrUpdateBelief(new Belief(belief.Topic, belief.Magnitude * 0.20)); // Stereotype is 20% strength
                    }
                }
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
            // Generate Belief Payload for the shorthand lab text
            double magnitude = spell.Level / 5.0;
            newLabText.BeliefPayload.Add(new Belief(spell.Base.ArtPair.Technique.AbilityName, magnitude));
            newLabText.BeliefPayload.Add(new Belief(spell.Base.ArtPair.Form.AbilityName, magnitude));

            // Generate personality beliefs based on spell tags
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
        public override IActivity Advance()
        {
            _isBestBookCached = false;
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
            IActivity activity = base.Advance();
            IdeaManager.CheckForIdea(this, activity);
            return activity;
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
