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
            Attributes = new List<AttributeType>();
            TotalNeeded = totalNeeded;
            _currentTotal = GetTotal();
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log)
        {
            string openingLog = "Trying to get abilities ";
            foreach(Ability ability in Abilities)
            {
                openingLog += " " + ability.AbilityName;
            }
            openingLog += " up to a total of " + TotalNeeded.ToString("0.0");
            log.Add(openingLog);
            _currentTotal = GetTotal();
            if (!ConditionFulfilled)
            {
                // the basic structure is (portion of necessary gain action provides) * (desire / time left until needed)
                foreach (Ability ability in Abilities)
                {
                    if (!MagicArts.IsArt(ability))
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
        }

        public override List<BookDesire> GetBookDesires()
        {
            List<BookDesire> bookDesires = new List<BookDesire>();
            if (!ConditionFulfilled)
            {
                foreach (Ability ability in this.Abilities)
                {
                    bookDesires.Add(new BookDesire(ability, this.Character.GetAbility(ability).Value));
                }
            }
            return bookDesires;
        }

        public override void ModifyVisDesires(VisDesire[] desires)
        {
            foreach (Ability ability in this.Abilities)
            {
                if (MagicArts.IsArt(ability))
                {
                    double visNeed = 0.5 + (this.Character.GetAbility(ability).Value / 10.0);
                    foreach (VisDesire visDesire in desires)
                    {
                        if (visDesire.Art == ability)
                        {
                            visDesire.Quantity += visNeed;
                            break;
                        }
                    }
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
                Practice practiceAction = new Practice(ability, effectiveDesire);
                log.Add("Practicing " + ability.AbilityName + " worth " + (effectiveDesire).ToString("0.000"));
                alreadyConsidered.Add(practiceAction);
            }
        }

        private void AddReadingToActionList(IEnumerable<IBook> topicalBooks, Ability ability, ConsideredActions alreadyConsidered, IList<string> log)
        {
            IBook bestBook = Character.GetBestBookToRead(ability);
            
            if (bestBook != null)
            {
                double gain = Character.GetBookLevelGain(bestBook);
                double effectiveDesire = GetDesirabilityOfIncrease(gain);
                if (!double.IsNaN(effectiveDesire) && effectiveDesire > 0)
                {
                    log.Add("Reading " + bestBook.Title + " worth " + (effectiveDesire).ToString("0.000"));
                    Read readingAction = new Read(bestBook, effectiveDesire);
                    alreadyConsidered.Add(readingAction);
                }
            }
        }

        private void AddVisUseToActionList(Ability ability, ConsideredActions alreadyConsidered, IList<string> log)
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
                    log.Add("Studying vis for " + magicArt.Ability.AbilityName + " worth " + effectiveDesire.ToString("0.000"));
                    VisStudying visStudy = new VisStudying(magicArt.Ability, effectiveDesire);
                    alreadyConsidered.Add(visStudy);
                    // TODO: how do we decrement the cost of the vis?
                }
                // putting a limit here to how far the circular loop will go
                else if (ConditionDepth <= 10)
                {
                    List<Ability> visType = new List<Ability>();
                    visType.Add(magicArt.Ability);
                    // Magus magus, uint ageToCompleteBy, double desire, Ability ability, double totalNeeded, ushort conditionDepth
                    VisCondition visCondition =
                        new VisCondition(mage, AgeToCompleteBy - 1, effectiveDesire, ability, visNeed, (ushort)(ConditionDepth + 1));
                    visCondition.AddActionPreferencesToList(alreadyConsidered, log);
                }
            }
        }

        private double GetDesirabilityOfIncrease(double increase)
        {
            double fractionalIncrease = increase / (TotalNeeded - _currentTotal);
            return Desire * fractionalIncrease / (ConditionDepth * TimeUntilDue);
        }
    }
}
