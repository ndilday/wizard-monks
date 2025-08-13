using System;
using WizardMonks.Instances;
using WizardMonks.Models.Characters;
using WizardMonks.Services.Characters;

namespace WizardMonks.Activities.ExposingActivities
{
    [Serializable]
    public class Teach : AExposingActivity
    {
        public bool Completed { get; private set; }
        // TODO: enable multiple students
        public Character Student { get; private set; }
        public Ability Topic { get; private set; }
        public Teach(Character student, Ability abilityToTeach, Ability exposure, double desire) : base(exposure, desire)
        {
            Action = Activity.Teach;
            Student = student;
            Topic = abilityToTeach;
            Completed = false;
        }

        protected override void DoAction(Character character)
        {
            double experienceDifference = character.GetAbility(Topic).Experience - Student.GetAbility(Topic).Experience;
            if (experienceDifference <= 0)
            {
                throw new ArgumentOutOfRangeException("Teacher has nothing to teach this student!");
            }
            double quality = character.GetAbility(Abilities.Teaching).Value + character.GetAttributeValue(AttributeType.Communication) + 6;
            Student.Advance(new LearnActivity(quality, character.GetAbility(Topic).Value, Topic, character));
            Completed = true;
        }

        public override bool Matches(IActivity action)
        {
            if (action.Action != Activity.Teach)
            {
                return false;
            }
            Teach teach = (Teach)action;
            return teach.Student == Student && teach.Topic == Topic;
        }

        public override string Log()
        {
            return "Teaching worth " + Desire.ToString("0.000");
        }
    }

}
