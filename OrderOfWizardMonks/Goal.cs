using System;
using System.Collections.Generic;
using System.Linq;

using WizardMonks.Instances;

namespace WizardMonks
{
    public class ConsideredActions
    {
        Dictionary<Activity, IList<IAction>> ActionTypeMap = new Dictionary<Activity,IList<IAction>>();
        
        public void Add(IAction action)
        {
            if (!ActionTypeMap.ContainsKey(action.Action))
            {
                ActionTypeMap[action.Action] = new List<IAction>();
                ActionTypeMap[action.Action].Add(action);
            }
            else
            {
                var match = ActionTypeMap[action.Action].Where(a => a.Matches(action)).FirstOrDefault();
                if (match != null)
                {
                    match.Desire += action.Desire;
                }
                else
                {
                    ActionTypeMap[action.Action].Add(action);
                }
            }
        }

        public IList<string> Log()
        {
            List<string> log = new List<string>();
            log.Add("----------");
            log.AddRange(ActionTypeMap.SelectMany(a => a.Value).Select(a => a.Log()));
            log.Add("----------");
            return log;
        }
        
        public IAction GetBestAction()
        {
            return ActionTypeMap.SelectMany(a => a.Value).OrderByDescending(a => a.Desire).FirstOrDefault();
        }
    }

    public interface IGoal
    {
        uint? DueDate { get; }
        byte Tier { get; }
        double Desire { get; set; }

