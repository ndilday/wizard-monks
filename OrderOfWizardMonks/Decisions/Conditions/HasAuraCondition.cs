using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks;
using WizardMonks.Instances;

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
                return Character.KnownAuras != null && Character.KnownAuras.Count > 0;
            }
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log)
        {
            if(!ConditionFulfilled)
            {
                FindAura findAuraAction = new FindAura(Abilities.AreaLore, this.Desire / (AgeToCompleteBy - Character.SeasonalAge));
                alreadyConsidered.Add(findAuraAction);
                log.Add("Finding an aura worth " + this.Desire.ToString("0.000"));
            }
        }
    }
}
