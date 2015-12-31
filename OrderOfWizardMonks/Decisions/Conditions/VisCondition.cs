using System;
using System.Collections.Generic;
using System.Linq;

using WizardMonks.Decisions.Conditions.Helpers;
using WizardMonks.Instances;

namespace WizardMonks.Decisions.Conditions
{
    public class VisCondition : ACondition
    {
        private Magus _mage;
        private bool _vimSufficient;
        HasLabCondition _labCondition;
        HasAuraCondition _auraCondition;
        public List<Ability> VisTypes { get; private set; }
        public double AmountNeeded { get; private set; }

        public VisCondition(Magus magus, uint ageToCompleteBy, double desire, List<Ability> abilities, double totalNeeded, ushort conditionDepth) :
            base(magus, ageToCompleteBy, desire, conditionDepth)
        {
            _mage = magus;
            VisTypes = abilities;
            AmountNeeded = totalNeeded;
            HasAuraCondition auraCondition = new HasAuraCondition(_mage, AgeToCompleteBy - 2, Desire, (ushort)(ConditionDepth + 2));
            HasLabCondition labCondition = new HasLabCondition(_mage, AgeToCompleteBy - 1, Desire, (ushort)(ConditionDepth + 1));
            _vimSufficient = VisTypes.Contains(MagicArts.Vim);
        }

        public VisCondition(Magus magus, uint ageToCompleteBy, double desire, Ability ability, double totalNeeded, ushort conditionDepth) :
            base(magus, ageToCompleteBy, desire, conditionDepth)
        {
            _mage = magus;
            VisTypes = new List<Ability>(1);
            VisTypes.Add(ability);
            AmountNeeded = totalNeeded;
            HasAuraCondition auraCondition = new HasAuraCondition(_mage, AgeToCompleteBy - 2, Desire, (ushort)(ConditionDepth + 2));
            HasLabCondition labCondition = new HasLabCondition(_mage, AgeToCompleteBy - 1, Desire, (ushort)(ConditionDepth + 2));
            _vimSufficient = VisTypes.Contains(MagicArts.Vim);
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log)
        {
            double storedVis = VisTypes.Sum(v => _mage.GetVisCount(v));
            double visStillNeeded = AmountNeeded - storedVis;
            // extract
            if(_vimSufficient)
            {
                if(!_mage.KnownAuras.Any())
                {
                    _auraCondition.AddActionPreferencesToList(alreadyConsidered, log);
                }
                else if(_mage.Laboratory == null)
                {
                    _labCondition.AddActionPreferencesToList(alreadyConsidered, log);
                }
                else
                {
                    double labTotal = _mage.GetLabTotal(MagicArtPairs.CrVi, Activity.DistillVis);
                    double currentDistillRate = labTotal / 10;
                    double extractDesirability = GetDesirabilityOfVisGain(currentDistillRate, visStillNeeded);
                    if (!double.IsNaN(extractDesirability) && extractDesirability > 0)
                    {
                        // we can get what we want in one season, go ahead and do it
                        log.Add("Extracting vis worth " + (extractDesirability).ToString("0.00"));
                        alreadyConsidered.Add(new VisExtracting(Abilities.MagicTheory, extractDesirability));

                        if (currentDistillRate < visStillNeeded)
                        {
                            // we are in the multi-season-to-fulfill scenario

                            // the difference between the desire of starting now
                            // and the desire of starting after gaining experience
                            // is the effective value of raising skills
                            LabTotalIncreaseHelper helper = 
                                new LabTotalIncreaseHelper(_mage, AgeToCompleteBy - 1, extractDesirability / labTotal, (ushort)(ConditionDepth + 1), MagicArtPairs.CrVi);
                            //helper.ModifyActionList(_mage, alreadyConsidered, log);
                        }
                    }
                }
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
                    total += _mage.GetVisCount(ability);
                }
                return total >= AmountNeeded;
            }
        }

        private double GetDesirabilityOfVisGain(double visGain, double visStillNeeded)
        {
            double proportion = visGain / visStillNeeded;
            double immediateDesire = Desire / (AgeToCompleteBy - Character.SeasonalAge);
            return immediateDesire * proportion / ConditionDepth;
        }
    }
}
