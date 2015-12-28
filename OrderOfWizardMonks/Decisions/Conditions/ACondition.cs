using System.Collections.Generic;

namespace WizardMonks.Decisions.Conditions
{
    public abstract class ACondition
    {
        public Character Character { get; private set; }
        public abstract bool ConditionFulfilled { get; }

        public abstract void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log);

        public ACondition(Character character)
        {
            Character = character;
        }
    }
}
