using System;
using System.Collections.Generic;
using System.Linq;

using WizardMonks.Instances;

namespace WizardMonks
{
    public partial class Character
    {
        protected List<IGoal> _goals;
        IAction DecideSeasonalActivity()
        {
            ConsideredActions actions = new ConsideredActions();
            foreach (IGoal goal in _goals)
            {
                if (!goal.IsComplete(this))
                {
                    // TODO: it should probably be an error case for a goal to still be here
                    // for now, ignore
                    List<string> dummy = new List<string>();
                    goal.ModifyActionList(this, actions, dummy);
                    goal.DecrementDueDate();
                }
            }
            Log.AddRange(actions.Log());
            return actions.GetBestAction();
        }

        public virtual void ReprioritizeGoals()
        {
            throw new NotImplementedException();
        }
    }

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
            return ActionTypeMap.SelectMany(a => a.Value).Select(a => a.Log()).ToList();
        }
        
        public IAction GetBestAction()
        {
            return ActionTypeMap.SelectMany(a => a.Value).OrderByDescending(a => a.Desire).FirstOrDefault();
        }
    }

    public interface IGoal
    {
        void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log);
        bool IsComplete(Character character);
        uint? DueDate { get; }
        double Desire { get; }
        void DecrementDueDate();
    }

    class AbilityScoreCondition : IGoal
    {
        #region Protected fields
        protected List<Ability> _abilities;
        protected double _total;
        #endregion

        public uint? DueDate { get; private set; }
        public virtual double Desire { get; private set; }
        
        public void DecrementDueDate()
        {
            if (DueDate != null)
            {
                DueDate--;
            }
        }

        public AbilityScoreCondition(List<Ability> abilities, double total, double desire, uint? dueDate = null)
        {
            _abilities = abilities;
            _total = total;
            Desire = desire;
            DueDate = dueDate;
        }

        public virtual void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log)
        {
            double remainingTotal = GetRemainingTotal(character);
            double desire = Desire;
            if (DueDate != null)
            {
                desire /= (double)DueDate;
            }
            IEnumerable<IBook> readableBooks = character.ReadableBooks;
            foreach (Ability ability in _abilities)
            {
                // Handle Reading
                CharacterAbilityBase charAbility = character.GetAbility(ability);
                var topicalBooks = readableBooks.Where(b => b.Topic == ability);
                AddReading(character, alreadyConsidered, topicalBooks, remainingTotal, desire);

                // Handle Practice
                // For now, assume 4pt practice on everything
                desire = desire * charAbility.GetValueGain(4) / remainingTotal;
                log.Add("Practicing " + ability.AbilityName + " worth " + desire.ToString("0.00"));
                Practice practiceAction = new Practice(ability, desire * charAbility.GetValueGain(4) / remainingTotal);
                alreadyConsidered.Add(practiceAction);

                if (character.GetType() == typeof(Magus))
                {
                    Magus mage = (Magus)character;
                    if (MagicArts.IsArt(charAbility.Ability))
                    {
                        HandleVisUse(mage, charAbility, remainingTotal, desire, alreadyConsidered, log);
                    }
                }

                // TODO: Learning By Training
                // TODO: Learning by Teaching
            }
        }

        private void HandleVisUse(Magus mage, CharacterAbilityBase charAbility, double remainingTotal, double desire, ConsideredActions alreadyConsidered, IList<string> log )
        {
            // see if the mage has enough vis of this type
            double stockpile = mage.GetVisCount(charAbility.Ability);
            double visNeed = 0.5 + (charAbility.Value / 10.0);
            double baseDesire = desire * charAbility.GetValueGain(6) / remainingTotal;
            // if so, assume vis will return an average of 6XP
            if (stockpile > visNeed)
            {
                log.Add("Studying vis for " + charAbility.Ability.AbilityName + " worth " + baseDesire.ToString("0.00"));
                VisStudying visStudy = new VisStudying(charAbility.Ability, baseDesire);
                alreadyConsidered.Add(visStudy);
                // TODO: how do we decrement the cost of the vis?
            }
            else
            {
                List<Ability> visType = new List<Ability>();
                visType.Add(charAbility.Ability);
                log.Add("Getting vis to study " + charAbility.Ability.AbilityName + " worth " + (baseDesire / 2).ToString("0.00"));
                VisCondition visCondition = new VisCondition(visType, visNeed - stockpile, baseDesire / 2, DueDate);
                visCondition.ModifyActionList(mage, alreadyConsidered, log);
            }
            // Handle the possibility of extracting vim vis
            if (charAbility.Ability == MagicArts.Vim)
            {
                baseDesire = desire * charAbility.GetValueGain(6);
                ushort additionalTime = 1;
                if (mage.Covenant == null)
                {
                    additionalTime++;
                }
                if (mage.Laboratory == null)
                {
                    additionalTime++;
                }
                if (mage.Covenant == null)
                {
                    log.Add("Finding an aura to set up a lab to extract vis to study " + charAbility.Ability.AbilityName + " worth " + (baseDesire / additionalTime).ToString("0.00"));
                    HasCovenantCondition covenantCondition = new HasCovenantCondition(baseDesire / additionalTime, DueDate);
                    covenantCondition.ModifyActionList(mage, alreadyConsidered, log);
                }
                else if (mage.Laboratory == null)
                {
                    log.Add("Setting up a lab to extract vis to study " + charAbility.Ability.AbilityName + " worth " + (baseDesire / additionalTime).ToString("0.00"));
                    HasLabCondition labCondition = new HasLabCondition(baseDesire / additionalTime, DueDate);
                    labCondition.ModifyActionList(mage, alreadyConsidered, log);
                }
                else
                {
                    log.Add("Extracting vis to study " + charAbility.Ability.AbilityName + " worth " + (baseDesire / additionalTime).ToString("0.00"));
                    List<Ability> vimVis = new List<Ability>();
                    double visNeeded = 0.5 + (charAbility.Value / 10.0);
                    vimVis.Add(MagicArts.Vim);
                    VisCondition visCondition = new VisCondition(vimVis, visNeeded, baseDesire / additionalTime, DueDate);
                    visCondition.ModifyActionList(mage, alreadyConsidered, log);
                }
                // TODO: consider the value of looking for another aura
                // TODO: consider the value of looking for vis sites
            }
            // TODO: expand vis study to 
        }

        public virtual bool IsComplete(Character character)
        {
            return GetRemainingTotal(character) <= 0;
        }

        protected virtual double GetRemainingTotal(Character character)
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

        public CharacteristicAbilityScoreCondition(List<Ability> abilities, List<AttributeType> attributes, double total, double desire, uint? dueDate = null) :
            base(abilities, total, desire, dueDate)
        {
            _attributes = attributes;
        }

        protected override double GetRemainingTotal(Character character)
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
            // TODO: consider CrVi spells that increase statistics
        }
    }

    class LabScoreGoal : CharacteristicAbilityScoreCondition
    {
        private HasLabCondition _hasLabCondition;

        public LabScoreGoal(List<Ability> abilities, List<AttributeType> attributes, double total, double desire, uint? dueDate = null)
            : base(abilities, attributes, total, desire, dueDate)
        {
            _hasLabCondition = new HasLabCondition(desire, dueDate);
        }

        protected override double GetRemainingTotal(Character character)
        {
            double currentTotal = 0;
            foreach (Ability ability in _abilities)
            {
                currentTotal += character.GetAbility(ability).Value;
            }
            foreach (AttributeType attribute in _attributes)
            {
                currentTotal += character.GetAttribute(attribute).Value;
            }
            if(character.GetType() == typeof(Magus))
            {
                Magus mage = (Magus)character;
                currentTotal += mage.Laboratory.Quality;
                foreach (Ability ability in _abilities)
                {
                    if (mage.Laboratory.ArtModifiers.ContainsKey(ability))
                    {
                        currentTotal += mage.Laboratory.ArtModifiers[ability];
                    }
                }
                // TODO: we probably need to take activity type into account, 
                // or that needs to be taken into account in setting the total of this object
            }
            return _total - currentTotal;
        }

        public override void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log)
        {
            if (!_hasLabCondition.IsComplete(character))
            {
                _hasLabCondition.ModifyActionList(character, alreadyConsidered, log);
            }
            else
            {
                base.ModifyActionList(character, alreadyConsidered, log);

                // TODO: consider aura search
                // TODO: consider laboratory building
                // TODO: consider laboratory refinements
                // TODO: consider taking an apprentice
                // TODO: consider finding a familiar
            }
        }

        public override bool IsComplete(Character character)
        {
            return _hasLabCondition.IsComplete(character) && GetRemainingTotal(character) <= 0;
        }
    }

    // TODO: consider a base class for goals with pre-reqs

    class HasCovenantCondition : IGoal
    {
        public uint? DueDate { get; private set; }
        public double Desire { get; private set; }

        public void DecrementDueDate()
        {
            if (DueDate != null)
            {
                DueDate--;
            }
        }

        public HasCovenantCondition(double value, uint? dueDate = null)
        {
            Desire = value;
            DueDate = dueDate;
        }

        public void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log)
        {
            double desire = Desire;
            if (DueDate != null)
            {
                desire /= (double)DueDate;
            }
            log.Add("Looking for an aura worth " + (desire).ToString("0.00"));
            alreadyConsidered.Add(new FindAura(Abilities.AreaLore, desire));
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

        public uint? DueDate { get; private set; }
        public double Desire { get; private set; }

        public void DecrementDueDate()
        {
            if (DueDate != null)
            {
                DueDate--;
            }
        }

        public HasLabCondition(double value, uint? dueDate = null)
        {
            Desire = value;
            DueDate = dueDate;
            List<Ability> mt = new List<Ability>();
            mt.Add(Abilities.MagicTheory);
            _hasCovenant = new HasCovenantCondition(value, dueDate - 2);
            _mtCondition = new AbilityScoreCondition(mt, 3, value, dueDate - 1);
        }

        public void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log)
        {
            double desire = Desire;
            if (DueDate != null)
            {
                desire /= (double)DueDate;
            }
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
            if(hasCovenant && hasMT)
            {
                log.Add("Setting up a lab worth " + (desire).ToString("0.00"));
                alreadyConsidered.Add(new BuildLaboratory(Abilities.MagicTheory, desire));
            }
        }

        public bool IsComplete(Character character)
        {
            return ((Magus)character).Laboratory != null;
        }
    }

    class VisCondition : IGoal
    {
        public uint? DueDate { get; private set; }
        public double Desire { get; private set; }

        public void DecrementDueDate()
        {
            if (DueDate != null)
            {
                DueDate--;
            }
        }

        List<Ability> _visTypes;
        List<Ability> _extractArts;
        double _total;
        HasLabCondition _hasLab;
        readonly ArtPair _pair = MagicArtPairs.CrVi;
        bool _isVimSufficient;
        bool _isFormSufficient;

        public VisCondition(List<Ability> visTypes, double total, double desire, uint? dueDate = null)
        {
            _extractArts = new List<Ability>();
            _extractArts.Add(MagicArts.Creo);
            _extractArts.Add(MagicArts.Vim);
            _extractArts.Add(Abilities.MagicTheory);
            _visTypes = visTypes;
            _total = total;
            Desire = desire;
            DueDate = dueDate;
            _isVimSufficient = visTypes.Where(v => v == MagicArts.Vim).Any();
            _isFormSufficient = _isVimSufficient || !visTypes.Where(v => MagicArts.IsTechnique(v)).Any();
            _hasLab = new HasLabCondition(desire, dueDate);
        }

        public void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log)
        {
            Magus mage = (Magus)character;
            double dueDateDesire = Desire;
            if (DueDate != null)
            {
                dueDateDesire /= (double)DueDate;
            }
            // we're doing the math a little oddly hear to try to use the LabGoalHelper we made from spells.
            // By applying the /10 of vis distillation to the denominator, it should work out
            double tempTotal = _total * 10;
            double currentRate = mage.GetLabTotal(_pair, Activity.DistillVis);
            if (!_isVimSufficient)
            {
                currentRate /= 2.0;
            }
            if (!_isFormSufficient)
            {
                currentRate /= 2.0;
            }

            if (!_hasLab.IsComplete(character))
            {
                // if we don't have a lab, we need a lab
                _hasLab.ModifyActionList(character, alreadyConsidered, log);

                // we should probably also consider the value-add of increasing CrVi, even if we don't have a lab yet
                // the difference between the desire of starting now
                // and the desire of starting after practice
                // is the effective value of practicing here
                IncreaseAbilityForLabGoalHelper helper = new IncreaseAbilityForLabGoalHelper(_extractArts, dueDateDesire, currentRate, tempTotal);
                helper.ModifyActionList(character, alreadyConsidered, log);
            }
            else
            {
                // Now we need to consider both the value of starting to distill vis now
                // and the value of instead learning more before doing so.
                
                if (currentRate >= tempTotal)
                {
                    // we can get what we want in one season, go ahead and do it
                    log.Add("Extracting vis worth " + (dueDateDesire).ToString("0.00"));
                    alreadyConsidered.Add(new VisExtracting(Abilities.MagicTheory, dueDateDesire));
                }
                else
                {
                    // we are in the multi-season-to-fulfill scenario
                    log.Add("Extracting vis worth " + (dueDateDesire).ToString("0.00"));
                    double desire = currentRate * dueDateDesire / (tempTotal);
                    alreadyConsidered.Add(new VisExtracting(Abilities.MagicTheory, desire));

                    // the difference between the desire of starting now
                    // and the desire of starting after practice
                    // is the effective value of practicing here
                    IncreaseAbilityForLabGoalHelper helper = new IncreaseAbilityForLabGoalHelper(_extractArts, dueDateDesire, currentRate, tempTotal);
                    helper.ModifyActionList(character, alreadyConsidered, log);
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

    /// <summary>
    /// This is a specialized goal-like object used to figure out 
    /// the value of increasing abilities relative to a certain lab goal.
    /// </summary>
    class IncreaseAbilityForLabGoalHelper
    {
        private double _currentLabTotal;
        private double _effectLevel;
        private double _desire;
        private List<Ability> _abilities;
        public IncreaseAbilityForLabGoalHelper(List<Ability> abilities, double desire, double labTotal, double level)
        {
            _desire = desire;
            _currentLabTotal = labTotal;
            _effectLevel = level;
            _abilities = abilities;
        }

        public virtual void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log)
        {
            IEnumerable<IBook> readableBooks = character.ReadableBooks;
            foreach (Ability ability in _abilities)
            {
                // Handle Reading
                CharacterAbilityBase charAbility = character.GetAbility(ability);
                var topicalBooks = readableBooks.Where(b => b.Topic == ability);
                AddReading(character, alreadyConsidered, topicalBooks);

                // Handle Practice
                // For now, assume 4pt practice on everything
                // after lots of math, the right equation is:
                // Desire * (labTotal + increase)/(labTotal + increase + level)
                double increase = character.GetAbility(ability).GetValueGain(4);
                double desire = (_currentLabTotal + increase) * _desire / (_currentLabTotal + increase + _effectLevel);
                Practice practiceAction = new Practice(ability, desire);
                log.Add("Practicing " + ability.AbilityName + " worth " + (desire).ToString("0.00"));
                alreadyConsidered.Add(practiceAction);

                if (character.GetType() == typeof(Magus))
                {
                    Magus mage = (Magus)character;
                    if (MagicArts.IsArt(charAbility.Ability))
                    {
                        HandleVisUse(mage, charAbility, alreadyConsidered, log);
                    }
                }

                // TODO: Learning By Training
                // TODO: Learning by Teaching
            }
        }

        private void HandleVisUse(Magus mage, CharacterAbilityBase charAbility, ConsideredActions alreadyConsidered, IList<string> log)
        {
            // see if the mage has enough vis of this type
            double stockpile = mage.GetVisCount(charAbility.Ability);
            double visNeed = 0.5 + (charAbility.Value / 10.0);
            double increase = charAbility.GetValueGain(6);
            double baseDesire = (_currentLabTotal + increase) * _desire / (_currentLabTotal + increase + _effectLevel);
            // if so, assume vis will return an average of 6XP
            if (stockpile > visNeed)
            {
                log.Add("Studying vis for " + charAbility.Ability.AbilityName + " worth " + baseDesire.ToString("0.00"));
                VisStudying visStudy = new VisStudying(charAbility.Ability, baseDesire);
                alreadyConsidered.Add(visStudy);
                // TODO: how do we decrement the cost of the vis?
            }
            // putting a limit here to how far the circular loop will go
            else if(baseDesire >= 0.01)
            {
                List<Ability> visType = new List<Ability>();
                visType.Add(charAbility.Ability);
                log.Add("Getting vis to study " + charAbility.Ability.AbilityName + " worth " + (baseDesire / 2).ToString("0.00"));
                VisCondition visCondition = new VisCondition(visType, visNeed - stockpile, baseDesire / 2);
                visCondition.ModifyActionList(mage, alreadyConsidered, log);
            }
            // Handle the possibility of extracting vim vis
            if (charAbility.Ability == MagicArts.Vim)
            {
                ushort additionalTime = 1;
                if (mage.Covenant == null)
                {
                    additionalTime++;
                }
                if (mage.Laboratory == null)
                {
                    additionalTime++;
                }

                // this math is kooky, but even Wolfram Alpha agrees with it
                baseDesire = (_currentLabTotal + increase) * _desire / (additionalTime * (_currentLabTotal + increase) + _effectLevel);

                if (mage.Covenant == null)
                {
                    log.Add("Finding an aura to set up a lab to extract vis to study " + charAbility.Ability.AbilityName + " before doing this lab work worth " + baseDesire.ToString("0.00"));
                    HasCovenantCondition covenantCondition = new HasCovenantCondition(baseDesire);
                    covenantCondition.ModifyActionList(mage, alreadyConsidered, log);
                }
                else if (mage.Laboratory == null)
                {
                    log.Add("Setting up a lab to extract vis to study " + charAbility.Ability.AbilityName + " before doing this lab work worth " + baseDesire.ToString("0.00"));
                    HasLabCondition labCondition = new HasLabCondition(baseDesire);
                    labCondition.ModifyActionList(mage, alreadyConsidered, log);
                }
                // putting a limit here to how far the circular loop will go
                else if(baseDesire >= 0.01)
                {
                    log.Add("Extracting vis to study " + charAbility.Ability.AbilityName + " before doing this lab work worth " + baseDesire.ToString("0.00"));
                    List<Ability> vimVis = new List<Ability>();
                    double visNeeded = 0.5 + (charAbility.Value / 10.0);
                    vimVis.Add(MagicArts.Vim);
                    VisCondition visCondition = new VisCondition(vimVis, visNeeded, baseDesire);
                    visCondition.ModifyActionList(mage, alreadyConsidered, log);
                }
                // TODO: consider the value of looking for another aura
                // TODO: consider the value of looking for vis sites
            }
            // TODO: expand vis study to 
        }


        private void AddReading(Character character, ConsideredActions alreadyConsidered, IEnumerable<IBook> topicalBooks)
        {
            if (topicalBooks.Any())
            {
                var bestBook =
                    (from book in topicalBooks
                     orderby character.GetBookLevelGain(book),
                             book.Level ascending
                     select book).First();
                double increase = character.GetBookLevelGain(bestBook);
                double desire = (_currentLabTotal + increase) * _desire / (_currentLabTotal + increase + _effectLevel);
                // check to see if reading this book is already in the action list
                Reading readingAction = new Reading(bestBook, desire);
                alreadyConsidered.Add(readingAction);
            }
        }
    }

    class InventSpellGoal : IGoal
    {
        private LabScoreGoal _labScore;
        private List<Ability> _abilitiesRequired;
        private Spell _spell;

        public uint? DueDate { get; private set; }
        public double Desire { get; private set; }

        public void DecrementDueDate()
        {
            if (DueDate != null)
            {
                DueDate--;
                _labScore.DecrementDueDate();
            }
        }

        public InventSpellGoal(Spell spell, double desire, uint? dueDate = null)
        {
            DueDate = dueDate;
            Desire = desire;
            _spell = spell;
            _abilitiesRequired = new List<Ability>();
            _abilitiesRequired.Add(spell.BaseArts.Technique);
            _abilitiesRequired.Add(spell.BaseArts.Form);
            _abilitiesRequired.Add(Abilities.MagicTheory);
            List<AttributeType> attributes = new List<AttributeType>();
            attributes.Add(AttributeType.Intelligence);

            // we need at least a lab score above the level of the spell to think about inventing it
            uint? labScoreDueDate = null;
            if (dueDate != null && dueDate > spell.Level)
            {
                labScoreDueDate = dueDate - (uint)spell.Level;
            }
            _labScore = new LabScoreGoal(_abilitiesRequired, attributes, spell.Level, desire, labScoreDueDate);
        }
 
        public void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log)
        {
            double desire = Desire;
            if (DueDate != null)
            {
                desire /= (double)DueDate;
            }

            if (!_labScore.IsComplete(character))
            {
                _labScore.ModifyActionList(character, alreadyConsidered, log);
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
                    _labScore.ModifyActionList(character, alreadyConsidered, log);
                }
                if (extraTotal >= level)
                {
                    // there's no reason to consider adding to abilities for this spell
                    // TODO: eventually, we should consider the fact that the extra learning
                    // could allow one to learn more spells in a season
                    log.Add("Inventing this spell worth " + desire.ToString("0.00"));
                    alreadyConsidered.Add(new InventSpell(_spell, Abilities.MagicTheory, desire));
                }
                else
                {
                    // we are in the multi-season-to-invent scenario
                    desire = extraTotal * desire / level;
                    alreadyConsidered.Add(new InventSpell(_spell, Abilities.MagicTheory, desire));

                    // the difference between the desire of starting now
                    // and the desire of starting after practice
                    // is the effective value of practicing here
                    IncreaseAbilityForLabGoalHelper helper = new IncreaseAbilityForLabGoalHelper(_abilitiesRequired, desire, extraTotal, level);
                    helper.ModifyActionList(character, alreadyConsidered, log);
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

        public uint? DueDate { get; private set; }
        public double Desire { get; private set; }

        public void DecrementDueDate()
        {
            if (DueDate != null)
            {
                DueDate--;
                _hasLabCondition.DecrementDueDate();
            }
        }

        public LongevityRitualGoal(double desire, uint? dueDate = null)
        {
            DueDate = dueDate;
            Desire = desire;
            _abilitiesRequired = new List<Ability>();
            _abilitiesRequired.Add(MagicArts.Creo);
            _abilitiesRequired.Add(MagicArts.Vim);
            _abilitiesRequired.Add(Abilities.MagicTheory);
            List<AttributeType> attributes = new List<AttributeType>();
            attributes.Add(AttributeType.Intelligence);

            // we need a lab to create a longevity ritual
            _hasLabCondition = new HasLabCondition(desire, dueDate == null ? null : dueDate - 3);
        }
 
        public void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log)
        {
            double visNeed = character.SeasonalAge / 20.0;
            VisCondition visCondition = new VisCondition(_abilitiesRequired, visNeed, Desire, DueDate - 2);

            bool visComplete = visCondition.IsComplete(character);
            bool labComplete = _hasLabCondition.IsComplete(character);

            if (!visComplete)
            {
                visCondition.ModifyActionList(character, alreadyConsidered, log);
            }
            if (!labComplete)
            {
                _hasLabCondition.ModifyActionList(character, alreadyConsidered, log);
            }
            if (visComplete && labComplete)
            {
                //effectively, every five points of lab total is worth a decade of effectiveness
                // TODO: figure out the math to value another season of learning
                // versus the cost of waiting
                // optimally, this will start to factor in the notion of due dates
                double dueDateDesire = Desire;
                if (DueDate != null)
                {
                    dueDateDesire /= (double)DueDate;
                }
                log.Add("Performing longevity ritual worth " + dueDateDesire.ToString("0.00"));
                alreadyConsidered.Add(new LongevityRitual(Abilities.MagicTheory, dueDateDesire));
                
            }
        }

        public bool IsComplete(Character character)
        {
            return character.LongevityRitual > 0;
        }
    }
}
