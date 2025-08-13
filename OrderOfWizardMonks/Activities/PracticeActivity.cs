using System;
using WizardMonks.Models.Characters;

namespace WizardMonks.Activities
{
    [Serializable]
    public class PracticeActivity(Ability ability, double desire) : IActivity
    {
        public Ability Ability { get; private set; } = ability;

        public virtual Activity Action
        {
            get { return Activity.Practice; }
        }

        public double Desire { get; set; } = desire;

        public virtual void Act(Character character)
        {
            character.Log.Add("Practicing " + Ability.AbilityName);
            character.GetAbility(Ability).AddExperience(4);
        }

        public virtual bool Matches(IActivity action)
        {
            if (action.Action != Activity.Practice)
            {
                return false;
            }
            PracticeActivity practice = (PracticeActivity)action;
            return practice.Ability == Ability;
        }

        public virtual string Log()
        {
            return "Practicing " + Ability.AbilityName + " worth " + Desire.ToString("0.000");
        }
    }

}
