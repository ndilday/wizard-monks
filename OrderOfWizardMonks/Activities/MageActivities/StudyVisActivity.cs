using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WizardMonks.Core;
using WizardMonks.Instances;

namespace WizardMonks.Activities.MageActivities
{
    [Serializable]
    public class StudyVisActivity : AMageActivity
    {
        public Ability Art { get; private set; }

        public StudyVisActivity(Ability art, double desire)
        {
            if (!MagicArts.IsArt(art))
            {
                throw new ArgumentException("Only magic arts have vis associated with them!");
            }
            Art = art;
            Desire = desire;
            Action = Activity.StudyVis;
        }

        public override void Act(Character character)
        {
            Magus mage = ConfirmCharacterIsMage(character);

            // determine the amount of vis needed
            CharacterAbilityBase charAbility = mage.GetAbility(Art);
            double visNeeded = 0.5 + charAbility.Value / 10.0;

            // decrement the used vis
            mage.UseVis(Art, visNeeded);

            // add experience
            ushort roll = Die.Instance.RollExplodingDie();
            double aura = mage.Covenant != null && mage.Covenant.Aura != null ? mage.Covenant.Aura.Strength : 0;
            double gain = roll + aura;
            character.Log.Add("Studying " + visNeeded.ToString("0.000") + " pawns of " + Art.AbilityName + " vis.");
            character.Log.Add("Gained " + gain + " experience.");
            charAbility.AddExperience(gain);
        }

        public override bool Matches(IActivity action)
        {
            if (action.Action != Activity.StudyVis)
            {
                return false;
            }
            StudyVisActivity study = (StudyVisActivity)action;
            return study.Art == Art;
        }

        public override string Log()
        {
            return "Studying " + Art.AbilityName + " vis worth " + Desire.ToString("0.000");
        }
    }

}
