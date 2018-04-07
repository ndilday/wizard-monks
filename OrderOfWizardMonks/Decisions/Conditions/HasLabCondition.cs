using System;
using System.Collections.Generic;
using WizardMonks.Instances;

namespace WizardMonks.Decisions.Conditions
{
    public class HasLabCondition : ACondition
    {
        private Magus _mage;
        public HasLabCondition(Magus magus, uint ageToCompleteBy, double desire, ushort conditionDepth = 1) : base(magus, ageToCompleteBy, desire, conditionDepth)
        {
            _mage = magus;
        }

        public override bool ConditionFulfilled
        {
            get
            {
                return _mage.Laboratory != null;
            }
        }


        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log)
        {
            if(!ConditionFulfilled)
            {
                HasAuraCondition auraCondition = new HasAuraCondition(_mage, this.AgeToCompleteBy, this.Desire, (ushort)(this.ConditionDepth + 1));
                if(!auraCondition.ConditionFulfilled)
                {
                    auraCondition.AddActionPreferencesToList(alreadyConsidered, log);
                }
                else
                {
                    BuildLaboratory buildLabAction = new BuildLaboratory(Abilities.MagicTheory, this.Desire / (AgeToCompleteBy - Character.SeasonalAge));
                    alreadyConsidered.Add(buildLabAction);
                    log.Add("Building a lab worth " + this.Desire.ToString("0.000"));
                }
            }
        }
    }
}
