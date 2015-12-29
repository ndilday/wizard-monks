using System;
using System.Collections.Generic;
using System.Linq;

using WizardMonks.Instances;

namespace WizardMonks.Decisions.Conditions
{
    public class VisCondition : ACondition
    {
        private Magus _magus;
        public List<Ability> VisTypes { get; private set; }
        public double AmountNeeded { get; private set; }

        public VisCondition(Magus magus, uint ageToCompleteBy, double desire, List<Ability> abilities, double totalNeeded, ushort conditionDepth) :
            base(magus, ageToCompleteBy, desire, conditionDepth)
        {
            _magus = magus;
            VisTypes = abilities;
            AmountNeeded = totalNeeded;
        }

        public VisCondition(Magus magus, uint ageToCompleteBy, double desire, Ability ability, double totalNeeded, ushort conditionDepth) :
            base(magus, ageToCompleteBy, desire, conditionDepth)
        {
            _magus = magus;
            VisTypes = new List<Ability>(1);
            VisTypes.Add(ability);
            AmountNeeded = totalNeeded;
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log)
        {
            // extract
            if(VisTypes.Contains(MagicArts.Vim))
            {
                if(!_magus.KnownAuras.Any())
                {
                    HasAuraCondition auraCondition = new HasAuraCondition(_magus, AgeToCompleteBy - 2, Desire, (ushort)(ConditionDepth + 2));
                    auraCondition.AddActionPreferencesToList(alreadyConsidered, log);
                }
                else if(_magus.Laboratory == null)
            }
            throw new NotImplementedException();
            
            // search for vis source
            // trade?
        }

        public override bool ConditionFulfilled
        {
            get
            {
                double total = 0;
                foreach(Ability ability in VisTypes)
                {
                    total += _magus.GetVisCount(ability);
                }
                return total >= AmountNeeded;
            }
        }
    }
}
