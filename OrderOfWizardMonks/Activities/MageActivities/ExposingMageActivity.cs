
using WizardMonks.Models.Characters;

namespace WizardMonks.Activities.MageActivities
{
    public abstract class AExposingMageActivity : AMageActivity
    {
        private readonly Ability _exposure;

        protected AExposingMageActivity(Ability exposure, double desire)
        {
            _exposure = exposure;
            Desire = desire;
        }

        public override sealed void Act(Character character)
        {
            DoMageAction(ConfirmCharacterIsMage(character));
            character.GetAbility(_exposure).AddExperience(2);
        }

        protected abstract void DoMageAction(Magus mage);
    }

}
