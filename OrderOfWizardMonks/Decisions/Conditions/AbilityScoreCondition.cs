using System;
using System.Collections.Generic;
using System.Linq;

using WizardMonks.Instances;

namespace WizardMonks.Decisions.Conditions
{
    class AbilityScoreCondition : ACondition
    {
        private double _currentTotal;
        public List<Ability> Abilities { get; protected set; }
        public List<AttributeType> Attributes { get; protected set; }
        public double TotalNeeded { get; protected set; }
        public override bool ConditionFulfilled
        {
            get
            {
                return _currentTotal >= TotalNeeded;
            }
        }

        public AbilityScoreCondition(Character character, uint dueDate, double desire, List<Ability> abilities, List<AttributeType> attributes, double totalNeeded) :
            base(character, dueDate, desire)
        {
            Abilities = abilities;
            Attributes = attributes;
            TotalNeeded = totalNeeded;
            _currentTotal = GetTotal();
        }

        public AbilityScoreCondition(Character character, uint dueDate, double desire, Ability ability, double totalNeeded) :
            base(character, dueDate, desire)
        {
            Abilities = new List<Ability>(1);
            Abilities.Add(ability);
            Attributes = null;
            TotalNeeded = totalNeeded;
            _currentTotal = GetTotal();
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log)
        {
            _currentTotal = GetTotal();

            // the basic structure is (portion of necessary gain action provides) * (desire / time left until needed)
            foreach (Ability ability in Abilities)
            {
                if (MagicArts.IsArt(ability))
                {
                    AddPracticeToActionList(ability, alreadyConsidered, log);
                }
                else if (Character.GetType() == typeof(Magus))
                {
                    AddVisUseToActionList(ability, alreadyConsidered, log);
                }

                var topicalBooks = Character.ReadableBooks.Where(b => b.Topic == ability);
                if (topicalBooks.Any())
                {
                    AddReadingToActionList(topicalBooks, ability, alreadyConsidered, log);
                }
            }
        }

        private double GetTotal()
        {
            double total = 0;
            foreach(AttributeType attributeType in Attributes)
            {
                total += Character.GetAttributeValue(attributeType);
            }
            foreach(Ability ability in Abilities)
            {
                total += Character.GetAbility(ability).Value;
            }
            return total;
        }

        private void AddPracticeToActionList(Ability ability, ConsideredActions alreadyConsidered, IList<string> log)
        {
            // For now, assume 4pt practice on everything
            double effectiveDesire = GetDesirabilityOfIncrease(Character.GetAbility(ability).GetValueGain(4));
            Practice practiceAction = new Practice(ability, effectiveDesire);
            log.Add("Practicing " + ability.AbilityName + " worth " + (effectiveDesire).ToString("0.00"));
            alreadyConsidered.Add(practiceAction);
        }

        private void AddReadingToActionList(IEnumerable<IBook> topicalBooks, Ability ability, ConsideredActions alreadyConsidered, IList<string> log)
        {
            var bestBook =
                    (from book in topicalBooks
                     orderby Character.GetBookLevelGain(book),
                             book.Level ascending
                     select new { Book = book, Gain = Character.GetBookLevelGain(book)}).First();
            double effectiveDesire = GetDesirabilityOfIncrease(bestBook.Gain);
            log.Add("Reading " + bestBook.Book.Title + " worth " + (effectiveDesire).ToString("0.00"));
            Reading readingAction = new Reading(bestBook.Book, effectiveDesire);
            alreadyConsidered.Add(readingAction);
        }

        private void AddVisUseToActionList(Ability ability, ConsideredActions alreadyConsidered, IList<string> log)
        {
            Magus mage = (Magus)Character;
            CharacterAbilityBase magicArt = mage.GetAbility(ability);
            double effectiveDesire = GetDesirabilityOfIncrease(magicArt.GetValueGain(mage.VisStudyRate));

            // see if the mage has enough vis of this type
            double stockpile = mage.GetVisCount(ability);
            double visNeed = 0.5 + (magicArt.Value / 10.0);
            
            // if so, assume vis will return an average of 6XP + aura
            if (stockpile > visNeed)
            {
                log.Add("Studying vis for " + magicArt.Ability.AbilityName + " worth " + effectiveDesire.ToString("0.00"));
                VisStudying visStudy = new VisStudying(magicArt.Ability, effectiveDesire);
                alreadyConsidered.Add(visStudy);
                // TODO: how do we decrement the cost of the vis?
            }
            // putting a limit here to how far the circular loop will go
            else if (baseDesire >= 0.01)
            {
                List<Ability> visType = new List<Ability>();
                visType.Add(magicArt.Ability);
                VisCondition visCondition =
                    new VisCondition(visType, visNeed - stockpile, baseDesire, (byte)(BaseTier + 1), BaseDueDate == null ? null : BaseDueDate - 1);
                visCondition.ModifyActionList(mage, alreadyConsidered, log);
            }
        }

        private double GetDesirabilityOfIncrease(double increase)
        {
            double proportion = increase / (TotalNeeded - _currentTotal);
            double immediateDesire = Desire / (AgeToCompleteBy - Character.SeasonalAge);
            return immediateDesire * proportion;
        }
    }
}
