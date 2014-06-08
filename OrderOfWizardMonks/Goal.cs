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
                    goal.ModifyActionList(this, actions);
                }
            }
            return actions.GetBestAction();
        }

        public virtual void ReprioritizeGoals()
        {
            throw new NotImplementedException();
        }
    }

    public partial class Magus
    {
        public override void ReprioritizeGoals()
        {
            if (_covenant == null)
            {
                _goals.Add(GoalFactory.GenerateGoal(GoalType.FoundCovenant, 0.75));
            }
            else if (Laboratory == null)
            {
                _goals.Add(GoalFactory.GenerateGoal(GoalType.BuildLab, 0.75));
            }
        }
    }

    public class ConsideredActions
    {
        Dictionary<Activity, IList<IAction>> ActionTypeMap;
        
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
        
        public IAction GetBestAction()
        {
            return ActionTypeMap.SelectMany(a => a.Value).OrderBy(a => a.Desire).FirstOrDefault();
        }
    }

    public interface IGoal
    {
        void ModifyActionList(Character character, ConsideredActions alreadyConsidered);
        bool IsComplete(Character character);
        DateTime? DueDate { get; }
        double Desire { get; }
    }

    class AbilityScoreCondition : IGoal
    {
        #region Protected fields
        protected List<Ability> _abilities;
        protected double _total;
        #endregion

        public DateTime? DueDate { get; private set; }
        public virtual double Desire { get; private set; }

        public AbilityScoreCondition(List<Ability> abilities, double total, double desire, DateTime? dueDate = null)
        {
            _abilities = abilities;
            _total = total;
            Desire = desire;
            DueDate = dueDate;
        }

        public virtual void ModifyActionList(Character character, ConsideredActions alreadyConsidered)
        {
            double remainingTotal = GetRemainingTotal(character);
            IEnumerable<IBook> readableBooks = character.ReadableBooks;
            foreach (Ability ability in _abilities)
            {
                // Handle Reading
                var topicalBooks = readableBooks.Where(b => b.Topic == ability);
                AddReading(character, alreadyConsidered, topicalBooks, remainingTotal);

                // Handle Practice
                // For now, assume 4pt practice on everything
                Practice practiceAction = new Practice(ability, character.GetAbility(ability).GetValueGain(4) / remainingTotal);
                alreadyConsidered.Add(practiceAction);

                // See if we need to handle vis
                // TODO: Learning By Training
                // TODO: Learning by Teaching
            }
        }

        public virtual bool IsComplete(Character character)
        {
            return GetRemainingTotal(character) > 0;
        }

        protected virtual double GetRemainingTotal(Character character)
        {
            double currentTotal = 0;
            foreach (Ability ability in _abilities)
            {
                currentTotal += character.GetAbility(ability).GetValue();
            }
            return _total - currentTotal;
        }

        private void AddReading(Character character, ConsideredActions alreadyConsidered, IEnumerable<IBook> topicalBooks, double remainingTotal)
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
                         PerceivedValue = character.GetBookLevelGain(book) * Desire / remainingTotal
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

        public CharacteristicAbilityScoreCondition(List<Ability> abilities, List<AttributeType> attributes, double total, double desire, DateTime? dueDate = null) :
            base(abilities, total, desire, dueDate)
        {
            _attributes = attributes;
        }

        protected override double GetRemainingTotal(Character character)
        {
            double currentTotal = 0;
            foreach (Ability ability in _abilities)
            {
                currentTotal += character.GetAbility(ability).GetValue();
            }
            foreach(AttributeType attribute in _attributes)
            {
                currentTotal += character.GetAttribute(attribute).Value;
            }
            return _total - currentTotal;
        }

        public override void ModifyActionList(Character character, ConsideredActions alreadyConsidered)
        {
            base.ModifyActionList(character, alreadyConsidered);
            // TODO: consider CrVi spells that increase statistics
        }
    }

    class LabScoreGoal : CharacteristicAbilityScoreCondition
    {
        private HasLabCondition _hasLabCondition;

        public LabScoreGoal(List<Ability> abilities, List<AttributeType> attributes, double total, double desire, DateTime? dueDate = null)
            : base(abilities, attributes, total, desire, dueDate)
        {
            _hasLabCondition = new HasLabCondition(desire, dueDate);
        }

        protected override double GetRemainingTotal(Character character)
        {
            double currentTotal = 0;
            foreach (Ability ability in _abilities)
            {
                currentTotal += character.GetAbility(ability).GetValue();
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

        public override void ModifyActionList(Character character, ConsideredActions alreadyConsidered)
        {
            if (!_hasLabCondition.IsComplete(character))
            {
                _hasLabCondition.ModifyActionList(character, alreadyConsidered);
            }
            else
            {
                base.ModifyActionList(character, alreadyConsidered);

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
        public DateTime? DueDate { get; private set; }
        public double Desire { get; private set; }

        public HasCovenantCondition(double value, DateTime? dueDate = null)
        {
            Desire = value;
            DueDate = dueDate;
        }

        public void ModifyActionList(Character character, ConsideredActions alreadyConsidered)
        {
            alreadyConsidered.Add(new FindAura(Abilities.AreaLore, Desire));
        }

        public bool IsComplete(Character character)
        {
            return ((Magus)character).Covenant != null;
        }
    }

    class HasLabCondition : IGoal
    {
        private HasCovenantCondition _hasCovenant;

        public DateTime? DueDate { get; private set; }
        public double Desire { get; private set; }

        public HasLabCondition(double value, DateTime? dueDate = null)
        {
            Desire = value;
            DueDate = dueDate;
            _hasCovenant = new HasCovenantCondition(value, dueDate);
        }

        public void ModifyActionList(Character character, ConsideredActions alreadyConsidered)
        {
            if (!_hasCovenant.IsComplete(character))
            {
                _hasCovenant.ModifyActionList(character, alreadyConsidered);
            }
            else
            {
                alreadyConsidered.Add(new BuildLaboratory(Abilities.MagicTheory, Desire));
            }
        }

        public bool IsComplete(Character character)
        {
            return ((Magus)character).Laboratory != null;
        }
    }

    class VisCondition : IGoal
    {
        public DateTime? DueDate { get; private set; }
        public double Desire { get; private set; }

        List<Ability> VisTypes;
        double Total;

        public void ModifyActionList(Character character, ConsideredActions alreadyConsidered)
        {
            // TODO: see how much vis can be extracted
        }

        public bool IsComplete(Character character)
        {
            if (character.GetType() != typeof(Magus))
            {
                throw new ArgumentException("Only magi can have Vis conditions");
            }
            Magus mage = (Magus)character;
            // TODO: we need a way to see how much vis a mage has
            return false;
        }

        public void ModifyActionList(Character character, ConsideredActions alreadyConsidered, double conditionValue)
        {
            throw new NotImplementedException();
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

        public virtual void ModifyActionList(Character character, ConsideredActions alreadyConsidered)
        {
            IEnumerable<IBook> readableBooks = character.ReadableBooks;
            foreach (Ability ability in _abilities)
            {
                // Handle Reading
                var topicalBooks = readableBooks.Where(b => b.Topic == ability);
                AddReading(character, alreadyConsidered, topicalBooks);

                // Handle Practice
                // For now, assume 4pt practice on everything
                // after lots of math, the right equation is:
                // Desire * (labTotal + increase)/(labTotal + increase + level)
                double increase = character.GetAbility(ability).GetValueGain(4);
                double desire = (_currentLabTotal + increase) * _desire / (_currentLabTotal + increase + _effectLevel);
                Practice practiceAction = new Practice(ability, desire);
                alreadyConsidered.Add(practiceAction);

                // See if we need to handle vis
                // TODO: Learning By Training
                // TODO: Learning by Teaching
            }
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

        public DateTime? DueDate { get; private set; }
        public double Desire { get; private set; }

        public InventSpellGoal(Spell spell, double desire, DateTime? dueDate)
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
            _labScore = new LabScoreGoal(_abilitiesRequired, attributes, spell.Level, desire, dueDate);
        }
 
        public void ModifyActionList(Character character, ConsideredActions alreadyConsidered)
        {
            if (!_labScore.IsComplete(character))
            {
                _labScore.ModifyActionList(character, alreadyConsidered);
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
                    _labScore.ModifyActionList(character, alreadyConsidered);
                }
                if (extraTotal >= level)
                {
                    // there's no reason to consider adding to abilities for this spell
                    // TODO: eventually, we should consider the fact that the extra learning
                    // could allow one to learn more spells in a season
                    alreadyConsidered.Add(new InventSpell(_spell, Abilities.MagicTheory, Desire));
                }
                else
                {
                    // we are in the multi-season-to-invent scenario
                    double desire = extraTotal * Desire / level;
                    alreadyConsidered.Add(new InventSpell(_spell, Abilities.MagicTheory, desire));

                    // the difference between the desire of starting now
                    // and the desire of starting after practice
                    // is the effective value of practicing here
                    IncreaseAbilityForLabGoalHelper helper = new IncreaseAbilityForLabGoalHelper(_abilitiesRequired, Desire, extraTotal, level);
                    helper.ModifyActionList(character, alreadyConsidered);
                }
            }
        }

        public bool IsComplete(Character character)
        {
            return ((Magus)character).SpellList.Where(s => s.Name == _spell.Name).Any();
        }
    }
}
