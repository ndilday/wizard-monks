using System;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Events;

namespace WizardMonks.Activities.MageActivities
{
    public abstract class AMageActivity : IActivity
    {
        public Activity Action { get; protected set; }

        public double Desire { get; set; }

        /// <summary>
        /// Set by DoMageAction() when the activity produces an outcome worth appraising.
        /// Read by CharacterAdvancementService after Act() returns.
        /// </summary>
        public WorldEvent EmittedEvent { get; protected set; }

        public abstract void Act(Character character);

        protected static HermeticMagus ConfirmCharacterIsMage(Character character)
        {
            if (typeof(HermeticMagus) != character.GetType())
            {
                throw new InvalidCastException("Only magi can perform this action!");
            }
            return (HermeticMagus)character;
        }

        public abstract bool Matches(IActivity action);

        public abstract string Log();
    }
}
