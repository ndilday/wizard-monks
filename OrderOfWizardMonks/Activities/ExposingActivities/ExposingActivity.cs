
namespace WizardMonks.Activities.ExposingActivities
{
    public abstract class AExposingActivity(Ability exposure, double desire) : IActivity
    {
        public Ability Exposure { get; private set; } = exposure;

        public Activity Action { get; protected set; }

        public double Desire { get; set; } = desire;

        public void Act(Character character)
        {
            DoAction(character);
            character.GetAbility(Exposure).AddExperience(2);
        }

        protected abstract void DoAction(Character character);

        public abstract bool Matches(IActivity action);

        public abstract string Log();
    }

}
