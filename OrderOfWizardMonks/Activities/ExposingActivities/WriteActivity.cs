using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WizardMonks.Models;

namespace WizardMonks.Activities.ExposingActivities
{
    [Serializable]
    public class WriteActivity : AExposingActivity
    {
        public Ability Topic { get; private set; }
        public double Level { get; private set; }
        public string Name { get; private set; }

        public WriteActivity(Ability topic, string name, Ability exposure, double level, double desire)
            : base(exposure, desire)
        {
            Topic = topic;
            Level = level;
            Action = Activity.WriteBook;
            Name = name;
        }

        protected override void DoAction(Character character)
        {
            ABook book = character.WriteBook(Topic, Name, Exposure, Level);
            if (book != null)
            {
                if (Level == 0)
                {
                    character.Log.Add("Wrote " + book.Title + ": Q" + book.Quality.ToString("0.0"));
                }
                else
                {
                    character.Log.Add("Wrote " + book.Title + ": Q" + book.Quality.ToString("0.0") + ", L" + book.Level.ToString("0.0"));
                }

            }
            else
            {
                character.Log.Add("Worked on " + Name);
            }
        }

        public override bool Matches(IActivity action)
        {
            if (action.Action != Activity.WriteBook)
            {
                return false;
            }
            WriteActivity writing = (WriteActivity)action;
            return writing.Topic == Topic && writing.Level == Level;
        }

        public override string Log()
        {
            if (Level == 1000)
            {
                return "Writing tractatus on " + Topic.AbilityName + " worth " + Desire.ToString("0.000");
            }
            return "Writing summa on " + Topic.AbilityName + " worth " + Desire.ToString("0.000");
        }
    }

}
