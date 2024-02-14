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

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log)
        {
            double storedVis = VisTypes.Sum(v => _mage.GetVisCount(v));
            _visStillNeeded = AmountNeeded - storedVis;
            if (_visStillNeeded > 0)
            {
                // extract
                if (_vimSufficient)
                {
                    if (!_auraCondition.ConditionFulfilled)
                    {
                        _auraCondition.AddActionPreferencesToList(alreadyConsidered, log);
                    }
                    else if (!_labCondition.ConditionFulfilled)
                    {
                        _labCondition.AddActionPreferencesToList(alreadyConsidered, log);
                    }
                    else
                    {
                        double currentDistillRate = _mage.GetVisDistillationRate();
                        double extractDesirability = GetDesirabilityOfVisGain(currentDistillRate, ConditionDepth);
                        if (extractDesirability > 0.00001)
                        {
                            // we can get what we want in one season, go ahead and do it
                            log.Add("Extracting vis worth " + extractDesirability.ToString("0.000"));
                            alreadyConsidered.Add(new VisExtracting(Abilities.MagicTheory, extractDesirability));

                            if (currentDistillRate < _visStillNeeded)
                            {
                                // we are in the multi-season-to-fulfill scenario

                                // the difference between the desire of starting now
                                // and the desire of starting after gaining experience
                                // is the effective value of raising skills
                                double labTotal = _mage.GetLabTotal(MagicArtPairs.CrVi, Activity.DistillVis);
                                LabTotalIncreaseHelper helper =
                                    new LabTotalIncreaseHelper(_mage, AgeToCompleteBy - 1, extractDesirability / labTotal, (ushort)(ConditionDepth + 1), MagicArtPairs.CrVi, false, GetDesirabilityOfLabTotalGain);
                                helper.AddActionPreferencesToList(alreadyConsidered, log);
                            }
                        }
                    }
                }
                // search for vis source
                FindVisSourceHelper visSourceHelper = new FindVisSourceHelper(_mage, VisTypes, AgeToCompleteBy - 1, Desire, (ushort)(ConditionDepth + 1), !_vimSufficient, GetDesirabilityOfVisGain);
                visSourceHelper.AddActionPreferencesToList(alreadyConsidered, log);

                // consider writing a book to trade for vis
                WritingHelper writingHelper = new WritingHelper(_mage, AgeToCompleteBy - 1, Desire, (ushort)(ConditionDepth + 1), GetDesirabilityOfVisGain);
                writingHelper.AddActionPreferencesToList(alreadyConsidered, log);
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

        public override void ModifyVisDesires(VisDesire[] desires)
        {
            foreach(Ability visType in this.VisTypes)
            {
                desires.First(d => d.Art == visType).Quantity += this.AmountNeeded;
            }
        }

        public override List<BookDesire> GetBookDesires()
        {
            List<BookDesire> bookDesires = new List<BookDesire>();
            if (!ConditionFulfilled)
            {
                // books won't help us get the vis we want, will they?
                /*foreach (Ability visType in this.VisTypes)
                {
                    bookDesires.Add(new BookDesire(visType, this.Character.GetAbility(visType).Value));
                }*/
                // add an interest in MagicLore here, just to have it somewhere?
                bookDesires.Add(new BookDesire(Abilities.MagicLore, this.Character.GetAbility(Abilities.MagicLore).Value));
            }
            return bookDesires;
        }

        private double GetDesirabilityOfVisGain(double visGain, ushort conditionDepth)
        {
            double proportion = visGain / _visStillNeeded;
            double immediateDesire = Desire / (AgeToCompleteBy - Character.SeasonalAge);
            return immediateDesire * proportion / conditionDepth;
        }

        private double GetDesirabilityOfLabTotalGain(double gain, ushort conditionDepth)
        {
            double proportion = gain / _visStillNeeded;
            double immediateDesire = Desire / (AgeToCompleteBy - Character.SeasonalAge);
            return immediateDesire * proportion / conditionDepth;
        }
    }
}
