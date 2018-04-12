using System;
using System.Collections.Generic;
using WizardMonks.Instances;

namespace WizardMonks.Decisions.Conditions
{
    public class HasLabCondition : ACondition
    {
        private Magus _mage;
        HasAuraCondition _auraCondition;
        public HasLabCondition(Magus magus, uint ageToCompleteBy, double desire, ushort conditionDepth = 1) : base(magus, ageToCompleteBy, desire, conditionDepth)
        {
            _mage = magus;
            _auraCondition = new HasAuraCondition(_mage, ageToCompleteBy - 1, desire, (ushort)(conditionDepth + 1));
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
                string startingLog = "Need to have a lab, desire " + Desire.ToString("0.0");
                log.Add(startingLog);
                if (!_auraCondition.ConditionFulfilled)
                {
                    _auraCondition.AddActionPreferencesToList(alreadyConsidered, log);
                }
                else
                {
                    BuildLaboratory buildLabAction = new BuildLaboratory(Abilities.MagicTheory, this.Desire / (TimeUntilDue * ConditionDepth));
                    alreadyConsidered.Add(buildLabAction);
                    log.Add(buildLabAction.Log());
                }
            }
        }
    }
}
