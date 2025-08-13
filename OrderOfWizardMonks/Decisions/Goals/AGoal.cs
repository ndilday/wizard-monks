using System;
using System.Collections.Generic;
using System.Linq;

using WizardMonks.Decisions.Conditions;
using WizardMonks.Economy;
using WizardMonks.Models.Characters;

namespace WizardMonks.Decisions.Goals
{
    public abstract class AGoal : IGoal
    {
        protected bool _completed = false;

        public uint? AgeToCompleteBy { get; private set; }
        public double Desire { get; set; }
        public List<ACondition> Conditions { get; protected set; }
        public Character Character { get; private set; }

        public AGoal(Character character, uint? ageToCompleteBy, double desire)
        {
            AgeToCompleteBy = ageToCompleteBy;
            Desire = desire;
            Character = character;
            Conditions = new List<ACondition>();
        }

        public virtual void AddActionPreferencesToList(ConsideredActions alreadyConsidered, Desires desires, IList<string> log)
        {
            if (!_completed)
            {
                bool conditionsFulfilled = true;
                foreach (ACondition condition in Conditions)
                {
                    if (!condition.ConditionFulfilled)
                    {
                        conditionsFulfilled = false;
                        condition.AddActionPreferencesToList(alreadyConsidered, desires, log);
                    }
                }
                if (conditionsFulfilled)
                {
                    _completed = true;
                }
            }
        }

        public virtual bool IsComplete()
        {
            return _completed;
        }
    }
}
