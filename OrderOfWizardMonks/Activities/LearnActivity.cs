using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WizardMonks.Models.Characters;

namespace WizardMonks.Activities
{
    [Serializable]
    public class LearnActivity(double quality, double maxLevel, Ability topic, Character teacher) : IActivity
    {
        private readonly double _quality = quality;
        private readonly double _maxLevel = maxLevel;
        public Ability Topic { get; private set; } = topic;
        private readonly Character _teacher = teacher;

        public Activity Action
        {
            get { return Activity.Learn; }
        }

        public double Desire { get; set; }

        public void Act(Character character)
        {
            character.GetAbility(Topic).AddExperience(_quality, _maxLevel);
        }

        public bool Matches(IActivity action)
        {
            if (action.Action != Activity.Learn)
            {
                return false;
            }
            LearnActivity learn = (LearnActivity)action;
            // TODO: add logic here once we flesh out learning
            return true;
        }

        public string Log()
        {
            return "Learning worth " + Desire.ToString("0.000");
        }
    }
}