        void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log);
        bool IsComplete(Character character);
        bool DecrementDueDate();
        void ModifyVisNeeds(Character character, VisDesire[] desires);
        // TODO: add a boolean to Goals to cache completeness
        // probably should not cache in reversable cases, i.e. labs
        IList<BookDesire> GetBookNeeds(Character character);
    }

    #region Basic (1-tier) Goals
    public abstract class BaseGoal : IGoal
    {
        public uint? DueDate {get; private set;}
        public byte Tier {get; private set;}
        public double Desire {get; set;}
        public virtual bool DecrementDueDate()
        {
            if(DueDate != null)
            {
                DueDate--;
                return DueDate != 0;
            }
            return true;
        }
        public abstract bool IsComplete(Character character);
        public abstract void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log);
        public abstract void ModifyVisNeeds(Character character, VisDesire[] desires);
        public abstract IList<BookDesire> GetBookNeeds(Character character);
        public BaseGoal(double desire, byte tier = 0, uint? dueDate = null)
        {
            DueDate = dueDate;
            Desire = desire;
            Tier = tier;
        }
    }

    class AbilityScoreCondition : BaseGoal
    {
        #region Protected fields
        protected List<Ability> _abilities;
        protected double _total;
        #endregion

        public AbilityScoreCondition(List<Ability> abilities, double total, double desire, byte tier = 0, uint? dueDate = null) : base(desire, tier, dueDate)
        {
            _abilities = abilities;
            _total = total;
        }

        public AbilityScoreCondition(Ability ability, double total, double desire, byte tier = 0, uint? dueDate = null) : base(desire, tier, dueDate)
        {
            _abilities = new List<Ability>();
            _abilities.Add(ability);
            _total = total;
        }

        public override void ModifyVisNeeds(Character character, VisDesire[] desires)
        {
            if (!IsComplete(character))
            {
                foreach (Ability ability in _abilities)
                {
                    if (MagicArts.IsArt(ability)  && character.GetType() == typeof(Magus))
                    {
                        CharacterAbilityBase charAbility = character.GetAbility(ability);
                        double experienceNeeded = charAbility.GetExperienceUntilLevel(_total);
                        double visPer = 0.5 + (charAbility.Value + _total) / 20.0;
                        double visNeeded = experienceNeeded * visPer / ((Magus)(character)).VisStudyRate;
                        desires[charAbility.Ability.AbilityId % 300].Quantity += visNeeded;
                    }
                }
            }
        }

        public override IList<BookDesire> GetBookNeeds(Character character)
        {
            if (IsComplete(character) || IsCompletable(character))
            {
                return null;
            }
            List<BookDesire> bookDesires = new List<BookDesire>();
            double abilityCount = _abilities.Count;
            foreach (Ability ability in _abilities)
            {
                double currentLevel = character.GetAbility(ability).Value;
                //double minLevel = ((_total - currentLevel) / abilityCount) + currentLevel;
                bookDesires.Add(new BookDesire(ability, currentLevel));
            }
            return bookDesires;
        }

        private bool IsCompletable(Character character)
        {
            // see if the current resources the character posesses are sufficient to reach the goal
            // for now, this will be: are all the books available to the character sufficient?
            double runningTotal = 0;
            foreach (Ability ability in _abilities)
            {
                runningTotal += character.GetAbilityMaximumFromReading(ability);
            }

            return runningTotal >= _total;
        }

        public override void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log)
        {
            double remainingTotal = GetRemainingTotal(character);
            double dueDateDesire = Desire / (Tier + 1);
            if (DueDate != null)
            {
                if (DueDate == 0)
                {
                    log.Add("Ability Condition failed!");
                    return;
                }
                dueDateDesire /= (double)DueDate;
            }
            if (dueDateDesire > 0.01)
            {
                IEnumerable<IBook> readableBooks = character.ReadableBooks;
                foreach (Ability ability in _abilities)
                {
                    bool isArt = MagicArts.IsArt(ability);
                    CharacterAbilityBase charAbility = character.GetAbility(ability);

                    // Handle Reading
                    var topicalBooks = readableBooks.Where(b => b.Topic == ability);
                    AddReading(character, alreadyConsidered, topicalBooks, remainingTotal, dueDateDesire);

                    // Abilities get practice, arts get vis study
                    if (!isArt && (ability.AbilityType != AbilityType.Supernatural || charAbility.Value > 0))
                    {
                        double desire = dueDateDesire * charAbility.GetValueGain(4) / remainingTotal;
                        log.Add("Practicing " + ability.AbilityName + " worth " + desire.ToString("0.00"));
                        Practice practiceAction = new Practice(ability, desire);
                        alreadyConsidered.Add(practiceAction);
                    }
                    else if (isArt && character.GetType() == typeof(Magus))
                    {
                        Magus mage = (Magus)character;
                        HandleVisUse(mage, charAbility, remainingTotal, dueDateDesire, alreadyConsidered, log);
                    }

                    // TODO: Learning By Training
                    // TODO: Learning by Teaching
                }
            }
        }

        private void HandleVisUse(Magus mage, CharacterAbilityBase charAbility, double remainingTotal, double desire, ConsideredActions alreadyConsidered, IList<string> log )
        {
            // see if the mage has enough vis of this type
            double stockpile = mage.GetVisCount(charAbility.Ability);
            double visNeed = 0.5 + (charAbility.Value / 10.0);
            double baseDesire = desire * charAbility.GetValueGain(mage.VisStudyRate) / remainingTotal;
            // if so, assume vis will return an average of 6XP + aura
            if (stockpile > visNeed)
            {
                log.Add("Studying vis for " + charAbility.Ability.AbilityName + " worth " + baseDesire.ToString("0.00"));
                VisStudying visStudy = new VisStudying(charAbility.Ability, baseDesire);
                alreadyConsidered.Add(visStudy);
                // TODO: how do we decrement the cost of the vis?
            }
            else if(baseDesire > 0.01 && (DueDate == null || DueDate > 1))
            {
                // only try to extract the vis now if there's sufficient time to do so
                List<Ability> visType = new List<Ability>();
                visType.Add(charAbility.Ability);
                VisCondition visCondition = new VisCondition(visType, visNeed - stockpile, baseDesire, (byte)(Tier + 1), DueDate == null ? null : DueDate - 1);
                visCondition.ModifyActionList(mage, alreadyConsidered, log);
            }
        }

        public override bool IsComplete(Character character)
        {
            return GetRemainingTotal(character) <= 0;
        }

        public virtual double GetRemainingTotal(Character character)
        {
            double currentTotal = 0;
            foreach (Ability ability in _abilities)
            {
                currentTotal += character.GetAbility(ability).Value;
            }
            return _total - currentTotal;
        }

        private void AddReading(Character character, ConsideredActions alreadyConsidered, IEnumerable<IBook> topicalBooks, double remainingTotal, double desire)
        {
            if (topicalBooks.Any())
            {
                var bestBook =
                    (from book in topicalBooks
                     orderby character.GetBookLevelGain(book),
                             book.Level ascending
                     select new EvaluatedBook
                     {
                         Book = book,
                         PerceivedValue = character.GetBookLevelGain(book) * desire / remainingTotal
                     }).First();
                // check to see if reading this book is already in the action list
                Reading readingAction = new Reading(bestBook.Book, bestBook.PerceivedValue);
                alreadyConsidered.Add(readingAction);
            }
        }
    }

    class CharacteristicAbilityScoreCondition : AbilityScoreCondition
    {
        protected List<AttributeType> _attributes;

        public CharacteristicAbilityScoreCondition(List<Ability> abilities, List<AttributeType> attributes, double total, double desire, byte tier, uint? dueDate = null) :
            base(abilities, total, desire, tier, dueDate)
        {
            _attributes = attributes;
        }

        public override double GetRemainingTotal(Character character)
        {
            double currentTotal = 0;
            foreach (Ability ability in _abilities)
            {
                currentTotal += character.GetAbility(ability).Value;
            }
            foreach(AttributeType attribute in _attributes)
            {
                currentTotal += character.GetAttribute(attribute).Value;
            }
            return _total - currentTotal;
        }

        public override void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log)
        {
            base.ModifyActionList(character, alreadyConsidered, log);
            // TODO: consider Cr Co/Me spells that increase statistics
            foreach (AttributeType attribute in _attributes)
            {
                double currentAttribute = character.GetAttribute(attribute).Value;
                if (currentAttribute < 5)
                {
                    ArtPair artPair;
                    if (attribute == AttributeType.Dexterity || attribute == AttributeType.Quickness ||
                        attribute == AttributeType.Stamina || attribute == AttributeType.Strength)
                    {
                        artPair = MagicArtPairs.CrCo;
                    }
                    else
                    {
                        artPair = MagicArtPairs.CrMe;
                    }

                    double total = 30;
                    if (currentAttribute > 0)
                    {
                        total = 35;
                    }
                    if (currentAttribute > 1)
                    {
                        total = 40;
                    }
                    if (currentAttribute > 2)
                    {
                        total = 45;
                    }
                    if (currentAttribute > 3)
                    {
                        total = 50;
                    }
                    if (currentAttribute > 4)
                    {
                        total = 55;
                    }
                    // TODO: see if the mage already knows a ritual of sufficient strength
                    // If not, consider the value of such a ritual
                }
            }

        }
    }

    class GauntletGoal : BaseGoal
    {
        private Magus _apprentice;

        public GauntletGoal(Magus apprentice, double desire, byte tier = 0, uint? dueDate = null) : base(desire, tier, dueDate)
        {
            _apprentice = apprentice;
        }

        public override bool IsComplete(Character character)
        {
            return character.GetType() == typeof(Magus) && ((Magus)(character)).Apprentice != _apprentice;
        }

        public override void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log)
        {
            double dueDateDesire = Desire / (Tier + 1);
            if (DueDate != null && DueDate > 0)
            {
                dueDateDesire /= (double)DueDate;
            }
            alreadyConsidered.Add(new GauntletApprentice(Abilities.MagicTheory, dueDateDesire));
        }

        public override void ModifyVisNeeds(Character character, VisDesire[] desires)
        {
        }

        public override IList<BookDesire> GetBookNeeds(Character character)
        {
            return null;
        }
    }
    #endregion

    #region Complex Goals
    class TractatusGoal : IGoal
    {
        // TODO: for now, writing goals are going to assume that 
        // the ability to write the book in question is already in place
        private string _name;
        private ushort _previouslyWritten;
        private bool _isArt;

        private AbilityScoreCondition _abilityScoreCondition;
        private AbilityScoreCondition _writingAbilityCondition;
        private AbilityScoreCondition _languageAbilityCondition;

        public Ability Topic { get; private set; }
        public double Desire { get; set; }
        public byte Tier { get; private set; }
        public uint? DueDate { get; private set; }

        public TractatusGoal(Ability topic, string name, ushort writtenCount, byte tier = 0, uint? dueDate = null)
        {
            Topic = topic;
            Tier = tier;
            DueDate = dueDate;

            _name = name;
            _previouslyWritten = writtenCount;
            _isArt = MagicArts.IsArt(topic);

            _writingAbilityCondition = new AbilityScoreCondition(Abilities.ArtesLiberales, 1, 0, (byte)(tier + 1), dueDate == null ? null : dueDate - 3);
            _languageAbilityCondition = new AbilityScoreCondition(Abilities.Latin, 5, 0, (byte)(tier + 1), dueDate == null ? null : dueDate - 2);
            double scoreNeeded = MagicArts.IsArt(topic) ? 5 * writtenCount + 5 : writtenCount + 1;
            if(scoreNeeded < 2)
            {
                scoreNeeded = 2;
            }
            _abilityScoreCondition = new AbilityScoreCondition(topic, scoreNeeded, 0, (byte)(tier + 1), dueDate == null ? null : dueDate - 1);
        }

        public bool IsComplete(Character character)
        {
            return character.HasWrittenBookWithTitle(_name);
        }

        public bool DecrementDueDate()
        {
            if (DueDate != null)
            {
                return
                    _abilityScoreCondition.DecrementDueDate() &&
                    _languageAbilityCondition.DecrementDueDate() &&
                    _writingAbilityCondition.DecrementDueDate() &&
                    --DueDate != 0;
            }
            return true;
        }

        public void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log)
        {
            double desire = CalculateDesire(character);
            bool alDone = _writingAbilityCondition.IsComplete(character);
            bool latinDone = _languageAbilityCondition.IsComplete(character);
            bool abilityDone = _abilityScoreCondition.IsComplete(character);
            if (!alDone)
            {
                _writingAbilityCondition.Desire = desire;
                _writingAbilityCondition.ModifyActionList(character, alreadyConsidered, log);
            }
            if (!latinDone)
            {
                _languageAbilityCondition.Desire = desire;
                _languageAbilityCondition.ModifyActionList(character, alreadyConsidered, log);
            }
            if (!abilityDone)
            {
                _abilityScoreCondition.Desire = desire;
                _abilityScoreCondition.ModifyActionList(character, alreadyConsidered, log);
            }
            if (abilityDone && latinDone && alDone)
            {
                alreadyConsidered.Add(new Writing(Topic, _name, Topic, 1000, desire));
            }
        }

        private double CalculateDesire(Character character)
        {
            double quality = character.GetAttribute(AttributeType.Communication).Value + 6;
            if (_isArt)
            {
                // for now, assume a reader of skill 5, so 1 pawn of vis/season
                return quality / ((Magus)character).VisStudyRate;
            }
            else
            {
                // right now, ability books are
                // getting valued more highly than art books
                // later, when we put in a better economic model,
                // this will float
                // for now, halve the ability book qualities
                return quality / 8;
            }
        }

        public void ModifyVisNeeds(Character character, VisDesire[] desires)
        {
            _abilityScoreCondition.ModifyVisNeeds(character, desires);
        }

        public IList<BookDesire> GetBookNeeds(Character character)
        {
            if(IsComplete(character))
            {
                return null;
            }
            return _abilityScoreCondition.GetBookNeeds(character);
        }
    }

    class SummaGoal : IGoal
    {
        // TODO: for now, writing goals are going to assume that 
        // the ability to write the book in question is already in place
        private string _name;
        private bool _isArt;

        private AbilityScoreCondition _abilityScoreCondition;
        private AbilityScoreCondition _writingAbilityCondition;
        private AbilityScoreCondition _languageAbilityCondition;

        public Ability Topic { get; private set; }
        public double Desire { get; set; }
        public byte Tier { get; private set; }
        public uint? DueDate { get; private set; }
        public double Level { get; private set; }

        public SummaGoal(Ability topic, double level, string name, byte tier = 0, uint? dueDate = null)
        {
            Topic = topic;
            Tier = tier;
            DueDate = dueDate;
            Level = level;

            _name = name;
            _isArt = MagicArts.IsArt(topic);

            _writingAbilityCondition = new AbilityScoreCondition(Abilities.ArtesLiberales, 1, 0, (byte)(tier + 1), dueDate == null ? null : dueDate - 3);
            _languageAbilityCondition = new AbilityScoreCondition(Abilities.Latin, 5, 0, (byte)(tier + 1), dueDate == null ? null : dueDate - 2);
            double scoreNeeded = level * 2.0;
            if(scoreNeeded < 2)
            {
                scoreNeeded = 2;
            }
            _abilityScoreCondition = new AbilityScoreCondition(topic, scoreNeeded, 0, (byte)(tier + 1), dueDate == null ? null : dueDate - 1);
        }

        public bool IsComplete(Character character)
        {
            return character.HasWrittenBookWithTitle(_name);
        }

        public bool DecrementDueDate()
        {
            if (DueDate != null)
            {
                return
                    _abilityScoreCondition.DecrementDueDate() &&
                    _languageAbilityCondition.DecrementDueDate() &&
                    _writingAbilityCondition.DecrementDueDate() &&
                    --DueDate != 0;
            }
            return true;
        }

        public void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log)
        {
            double desire = CalculateDesire(character);
            bool alDone = _writingAbilityCondition.IsComplete(character);
            bool latinDone = _languageAbilityCondition.IsComplete(character);
            bool abilityDone = _abilityScoreCondition.IsComplete(character);
            if (!alDone)
            {
                _writingAbilityCondition.Desire = desire;
                _writingAbilityCondition.ModifyActionList(character, alreadyConsidered, log);
            }
            if (!latinDone)
            {
                _languageAbilityCondition.Desire = desire;
                _languageAbilityCondition.ModifyActionList(character, alreadyConsidered, log);
            }
            if (!abilityDone)
            {
                _abilityScoreCondition.Desire = desire;
                _abilityScoreCondition.ModifyActionList(character, alreadyConsidered, log);
            }
            if (abilityDone && latinDone && alDone)
            {
                alreadyConsidered.Add(new Writing(Topic, _name, Abilities.Latin, Level, desire));
            }
        }

        private double CalculateDesire(Character character)
        {
            double quality = character.GetAttribute(AttributeType.Communication).Value + 6;
            // assuming someone half as skilled as the book provides
            double expValue = Level * (Level + 1) / 4.0;
            double bookSeasons = expValue / quality;
            double value =  character.RateSeasonalExperienceGain(Topic, quality) * bookSeasons;
            double seasons = Level / (character.GetAttribute(AttributeType.Communication).Value + character.GetAbility(Abilities.Latin).Value);
            if (!_isArt)
            {
                seasons *= 5;
            }
            return value / seasons;
        }

        public void ModifyVisNeeds(Character character, VisDesire[] desires)
        {
            _abilityScoreCondition.ModifyVisNeeds(character, desires);
        }

        public IList<BookDesire> GetBookNeeds(Character character)
        {
            if(IsComplete(character))
            {
                return null;
            }
            return _abilityScoreCondition.GetBookNeeds(character);
        }
    }

    class LabScoreGoal : IGoal
    {
        private HasLabCondition _hasLabCondition;
        private CharacteristicAbilityScoreCondition _attributeAbilityScore;
        private double _total;
        Activity _labWorkType;
        private ArtPair _artPair;

        public uint? DueDate { get; private set; }
        public byte Tier { get; private set; }
        public double Desire { get; set; }

        public LabScoreGoal(ArtPair artPair, Activity labWorkType, double total, double desire, byte tier, uint? dueDate = null)
        {
            _hasLabCondition = new HasLabCondition(desire, tier, dueDate);
            List<AttributeType> attributes = new List<AttributeType>();
            List<Ability> abilities = new List<Ability>();
            abilities.Add(artPair.Technique);
            abilities.Add(artPair.Form);
            abilities.Add(Abilities.MagicTheory);
            attributes.Add(AttributeType.Intelligence);
            _attributeAbilityScore = new CharacteristicAbilityScoreCondition(abilities, attributes, total, desire, (byte)(tier + 1), dueDate);
            DueDate = dueDate;
            Tier = tier;
            Desire = desire;
            _total = total;
            _labWorkType = labWorkType;
            _artPair = artPair;
        }

        public bool DecrementDueDate()
        {
            if (DueDate != null)
            {
                if (!_hasLabCondition.DecrementDueDate() || !_attributeAbilityScore.DecrementDueDate()) return false;
                DueDate--;
                return DueDate != 0;
            }
            return true;
        }

        public void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log)
        {
            Magus mage = (Magus)character;
            double remainingTotal = _total - mage.GetLabTotal(_artPair, _labWorkType);
            double dueDateDesire = Desire / (Tier + 1);
            if (DueDate != null)
            {
                if (DueDate == 0)
                {
                    log.Add("Ability Condition failed!");
                    return;
                }
                dueDateDesire /= (double)DueDate;
            }

            // consider aura search
            HandleAuraSearch(mage, alreadyConsidered, log, remainingTotal, dueDateDesire);

            if (!_hasLabCondition.IsComplete(character))
            {
                _hasLabCondition.ModifyActionList(character, alreadyConsidered, log);
            }
            else if(!_attributeAbilityScore.IsComplete(character))
            {
                _attributeAbilityScore.ModifyActionList(character, alreadyConsidered, log);
            }

            // TODO: consider laboratory refinements
            // TODO: consider taking an apprentice
            // TODO: consider finding a familiar
        }

        private void HandleAuraSearch(Magus mage, ConsideredActions alreadyConsidered, IList<string> log, double remainingTotal, double dueDateDesire)
        {
            double greatestAura = mage.KnownAuras.Select(a => a.Strength).Max();
            double currentAura = mage.Covenant.Aura.Strength;
            if (greatestAura > currentAura)
            {
                Aura bestAura = mage.KnownAuras.Where(a => a.Strength == greatestAura).First();
                if (!_hasLabCondition.IsComplete(mage))
                {
                    // just move the covenant right now
                    log.Add("Since no lab built, moving to better aura.");
                    mage.FoundCovenant(bestAura);
                }
                else
                {
                    // how do we want to rate the value of moving and having to build a new lab?
                    // it seems like basically the same as any other single-month activity
                    double gain = greatestAura - currentAura;
                    double desire = dueDateDesire * gain / remainingTotal;
                    log.Add("Moving to new aura (to boost lab total) worth " + dueDateDesire.ToString("0.00"));
                    alreadyConsidered.Add(new BuildLaboratory(bestAura, Abilities.MagicTheory, desire));
                }
            }
            else
            {
                // consider finding a new aura
                IncreaseAuraHelper helper = new IncreaseAuraHelper(Desire / remainingTotal, (byte)(Tier + 1), DueDate == null ? null : DueDate - 1);
                helper.ModifyActionList(mage, alreadyConsidered, log);
            }
        }

        public bool IsComplete(Character character)
        {
            if (character.GetType() != typeof(Magus))
            {
                throw new ArgumentException("Only magi have lab totals!");
            }
            Magus mage = (Magus)character;
            return _hasLabCondition.IsComplete(character) && mage.GetLabTotal(_artPair, _labWorkType) >= _total;
        }

        public void ModifyVisNeeds(Character character, VisDesire[] desires)
        {
            _hasLabCondition.ModifyVisNeeds(character, desires);
            _attributeAbilityScore.ModifyVisNeeds(character, desires);
        }

        public IList<BookDesire> GetBookNeeds(Character character)
        {
            List<BookDesire> bookDesires = new List<BookDesire>();
            bookDesires.AddRange(_hasLabCondition.GetBookNeeds(character));
            bookDesires.AddRange(_attributeAbilityScore.GetBookNeeds(character));
            return bookDesires;
        }
    }

    class HasCovenantCondition : IGoal
    {
        private CharacteristicAbilityScoreCondition _minScore;
        private double _desire;

        public uint? DueDate { get; private set; }
        public byte Tier { get; private set; }
        public double Desire 
        {
            get
            {
                return _desire;
            }
            set
            {
                _desire = value;
                _minScore.Desire = value;
            }
        }

        public bool DecrementDueDate()
        {
            if (DueDate != null)
            {
                if (!_minScore.DecrementDueDate()) return false;
                DueDate--;
                return DueDate != 0;
            }
            return true;
        }

        public void ModifyVisNeeds(Character character, VisDesire[] desires)
        {
            if (!_minScore.IsComplete(character))
            {
                _minScore.ModifyVisNeeds(character, desires);
            }
        }

        public IList<BookDesire> GetBookNeeds(Character character)
        {
            if (!_minScore.IsComplete(character))
            {
                return _minScore.GetBookNeeds(character);
            }
            return null;
        }

        public HasCovenantCondition(double value, byte tier, uint? dueDate = null)
        {
            _desire = value;
            DueDate = dueDate;
            Tier = tier;
            List<Ability> abilities = new List<Ability>();
            abilities.Add(Abilities.AreaLore);
            abilities.Add(MagicArts.Intellego);
            abilities.Add(MagicArts.Vim);
            List<AttributeType> attributes = new List<AttributeType>();
            attributes.Add(AttributeType.Perception);
            _minScore = new CharacteristicAbilityScoreCondition(abilities, attributes, 2, value, (byte)(tier + 1), dueDate == null ? null : dueDate - 1);
        }

        public void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log)
        {
            double dueDateDesire = Desire / (Tier + 1);
            if (DueDate != null)
            {
                if (DueDate == 0)
                {
                    log.Add("Has Covenant Condition failed");
                    return;
                }
                dueDateDesire /= (double)DueDate;
            }

            if (!_minScore.IsComplete(character) && dueDateDesire > 0.01)
            {
                _minScore.ModifyActionList(character, alreadyConsidered, log);
            }
        }

        public bool IsComplete(Character character)
        {
            return ((Magus)character).Covenant != null;
        }
    }

    class HasLabCondition : IGoal
    {
        private HasCovenantCondition _hasCovenant;
        private AbilityScoreCondition _mtCondition;
        private double _desire;

        public uint? DueDate { get; private set; }
        public byte Tier { get; private set; }
        public double Desire 
        {
            get
            {
                return _desire;
            }
            set
            {
                _desire = value;
                _hasCovenant.Desire = value;
                _mtCondition.Desire = value;
            }
        }

        public bool DecrementDueDate()
        {
            if (DueDate != null)
            {
                if(!_hasCovenant.DecrementDueDate()) return false;
                if(!_mtCondition.DecrementDueDate()) return false;
                DueDate--;
                return DueDate != 0;
            }
            return true;
        }

        public void ModifyVisNeeds(Character character, VisDesire[] desires) 
        {
            if (!_hasCovenant.IsComplete(character))
            {
                _hasCovenant.ModifyVisNeeds(character, desires);
            }
            if (!_mtCondition.IsComplete(character))
            {
                _mtCondition.ModifyVisNeeds(character, desires);
            }
        }

        public IList<BookDesire> GetBookNeeds(Character character)
        {
            if(!IsComplete(character))
            {
                List<BookDesire> bookDesires = new List<BookDesire>();
                IList<BookDesire> bookNeeds;
                if (!_hasCovenant.IsComplete(character))
                {
                    bookNeeds = _hasCovenant.GetBookNeeds(character);
                    if (bookNeeds != null)
                    {
                        bookDesires.AddRange(bookNeeds);
                    }
                }
                if (!_mtCondition.IsComplete(character))
                {
                    bookNeeds = _mtCondition.GetBookNeeds(character);
                    if (bookNeeds != null)
                    {
                        bookDesires.AddRange(bookNeeds);
                    }
                }
                return bookDesires;
            }
            return null;
        }

        public HasLabCondition(double value, byte tier, uint? dueDate = null)
        {
            _desire = value;
            DueDate = dueDate;
            Tier = tier;
            _hasCovenant = new HasCovenantCondition(value, tier, dueDate == null ? null : dueDate - 2);
            _mtCondition = new AbilityScoreCondition(Abilities.MagicTheory, 3, value, tier, dueDate == null ? null : dueDate - 1);
        }

        public void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log)
        {
            double dueDateDesire = Desire / (Tier + 1);
            if (DueDate != null)
            {
                if (DueDate == 0)
                {
                    log.Add("Not enough time to build lab");
                    return;
                }
                dueDateDesire /= (double)DueDate;
            }
            if (dueDateDesire > 0.01)
            {
                bool hasCovenant = _hasCovenant.IsComplete(character);
                bool hasMT = _mtCondition.IsComplete(character);
                if (!hasCovenant)
                {
                    _hasCovenant.ModifyActionList(character, alreadyConsidered, log);
                }
                if (!hasMT)
                {
                    _mtCondition.ModifyActionList(character, alreadyConsidered, log);
                }
                if (hasCovenant && hasMT)
                {
                    log.Add("Setting up a lab worth " + (dueDateDesire).ToString("0.00"));
                    alreadyConsidered.Add(new BuildLaboratory(Abilities.MagicTheory, dueDateDesire));
                }
            }
        }

        public bool IsComplete(Character character)
        {
            return ((Magus)character).Laboratory != null;
        }
    }

    class VisCondition : IGoal
    {
        HasLabCondition _hasLab;

        List<Ability> _visTypes;
        List<Ability> _extractAbilities;
        double _total;
        bool _isVimSufficient;

        public uint? DueDate { get; private set; }
        public byte Tier { get; private set; }
        public double Desire { get; set; }

        public VisCondition(List<Ability> visTypes, double total, double desire, byte tier, uint? dueDate = null)
        {
            DueDate = dueDate;
            Tier = tier;
            Desire = desire;

            _extractAbilities = new List<Ability>();
            _extractAbilities.Add(MagicArts.Creo);
            _extractAbilities.Add(MagicArts.Vim);
            _extractAbilities.Add(Abilities.MagicTheory);
            _visTypes = visTypes;
            _total = total;
            _isVimSufficient = visTypes.Where(v => v == MagicArts.Vim).Any();

            _hasLab = new HasLabCondition(desire, tier, dueDate == null || dueDate < 2 ? null : dueDate - 1);
        }

        public bool DecrementDueDate()
        {
            if (DueDate != null)
            {
                if(!_hasLab.DecrementDueDate()) return false;
                DueDate--;
                return DueDate != 0;
            }
            return true;
        }

        public void ModifyVisNeeds(Character character, VisDesire[] desires)
        {
            if (!_hasLab.IsComplete(character))
            {
                _hasLab.ModifyVisNeeds(character, desires);
            }
            foreach (Ability ability in _visTypes)
            {
                desires[ability.AbilityId % 300].Quantity += _total;
            }
        }

        public IList<BookDesire> GetBookNeeds(Character character)
        {
            List<BookDesire> bookDesires = new List<BookDesire>();
            if (!_hasLab.IsComplete(character))
            {
                var bookNeeds = _hasLab.GetBookNeeds(character);
                if (bookNeeds != null)
                {
                    bookDesires.AddRange(bookNeeds);
                }
            }
            if (_isVimSufficient && !IsComplete(character))
            {
                foreach (Ability ability in _extractAbilities)
                {
                    bookDesires.Add(new BookDesire(ability, character.GetAbility(ability).Value));
                }
            }
            return bookDesires;
        }

        public void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log)
        {
            double dueDateDesire = Desire / (Tier + 1);
            if (DueDate != null)
            {
                if (DueDate == 0)
                {
                    log.Add("Vis Goal Failed");
                    return;
                }
                dueDateDesire /= (double)DueDate;
            }
            if (dueDateDesire > 0.01)
            {
                Magus mage = (Magus)character;
                bool hasLab = _hasLab.IsComplete(character);

                if (_isVimSufficient)
                {
                    // determine value of extracting vis from aura

                    if (!hasLab)
                    {
                        // if we don't have a lab, we need a lab
                        _hasLab.ModifyActionList(character, alreadyConsidered, log);
                    }
                    else
                    {
                        // Now we need to consider both the value of starting to distill vis now
                        // and the value of instead learning more before doing so.
                        double labTotal = mage.GetLabTotal(MagicArtPairs.CrVi, Activity.DistillVis);
                        double currentRate = labTotal / 10;
                        if (currentRate >= _total)
                        {
                            // we can get what we want in one season, go ahead and do it
                            log.Add("Extracting vis worth " + (dueDateDesire).ToString("0.00"));
                            alreadyConsidered.Add(new VisExtracting(Abilities.MagicTheory, dueDateDesire));
                        }
                        else
                        {
                            // we are in the multi-season-to-fulfill scenario
                            double desire = currentRate * dueDateDesire / _total;
                            log.Add("Extracting vis worth " + (desire).ToString("0.00"));
                            alreadyConsidered.Add(new VisExtracting(Abilities.MagicTheory, desire));

                            // the difference between the desire of starting now
                            // and the desire of starting after gaining experience
                            // is the effective value of practicing here

                            // TODO: this should really be a lab total increse, yes?
                            IncreaseAbilitiesForVisHelper helper = new IncreaseAbilitiesForVisHelper(_extractAbilities, Desire, labTotal, (byte)(Tier + 1), DueDate - 1);
                            helper.ModifyActionList(character, alreadyConsidered, log);
                        }
                    }
                }
                // determine current rate of automatic accrual of vis of the desired type
                double annualGain =
                    mage.KnownAuras.SelectMany(a => a.VisSources).Where(v => _visTypes.Contains(v.Art)).Select(v => v.AnnualAmount).Sum();
                if (annualGain == 0 || (DueDate != null && (annualGain * DueDate / 4) < _total))
                {
                    List<Ability> visSearchAbilities = new List<Ability>();
                    visSearchAbilities.Add(Abilities.MagicLore);
                    visSearchAbilities.Add(MagicArts.Intellego);
                    visSearchAbilities.Add(MagicArts.Vim);
                    // we're not getting vis fast enough, so we need to find a new source
                    // consider the value of searching for new vis sites in current auras
                    // determine average vis source found
                    double magicLore = mage.GetAbility(Abilities.MagicLore).Value;
                    magicLore += mage.GetAttribute(AttributeType.Perception).Value;
                    magicLore += mage.GetCastingTotal(MagicArtPairs.InVi) / 5;
                    if (magicLore > 0)
                    {
                        foreach (Aura aura in mage.KnownAuras)
                        {
                            double averageFind = aura.GetAverageVisSourceSize(magicLore);
                            if (averageFind > 0)
                            {
                                // modify by chance vis will be of the proper type
                                double desire = (averageFind * _visTypes.Count() / 15);

                                // TODO: modify by lifelong value of source?
                                log.Add("Looking for vis source worth " + (desire * dueDateDesire).ToString("0.00"));
                                alreadyConsidered.Add(new FindVisSource(aura, Abilities.MagicLore, desire * dueDateDesire));
                            }
                            // consider the value of getting better at the vis search skills first
                            double currentVis = aura.VisSources.Select(v => v.AnnualAmount).Sum();
                            IncreaseAbilitiesForVisSearchHelper helper =
                                new IncreaseAbilitiesForVisSearchHelper(Desire, currentVis, magicLore, aura.Strength, (byte)(Tier + 1), DueDate == null ? null : DueDate - 1);
                            helper.ModifyActionList(character, alreadyConsidered, log);
                        }
                    }
                    else
                    {
                        // consider increasing skills to be able to find vis
                        List<Ability> visSourceAbilities = new List<Ability>();
                        visSourceAbilities.Add(MagicArts.Intellego);
                        visSourceAbilities.Add(MagicArts.Vim);
                        visSourceAbilities.Add(Abilities.MagicLore);
                        AbilityScoreCondition condition =
                            new AbilityScoreCondition(visSourceAbilities, 2, Desire, (byte)(Tier + 1), DueDate == null ? null : DueDate - 1);
                        condition.ModifyActionList(character, alreadyConsidered, log);
                    }
                    // consider the value of looking for a new aura
                    double areaLore = mage.GetAbility(Abilities.AreaLore).Value;
                    areaLore += mage.GetAttribute(AttributeType.Perception).Value;
                    areaLore += mage.GetCastingTotal(MagicArtPairs.InVi) / 10;
                    if (areaLore > 0 && magicLore > 0)
                    {
                        double auraFound = mage.GetAverageAuraFound();
                        double multiplier = Math.Sqrt(magicLore * auraFound) * 2 / 3;
                        double areaUnder = 11.180339887498948482045868343656 * multiplier;
                        double visFromAura = areaUnder * _visTypes.Count() / 75;
                        dueDateDesire = dueDateDesire * visFromAura * (Tier + 1) / (Tier + 2);
                        log.Add("Looking for aura (to find a vis source in) worth " + (dueDateDesire).ToString("0.00"));
                        alreadyConsidered.Add(new FindAura(Abilities.AreaLore, dueDateDesire));
                    }
                }
            }
        }

        public bool IsComplete(Character character)
        {
            if (character.GetType() != typeof(Magus))
            {
                throw new ArgumentException("Only magi can have Vis conditions");
            }
            Magus mage = (Magus)character;
            double total = 0;
            foreach (Ability visType in _visTypes)
            {
                total += mage.GetVisCount(visType);
            }
            return total >= _total;
        }
    }

    class InventSpellGoal : IGoal
    {
        private LabScoreGoal _labScoreGoal;
        private Spell _spell;

        public uint? DueDate { get; private set; }
        public byte Tier { get; private set; }
        public double Desire { get; set; }

        public bool DecrementDueDate()
        {
            if (DueDate != null)
            {
                if(!_labScoreGoal.DecrementDueDate()) return false;
                DueDate--;
                return DueDate != 0;
            }
            return true;
        }

        public InventSpellGoal(Spell spell, double desire, byte tier, uint? dueDate = null)
        {
            DueDate = dueDate;
            Tier = tier;
            Desire = desire;
            _spell = spell;
            List<AttributeType> attributes = new List<AttributeType>();
            attributes.Add(AttributeType.Intelligence);

            // we need at least a lab score above the level of the spell to think about inventing it
            uint? labScoreDueDate = null;
            if (dueDate != null && dueDate > spell.Level)
            {
                labScoreDueDate = dueDate - (uint)spell.Level;
            }
            else
            {
                // TODO: throw error?
            }
            _labScoreGoal = new LabScoreGoal(spell.BaseArts, Activity.InventSpells, spell.Level, desire, tier, labScoreDueDate);
        }

        public void ModifyVisNeeds(Character character, VisDesire[] desires)
        {
            _labScoreGoal.ModifyVisNeeds(character, desires);
        }

        public IList<BookDesire> GetBookNeeds(Character character)
        {
            return _labScoreGoal.GetBookNeeds(character);
        }

        public void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log)
        {
            double dueDateDesire = Desire / (Tier + 1);
            if (DueDate != null)
            {
                if (DueDate == 0)
                {
                    log.Add("Not enough time to invent spell");
                    return;
                }
                dueDateDesire /= (double)DueDate;
            }
            if (dueDateDesire > 0.01)
            {
                if (!_labScoreGoal.IsComplete(character))
                {
                    _labScoreGoal.ModifyActionList(character, alreadyConsidered, log);
                }
                else
                {
                    double level = _spell.Level;
                    // Now we need to consider both the value of starting to invent the spell now
                    // and the value of instead learning more before doing so.
                    // As a first pass, value the portion of the spell inventable this season
                    Magus mage = (Magus)character;
                    double extraTotal = mage.GetLabTotal(_spell.BaseArts, Activity.InventSpells) - level;
                    if (extraTotal <= 0)
                    {
                        // This should not happen
                        _labScoreGoal.ModifyActionList(character, alreadyConsidered, log);
                    }
                    if (extraTotal >= level)
                    {
                        // there's no reason to consider adding to abilities for this spell
                        // TODO: eventually, we should consider the fact that the extra learning
                        // could allow one to learn more spells in a season
                        log.Add("Inventing this spell worth " + dueDateDesire.ToString("0.00"));
                        alreadyConsidered.Add(new InventSpell(_spell, Abilities.MagicTheory, dueDateDesire));
                    }
                    else
                    {
                        // we are in the multi-season-to-invent scenario
                        dueDateDesire = extraTotal * dueDateDesire / level;
                        alreadyConsidered.Add(new InventSpell(_spell, Abilities.MagicTheory, dueDateDesire));

                        // the difference between the desire of starting now
                        // and the desire of starting after practice
                        // is the effective value of practicing here
                        IncreaseLabTotalVersusEffectLevelHelper helper = new IncreaseLabTotalVersusEffectLevelHelper(_spell.BaseArts, dueDateDesire, extraTotal, level, (byte)(Tier + 1));
                        helper.ModifyActionList(character, alreadyConsidered, log);
                    }
                }
            }
        }

        public bool IsComplete(Character character)
        {
            return ((Magus)character).SpellList.Where(s => s.Name == _spell.Name).Any();
        }
    }

    class LongevityRitualGoal : IGoal
    {
        private HasLabCondition _hasLabCondition;
        private List<Ability> _abilitiesRequired;
        private List<Ability> _artsRequired;

        public uint? DueDate { get; private set; }
        public byte Tier { get; private set; }
        public double Desire { get; set; }

        public bool DecrementDueDate()
        {
            if (DueDate != null)
            {
                if(!_hasLabCondition.DecrementDueDate()) return false;
                DueDate--;
                return DueDate != 0;
            }
            return true;
        }

        public LongevityRitualGoal(Magus mage, byte tier = 0, uint? dueDate = null)
        {
            DueDate = dueDate;
            Tier = tier;
            Desire = CalculateDesire(mage);
            _abilitiesRequired = new List<Ability>();
            _abilitiesRequired.Add(MagicArts.Creo);
            _abilitiesRequired.Add(MagicArts.Vim);
            _abilitiesRequired.Add(Abilities.MagicTheory);
            _artsRequired = new List<Ability>();
            _artsRequired.Add(MagicArts.Creo);
            _artsRequired.Add(MagicArts.Vim);
            List<AttributeType> attributes = new List<AttributeType>();
            attributes.Add(AttributeType.Intelligence);

            // we need a lab to create a longevity ritual
            _hasLabCondition = new HasLabCondition(Desire, tier, dueDate == null || dueDate <= 3 ? null : dueDate - 3);
        }

        private double CalculateDesire(Magus mage)
        {
            // the number of years added to life is a baseline
            double lrLabTotal = mage.GetLabTotal(MagicArtPairs.CrVi, Activity.LongevityRitual);
            double dvLabTotal = mage.GetLabTotal(MagicArtPairs.CrVi, Activity.DistillVis);
            return (lrLabTotal * dvLabTotal * 4.0 / 50.0) - (mage.SeasonalAge / 20.0);

            // TODO: the warping probably ought to reduce value
        }

        public void ModifyVisNeeds(Character character, VisDesire[] desires)
        {
            double visNeed = character.SeasonalAge / 20.0;
            VisCondition visCondition = new VisCondition(_artsRequired, visNeed, Desire, Tier, DueDate == null ? null : DueDate - 1);
            visCondition.ModifyVisNeeds(character, desires);
        }

        public IList<BookDesire> GetBookNeeds(Character character)
        {
            if(!IsComplete(character))
            {
                List<BookDesire> booksDesired = new List<BookDesire>();
                var bookNeeds = _hasLabCondition.GetBookNeeds(character);
                if (bookNeeds != null)
                {
                    booksDesired.AddRange(bookNeeds);
                }
                double visNeed = character.SeasonalAge / 20.0;
                VisCondition visCondition = new VisCondition(_artsRequired, visNeed, Desire, Tier, DueDate == null ? null : DueDate - 1);
                if (!visCondition.IsComplete(character))
                {
                    bookNeeds = visCondition.GetBookNeeds(character);
                    if (bookNeeds != null)
                    {
                        booksDesired.AddRange(bookNeeds);
                    }
                }
                return booksDesired;
            }
            return null;
        }

        public void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log)
        {
            if (character.GetType() == typeof(Magus))
            {
                Magus mage = (Magus)character;
                Desire = CalculateDesire(mage);
                double visNeed = character.SeasonalAge / 20.0;
                VisCondition visCondition = new VisCondition(_artsRequired, visNeed, Desire, Tier, DueDate == null ? null : DueDate - 1);

                bool visComplete = visCondition.IsComplete(character);
                bool labComplete = _hasLabCondition.IsComplete(character);

                if (!visComplete)
                {
                    visCondition.ModifyActionList(character, alreadyConsidered, log);
                }
                if (!labComplete)
                {
                    _hasLabCondition.Desire = Desire;
                    _hasLabCondition.ModifyActionList(character, alreadyConsidered, log);
                }
                if (visComplete && labComplete)
                {
                    //effectively, every five points of lab total is worth a decade of effectiveness
                    double dueDateDesire = Desire / (Tier + 1);
                    if (DueDate != null)
                    {
                        if (DueDate == 0)
                        {
                            log.Add("Not enough time to perform longevity ritual");
                            return;
                        }
                        dueDateDesire /= (double)DueDate;
                    }
                    if (DueDate == null || DueDate < 4)
                    {
                        log.Add("Performing longevity ritual worth " + dueDateDesire.ToString("0.00"));
                        alreadyConsidered.Add(new LongevityRitual(Abilities.MagicTheory, dueDateDesire));
                    }
                    double labTotalDesire = mage.GetLabTotal(MagicArtPairs.CrVi, Activity.DistillVis) * 8;
                    if (DueDate != null)
                    {
                        labTotalDesire /= (double)DueDate;
                    }
                    // every point of lab total is effectively two years
                    LongevityRitualAbilitiesHelper helper = 
                        new LongevityRitualAbilitiesHelper(labTotalDesire, (byte)(Tier + 1), DueDate == null ? null : DueDate - 1);
                    helper.ModifyActionList(character, alreadyConsidered, log);
                }
            }
        }

        public bool IsComplete(Character character)
        {
            return character.LongevityRitual > 0;
        }
    }

    class HasApprenticeCondition : IGoal
    {
        #region Private Fields
        AbilityScoreCondition[] _artRequirements;
        #endregion
        public uint? DueDate { get; private set; }
        public byte Tier { get; private set; }
        public double Desire { get; set; }

        public bool DecrementDueDate()
        {
            if (DueDate != null)
            {
                foreach (AbilityScoreCondition condition in _artRequirements)
                {
                    if (!condition.DecrementDueDate())
                    {
                        return false;
                    }
                }
                DueDate--;
                return DueDate != 0;
            }
            return true;
        }

        public void ModifyVisNeeds(Character character, VisDesire[] desires)
        {
            if (!IsComplete(character))
            {
                foreach (AbilityScoreCondition artCondition in _artRequirements)
                {
                    artCondition.ModifyVisNeeds(character, desires);
                }
            }
        }

        public IList<BookDesire> GetBookNeeds(Character character)
        {
            if (!IsComplete(character))
            {
                List<BookDesire> bookDesires = new List<BookDesire>();
                foreach (AbilityScoreCondition artCondition in _artRequirements)
                {
                    var bookNeeds = artCondition.GetBookNeeds(character);
                    if (bookNeeds != null)
                    {
                        bookDesires.AddRange(bookNeeds);
                    }
                }
                return bookDesires;
            }
            return null;
        }

        public HasApprenticeCondition(double desire = 1, byte tier = 0, uint? dueDate = null)
        {
            DueDate = dueDate;
            Tier = tier;
            Desire = desire;

            // TODO: remove the magic number
            _artRequirements = new AbilityScoreCondition[15];
            uint? modifiedDueDate = dueDate == null || dueDate < 3 ? null : dueDate - 2;
            byte nextTier = (byte)(tier + 1);
            _artRequirements[0] = new AbilityScoreCondition(MagicArts.Creo, 5, Desire,
                nextTier, modifiedDueDate);
            _artRequirements[1] = new AbilityScoreCondition(MagicArts.Intellego, 5, Desire,
                nextTier, modifiedDueDate);
            _artRequirements[2] = new AbilityScoreCondition(MagicArts.Muto, 5, Desire,
                nextTier, modifiedDueDate);
            _artRequirements[3] = new AbilityScoreCondition(MagicArts.Perdo, 5, Desire,
                nextTier, modifiedDueDate);
            _artRequirements[4] = new AbilityScoreCondition(MagicArts.Rego, 5, Desire,
                nextTier, modifiedDueDate);
            _artRequirements[5] = new AbilityScoreCondition(MagicArts.Animal, 5, Desire,
                nextTier, modifiedDueDate);
            _artRequirements[6] = new AbilityScoreCondition(MagicArts.Aquam, 5, Desire,
                nextTier, modifiedDueDate);
            _artRequirements[7] = new AbilityScoreCondition(MagicArts.Auram, 5, Desire,
                nextTier, modifiedDueDate);
            _artRequirements[8] = new AbilityScoreCondition(MagicArts.Corpus, 5, Desire,
                nextTier, modifiedDueDate);
            _artRequirements[9] = new AbilityScoreCondition(MagicArts.Herbam, 5, Desire,
                nextTier, modifiedDueDate);
            _artRequirements[10] = new AbilityScoreCondition(MagicArts.Ignem, 5, Desire,
                nextTier, modifiedDueDate);
            _artRequirements[11] = new AbilityScoreCondition(MagicArts.Imaginem, 5, Desire,
                nextTier, modifiedDueDate);
            _artRequirements[12] = new AbilityScoreCondition(MagicArts.Mentem, 5, Desire,
                nextTier, modifiedDueDate);
            _artRequirements[13] = new AbilityScoreCondition(MagicArts.Terram, 5, Desire,
                nextTier, modifiedDueDate);
            _artRequirements[14] = new AbilityScoreCondition(MagicArts.Vim, 5, Desire,
                nextTier, modifiedDueDate);
        }

        public void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log)
        {
            bool isReady = true;
            foreach (AbilityScoreCondition condition in _artRequirements)
            {
                if (!condition.IsComplete(character))
                {
                    isReady = false;
                    condition.ModifyActionList(character, alreadyConsidered, log);
                }
            }
            if (isReady)
            {
                double desire = Desire / (Tier + 1);
                if(DueDate != null)
                {
                    if (DueDate == 0)
                    {
                        log.Add("Not enough time to find an apprentice");
                        return;
                    }
                    desire /= (double)DueDate;
                }

                alreadyConsidered.Add(new FindApprentice(MagicArts.Intellego, desire));
            }
        }

        public bool IsComplete(Character character)
        {
            // TODO: there's nothing preventing a character from having multiple apprentices
            return character.GetType() == typeof(Magus) && ((Magus)character).Apprentice != null;
        }
    }

    class TeachingApprenticeGoal : BaseGoal
    {
        private Teach _teachAction;
        public IEnumerable<Character> Students { get; private set; }

        public TeachingApprenticeGoal(Character student, double desire, byte tier = 0, uint? dueDate = null) : base(desire, tier, dueDate)
        {
            var students = new List<Character>();
            students.Add(student);
            Students = students;
        }

        public override void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log)
        {
            double dueDateDesire = Desire / (double)(Tier + 1);
            if(DueDate != null)
            {
                if (DueDate > 0)
                {
                    dueDateDesire /= (double)DueDate;
                }
                else
                {
                    character.Log.Add("Behind schedule on teaching!");
                }
            }
            Magus mage = (Magus)character;
            // TODO: figure out what to teach
            Ability ability = Abilities.MagicTheory;
            double xpDiff = mage.GetAbility(ability).Experience - mage.Apprentice.GetAbility(ability).Experience;
            double quality = mage.GetAttributeValue(AttributeType.Communication) + mage.GetAbility(Abilities.Teaching).Value + 6.0;
            if (quality > xpDiff)
            {
                var arts = mage.GetAbilities().Where(a => MagicArts.IsArt(a.Ability)).OrderBy(a => a.Value);
                foreach (CharacterAbilityBase art in arts)
                {
                    xpDiff = art.Experience - mage.Apprentice.GetAbility(art.Ability).Experience;
                    if (quality <= xpDiff)
                    {
                        ability = art.Ability;
                        break;
                    }
                }
            }
            _teachAction = new Teach(Students.First(), ability, Abilities.Teaching, dueDateDesire);
            alreadyConsidered.Add(_teachAction);
        }

        public override bool IsComplete(Character character)
        {
            return  _teachAction != null && _teachAction.Completed;
        }

        public override void ModifyVisNeeds(Character character, VisDesire[] desires)
        {
        }

        public override IList<BookDesire> GetBookNeeds(Character character)
        {
            return null;
        }
    }
    #endregion
}
