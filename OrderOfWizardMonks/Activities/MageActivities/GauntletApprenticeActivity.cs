
namespace WizardMonks.Activities.MageActivities
{
    public class GauntletApprentice(Ability exposure, double desire) : AExposingMageActivity(exposure, desire)
    {
        public override bool Matches(IActivity action)
        {
            return action.Action == Activity.GauntletApprentice;
        }

        public override string Log()
        {
            return "Gauntleting apprentice worth " + Desire.ToString("0.000");
        }

        protected override void DoMageAction(Magus mage)
        {
            mage.GauntletApprentice();
        }
    }

}
