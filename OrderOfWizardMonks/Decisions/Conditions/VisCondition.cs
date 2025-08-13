using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Activities;
using WizardMonks.Activities.MageActivities;
using WizardMonks.Decisions.Conditions.Helpers;
using WizardMonks.Economy;
using WizardMonks.Instances;
using WizardMonks.Models.Characters;

namespace WizardMonks.Decisions.Conditions
{
    public class VisCondition : ACondition
    {
        private Magus _mage;
        private bool _vimSufficient;
        HasLabCondition _labCondition;
        HasAuraCondition _auraCondition;
        double _visStillNeeded;

        public List<Ability> VisTypes { get; private set; }
        public double AmountNeeded { get; private set; }

        public VisCondition(Magus magus, uint ageToCompleteBy, double desire, List<Ability> abilities, double totalNeeded, ushort conditionDepth) :
            base(magus, ageToCompleteBy, desire, conditionDepth)
        {
            _mage = magus;
            VisTypes = abilities;
            AmountNeeded = totalNeeded;
            _auraCondition = new HasAuraCondition(_mage, AgeToCompleteBy - 2, Desire, (ushort)(ConditionDepth + 2));
            _labCondition = new HasLabCondition(_mage, AgeToCompleteBy - 1, Desire, (ushort)(ConditionDepth + 1));
            _vimSufficient = VisTypes.Contains(MagicArts.Vim);
        }

        public VisCondition(Magus magus, uint ageToCompleteBy, double desire, Ability ability, double totalNeeded, ushort conditionDepth) :
            base(magus, ageToCompleteBy, desire, conditionDepth)
        {
            _mage = magus;
            VisTypes = new List<Ability>(1);
            VisTypes.Add(ability);
            AmountNeeded = totalNeeded;
            _auraCondition = new HasAuraCondition(_mage, AgeToCompleteBy - 2, Desire, (ushort)(ConditionDepth + 2));
            _labCondition = new HasLabCondition(_mage, AgeToCompleteBy - 1, Desire, (ushort)(ConditionDepth + 2));
            _vimSufficient = VisTypes.Contains(MagicArts.Vim);
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, Desires desires, IList<string> log)
        {
            double storedVis = VisTypes.Sum(v => _mage.GetVisCount(v));
            _visStillNeeded = AmountNeeded - storedVis;

            if (_visStillNeeded > 0)
            {
                foreach (Ability visType in this.VisTypes)
                {
                    desires.VisDesires.First(d => d.Art == visType).Quantity += _visStillNeeded;
                }

                // extract
                if (_vimSufficient)
                {
                    if (!_auraCondition.ConditionFulfilled)
                    {
                        _auraCondition.AddActionPreferencesToList(alreadyConsidered, desires, log);
                    }
                    else if (!_labCondition.ConditionFulfilled)
                    {
                        _labCondition.AddActionPreferencesToList(alreadyConsidered, desires, log);
                    }
                    else
                    {
                        double currentDistillRate = _mage.GetVisDistillationRate();
                        double extractDesirability = GetDesirabilityOfVisGain(currentDistillRate, ConditionDepth);
                        if (extractDesirability > 0.00001)
                        {
                            // we can get what we want in one season, go ahead and do it
                            log.Add("Extracting vis worth " + extractDesirability.ToString("0.000"));
                            alreadyConsidered.Add(new ExtractVisActivity(Abilities.MagicTheory, extractDesirability));

                            if (currentDistillRate < _visStillNeeded && AgeToCompleteBy -1 > _mage.SeasonalAge)
                            {
                                // we are in the multi-season-to-fulfill scenario

                                // the difference between the desire of starting now
                                // and the desire of starting after gaining experience
                                // is the effective value of raising skills
                                double labTotal = _mage.GetLabTotal(MagicArtPairs.CrVi, Activity.DistillVis);
                                LabTotalIncreaseHelper helper = new(
                                    _mage, 
                                    AgeToCompleteBy - 1, 
                                    (ushort)(ConditionDepth + 1), 
                                    MagicArtPairs.CrVi, 
                                    Activity.DistillVis,
                                    GetDesirabilityOfLabTotalGain);
                                helper.AddActionPreferencesToList(alreadyConsidered, desires, log);
                            }
                        }
                    }
                }
                // search for vis source
                FindVisSourceHelper visSourceHelper = new(_mage, VisTypes, AgeToCompleteBy - 1, (ushort)(ConditionDepth + 1), GetDesirabilityOfVisGain);
                visSourceHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);

                // consider writing a book to trade for vis
                WritingHelper writingHelper = new(_mage, AgeToCompleteBy - 1, (ushort)(ConditionDepth + 1), GetDesirabilityOfVisGain);
                writingHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);
            }
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

        private double GetDesirabilityOfVisGain(double visGain, ushort conditionDepth)
        {
            double proportion = visGain / _visStillNeeded;
            double immediateDesire = Desire / (AgeToCompleteBy - Character.SeasonalAge);
            return immediateDesire * proportion / conditionDepth;
        }

        private double GetDesirabilityOfLabTotalGain(double gain, ushort conditionDepth)
        {
            double proportion = gain / 10.0 / _visStillNeeded;
            double immediateDesire = Desire / (AgeToCompleteBy - Character.SeasonalAge);
            return immediateDesire * proportion / conditionDepth;
        }
    }
}
