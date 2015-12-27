using System;
using System.Collections.Generic;
using System.Linq;

namespace WizardMonks.Decisions
{
    class AbilityScoreCondition : ACondition
    {
        private double? _currentTotal;
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

        public AbilityScoreCondition(Character character, List<Ability> abilities, List<AttributeType> attributes, double totalNeeded) :
            base(character)
        {
            Abilities = abilities;
            Attributes = attributes;
            TotalNeeded = totalNeeded;
            _currentTotal = GetTotal();
        }

        public AbilityScoreCondition(Character character, Ability ability, double totalNeeded) :
            base(character)
        {
            Abilities = new List<Ability>(1);
            Abilities.Add(ability);
            Attributes = null;
            TotalNeeded = totalNeeded;
            _currentTotal = GetTotal();
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log)
        {
            throw new NotImplementedException();
        }

        public double GetTotal()
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
    }
}
