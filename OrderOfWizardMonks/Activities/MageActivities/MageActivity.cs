using System;
using WizardMonks.Models.Characters;

namespace WizardMonks.Activities.MageActivities
{
    public abstract class AMageActivity : IActivity
    {
        public Activity Action { get; protected set; }

        public double Desire { get; set; }

        public abstract void Act(Character character);

        protected static Magus ConfirmCharacterIsMage(Character character)
        {
            if (typeof(Magus) != character.GetType())
            {
                throw new InvalidCastException("Only magi can perform this action!");
            }
            return (Magus)character;
        }

        public abstract bool Matches(IActivity action);

        public abstract string Log();
    }
}
