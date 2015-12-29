using System.Collections.Generic;

namespace WizardMonks.Decisions.Conditions
{
    public abstract class ACondition
    {
        public Character Character { get; private set; }
        public uint AgeToCompleteBy { get; set; }
        public double Desire { get; set; }
        public ushort ConditionDepth { get; protected set; }
        public abstract bool ConditionFulfilled { get; }

        public abstract void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log);

        public ACondition(Character character, uint ageToCompleteBy, double desire, ushort conditionDepth = 1)
        {
            Character = character;
            AgeToCompleteBy = ageToCompleteBy;
            Desire = desire;
            ConditionDepth = conditionDepth;
        }
    }
}
