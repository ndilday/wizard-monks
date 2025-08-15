using System.Collections.Generic;
using System.Linq;
using WizardMonks.Activities.MageActivities;
using WizardMonks.Instances;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Covenants;
using WizardMonks.Services.Characters;

namespace WizardMonks.Decisions.Conditions
{
    public class IsCovenantFounderCondition : ACondition
    {
        private Magus _mage;
        public IsCovenantFounderCondition(Magus magus, uint ageToCompleteBy, double desire, ushort conditionDepth = 1) : base(magus, ageToCompleteBy, desire, conditionDepth)
        {
            _mage = magus;
        }

        public override bool ConditionFulfilled
        {
            get
            {
                return _mage.Covenant != null && _mage.Covenant.GetRoleForMagus(_mage) == CovenantRole.Founder;
            }
        }


        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, Desires desires, IList<string> log)
        {
            if (!ConditionFulfilled)
            {
                if (_mage.Covenant != null)
                {
                    _mage.LeaveCovenant();

                }
                HasAuraCondition auraCondition = new(_mage, this.AgeToCompleteBy, this.Desire, (ushort)(this.ConditionDepth + 1));
                if (!auraCondition.ConditionFulfilled)
                {
                    auraCondition.AddActionPreferencesToList(alreadyConsidered, desires, log);
                }
                else
                {
                    _mage.FoundCovenant(_mage.KnownAuras.OrderByDescending(a => a.Strength).First());
                    BuildLaboratoryActivity buildLabAction = new(Abilities.MagicTheory, this.Desire / (AgeToCompleteBy - Character.SeasonalAge));
                    alreadyConsidered.Add(buildLabAction);
                    log.Add("Building a lab worth " + this.Desire.ToString("0.000"));
                }
            }
        }
    }
}
