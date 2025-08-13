using System.Collections.Generic;
using System.Linq;
using WizardMonks.Activities;
using WizardMonks.Activities.MageActivities;
using WizardMonks.Economy;
using WizardMonks.Instances;
using WizardMonks.Models.Books;
using WizardMonks.Models.Characters;

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
            Attributes = new List<AttributeType>();
            TotalNeeded = totalNeeded;
            _currentTotal = GetTotal();
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, Desires desires, IList<string> log)
        {
            _currentTotal = GetTotal();
            if (!ConditionFulfilled)
            {
                ModifyDesires(desires);
                // the basic structure is (portion of necessary gain action provides) * (desire / time left until needed)
                foreach (Ability ability in Abilities)
                {
                    var topicalBooks = Character.ReadableBooks.Where(b => b.Topic == ability);
                    if (topicalBooks.Any())
                    {
                        AddReadingToActionList(topicalBooks, ability, alreadyConsidered, log);
                    }
                    // we should only practice if there isn't a book to read
                    else if (!MagicArts.IsArt(ability))
                    {
                        AddPracticeToActionList(ability, alreadyConsidered, log);
                    }
                    // should we only study vis if we don't have a book?
                    else if (Character.GetType() == typeof(Magus))
                    {
                        AddVisUseToActionList(ability, alreadyConsidered, desires, log);
                    }
                }
            }
        }

        private void ModifyDesires(Desires desires)
        {
            foreach (Ability ability in this.Abilities)
            {
                // if we have a summa to read, we should do that before studying vis
                if (MagicArts.IsArt(ability) && this.Character.GetBestSummaToRead(ability) == null)
                {
                    double visNeed = 0.5 + (this.Character.GetAbility(ability).Value / 10.0);
                    desires.AddVisDesire(ability, visNeed);
                }

                double abilityLevel = this.Character.GetAbilityMaximumFromReading(ability);
                if (abilityLevel < TotalNeeded && this.Character.GetBestBookToRead(ability) == null)
                {
                    desires.AddBookDesire(new BookDesire(this.Character, ability, Desire, abilityLevel));
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
            if (!double.IsNaN(effectiveDesire) && effectiveDesire > 0)
            {
                PracticeActivity practiceAction = new(ability, effectiveDesire);
                log.Add(Character.Name + " Practicing " + ability.AbilityName + " worth " + (effectiveDesire).ToString("0.000"));
                alreadyConsidered.Add(practiceAction);
            }
        }

        private void AddReadingToActionList(IEnumerable<ABook> topicalBooks, Ability ability, ConsideredActions alreadyConsidered, IList<string> log)
        {
            ABook bestBook = Character.GetBestBookToRead(ability);
            
            if (bestBook != null)
            {
                double gain = Character.GetBookLevelGain(bestBook);
                double effectiveDesire = GetDesirabilityOfIncrease(gain);
                if (!double.IsNaN(effectiveDesire) && effectiveDesire > 0)
                {
                    log.Add(Character.Name + "Reading " + bestBook.Title + " worth " + (effectiveDesire).ToString("0.000"));
                    ReadActivity readingAction = new(bestBook, effectiveDesire);
                    alreadyConsidered.Add(readingAction);
                }
            }
        }

        private void AddVisUseToActionList(Ability ability, ConsideredActions alreadyConsidered, Desires desires, IList<string> log)
        {
            Magus mage = (Magus)Character;
            CharacterAbilityBase magicArt = mage.GetAbility(ability);
            double effectiveDesire = GetDesirabilityOfIncrease(magicArt.GetValueGain(mage.VisStudyRate));
            if (!double.IsNaN(effectiveDesire) && effectiveDesire > 0)
            {
                // see if the mage has enough vis of this type
                double stockpile = mage.GetVisCount(ability);
                double visNeed = 0.5 + (magicArt.Value / 10.0);

                // if so, assume vis will return an average of 6XP + aura
                if (stockpile > visNeed)
                {
                    log.Add(mage.Name + "Studying vis for " + magicArt.Ability.AbilityName + ability.AbilityName + " worth " + effectiveDesire.ToString("0.000"));
                    StudyVisActivity visStudy = new(magicArt.Ability, effectiveDesire);
                    alreadyConsidered.Add(visStudy);
                    // TODO: how do we decrement the cost of the vis?
                }
                // putting a limit here to how far the circular loop will go
                else if (ConditionDepth < 10 && AgeToCompleteBy > mage.SeasonalAge)
                {
                    List<Ability> visType = new();
                    visType.Add(magicArt.Ability);
                    // Magus magus, uint ageToCompleteBy, double desire, Ability ability, double totalNeeded, ushort conditionDepth
                    VisCondition visCondition =
                        new(mage, AgeToCompleteBy - 1, effectiveDesire, ability, visNeed, (ushort)(ConditionDepth + 1));
                    visCondition.AddActionPreferencesToList(alreadyConsidered, desires, log);
                }
            }
        }

        private double GetDesirabilityOfIncrease(double increase)
        {
            // TODO: temporarily seeing how logic changes if we stop reducing desire based on how far away we are from the desire
            // since this has had the impact of making people want to keep practicing a particular thing once they start doing it
            //double proportion = increase / (TotalNeeded - _currentTotal);
            double immediateDesire = Desire / (AgeToCompleteBy - Character.SeasonalAge);
            return immediateDesire * increase / ConditionDepth;
        }
    }
}
