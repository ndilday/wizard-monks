using System;
using System.Collections.Generic;
using System.Linq;

namespace WizardMonks
{
    public partial class Character
    {
        List<Goal2> Goals;
        IAction Decide()
        {
            ConsideredActions actions = new ConsideredActions();
            double subValue;
            foreach (Goal2 goal in Goals)
            {
                var activeConditions = goal.Conditions.Where(c => !c.IsComplete(this));
                subValue = goal.Value / (double)activeConditions.Count();
                foreach (IGoalCondition condition in activeConditions)
                {
                    condition.ModifyActionList(this, actions, subValue);
                }
            }
            return null;
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
    }

    class Goal2
    {
        public DateTime DueDate { get; set; }
        public List<IGoalCondition> Conditions { get; set; }
        public double Value;
    }

    interface IGoalCondition
    {
        void ModifyActionList(Character character, ConsideredActions alreadyConsidered, double conditionValue);
        bool IsComplete(Character character);
    }

    class AbilityScoreCondition : IGoalCondition
    {
        protected List<Ability> Abilities;
        protected double Total;

        protected virtual double GetRemainingTotal(Character character)
        {
            double currentTotal = 0;
            foreach (Ability ability in Abilities)
            {
                currentTotal += character.GetAbility(ability).GetValue();
            }
            return Total - currentTotal;
        }

        public virtual void ModifyActionList(Character character, ConsideredActions alreadyConsidered, double conditionValue)
        {
            double remainingTotal = GetRemainingTotal(character);
            IEnumerable<IBook> readableBooks = character.ReadableBooks;
            foreach (Ability ability in Abilities)
            {
                // Handle Reading
                var topicalBooks = readableBooks.Where(b => b.Topic == ability);
                AddReading(character, alreadyConsidered, topicalBooks, conditionValue, remainingTotal);

                // Handle Practice
                // For now, assume 4pt practice on everything
                Practice practiceAction = new Practice(ability, character.GetAbility(ability).GetValueGain(4) / remainingTotal);
                alreadyConsidered.Add(practiceAction);

                // See if we need to handle vis
                // TODO: Learning By Training
                // TODO: Learning by Teaching
            }
        }

        private static void AddReading(Character character, ConsideredActions alreadyConsidered, IEnumerable<IBook> topicalBooks, double conditionValue, double remainingTotal)
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
                         PerceivedValue = character.GetBookLevelGain(book) * conditionValue / remainingTotal
                     }).First();
                // check to see if reading this book is already in the action list
                Reading readingAction = new Reading(bestBook.Book, bestBook.PerceivedValue);
                alreadyConsidered.Add(readingAction);
            }
        }

        public bool IsComplete(Character character)
        {
            return GetRemainingTotal(character) > 0;
        }
    }

    class CharacteristicAbilityScoreCondition : AbilityScoreCondition
    {
        protected List<AttributeType> Attributes;
        protected override double GetRemainingTotal(Character character)
        {
            double currentTotal = 0;
            foreach (Ability ability in Abilities)
            {
                currentTotal += character.GetAbility(ability).GetValue();
            }
            foreach(AttributeType attribute in Attributes)
            {
                currentTotal += character.GetAttribute(attribute).Value;
            }
            return Total - currentTotal;
        }

        public override void ModifyActionList(Character character, ConsideredActions alreadyConsidered, double conditionValue)
        {
            base.ModifyActionList(character, alreadyConsidered, conditionValue);
            // TODO: consider CrVi spells that increase statistics
        }
    }

    class LabScoreCondition : CharacteristicAbilityScoreCondition
    {
        protected override double GetRemainingTotal(Character character)
        {
            double currentTotal = 0;
            foreach (Ability ability in Abilities)
            {
                currentTotal += character.GetAbility(ability).GetValue();
            }
            foreach (AttributeType attribute in Attributes)
            {
                currentTotal += character.GetAttribute(attribute).Value;
            }
            if(character.GetType() == typeof(Magus))
            {
                Magus mage = (Magus)character;
                currentTotal += mage.Laboratory.Quality;
                foreach (Ability ability in Abilities)
                {
                    if (mage.Laboratory.ArtModifiers.ContainsKey(ability))
                    {
                        currentTotal += mage.Laboratory.ArtModifiers[ability];
                    }
                }
                // TODO: we probably need to take activity type into account, 
                // or that needs to be taken into account in setting the total of this object
            }
            return Total - currentTotal;
        }

        public override void ModifyActionList(Character character, ConsideredActions alreadyConsidered, double conditionValue)
        {
            base.ModifyActionList(character, alreadyConsidered, conditionValue);
            // TODO: consider laboratory refinements
            // TODO: consider taking an apprentice
            // TODO: consider finding a familiar
        }
    }

    class VisCondition :IGoalCondition
    {
        List<Ability> VisTypes;
        double Total;

        public virtual void ModifyActionList(Character character, IList<IAction> alreadyConsidered)
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
}
