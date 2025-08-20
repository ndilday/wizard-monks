using System.Collections.Generic;
using System.Linq;
using WizardMonks.Activities.ExposingActivities;
using WizardMonks.Instances;
using WizardMonks.Models.Characters;
using WizardMonks.Services.Characters;

namespace WizardMonks.Decisions.Conditions
{
    class HasAuraCondition : ACondition
    {
        public HasAuraCondition(Character character, uint ageToCompleteBy, double desire, ushort conditionDepth = 1) : base(character, ageToCompleteBy, desire, conditionDepth)
        {

        }

        public override bool ConditionFulfilled
        {
            get
            {
                return Character.GetOwnedAuras().Count() > 0;
            }
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, Desires desires, IList<string> log)
        {
            if(!ConditionFulfilled)
            {
                FindAuraActivity findAuraAction = new(Abilities.AreaLore, this.Desire / (AgeToCompleteBy - Character.SeasonalAge));
                alreadyConsidered.Add(findAuraAction);
                log.Add("Finding an aura worth " + this.Desire.ToString("0.000"));
            }
        }
    }
}
