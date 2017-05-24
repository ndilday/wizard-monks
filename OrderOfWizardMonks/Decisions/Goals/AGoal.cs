using System;
using System.Collections.Generic;
using System.Linq;

using WizardMonks.Decisions.Conditions;

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

        public virtual void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log)
        {
            if (!_completed)
            {
                bool conditionsFulfilled = true;
                foreach (ACondition condition in Conditions)
                {
                    if (!condition.ConditionFulfilled)
                    {
                        conditionsFulfilled = false;
                        condition.AddActionPreferencesToList(alreadyConsidered, log);
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

        public virtual IList<BookDesire> GetBookDesires()
        {
            List<BookDesire> bookDesires = new List<BookDesire>();
            foreach(ACondition condition in Conditions)
            {
                var conditionalDesires = condition.GetBookDesires();
                if(conditionalDesires != null && conditionalDesires.Any())
                {
                    bookDesires.AddRange(conditionalDesires);
                }
            }
            return bookDesires;
        }

        public virtual void ModifyVisDesires(Magus magus, VisDesire[] visDesires)
        {
            foreach(ACondition condition in this.Conditions)
            {
                condition.ModifyVisDesires(visDesires);
            }
        }
    }
}
