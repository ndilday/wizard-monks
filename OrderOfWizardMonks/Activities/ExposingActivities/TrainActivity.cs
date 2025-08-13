using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WizardMonks.Models.Characters;

namespace WizardMonks.Activities.ExposingActivities
{
    [Serializable]
    public class Train : AExposingActivity
    {
        // TODO: enable multiple students
        public Character Student { get; private set; }
        public Ability AbilityToTrain { get; private set; }
        public Train(Character student, Ability abilityToTrain, Ability exposure, double desire) : base(exposure, desire)
        {
            Action = Activity.Train;
            Student = student;
            AbilityToTrain = abilityToTrain;
        }

        protected override void DoAction(Character character)
        {
            double abilityDifference = character.GetAbility(AbilityToTrain).Value - Student.GetAbility(AbilityToTrain).Value;
            if (abilityDifference <= 0)
            {
                throw new ArgumentOutOfRangeException("Trainer has nothing to teach this student!");
            }
            double amountTrained = 3 + character.GetAbility(AbilityToTrain).Value;
            if (amountTrained > abilityDifference)
            {
                amountTrained = abilityDifference;
            }
            Student.GetAbility(AbilityToTrain).AddExperience(amountTrained);
        }

        public override bool Matches(IActivity action)
        {
            if (action.Action != Activity.Train)
            {
                return false;
            }
            Train train = (Train)action;
            return train.AbilityToTrain == AbilityToTrain && train.Student == Student;
        }

        public override string Log()
        {
            return "Training worth " + Desire.ToString("0.000");
        }
    }

}
