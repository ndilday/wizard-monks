using System;
using System.Collections.Generic;
using WizardMonks.Decisions.Conditions.Helpers;
using WizardMonks.Economy;
using WizardMonks.Models.Characters;
using WizardMonks.Services.Characters;

namespace WizardMonks.Decisions.Goals
{
    /// <summary>
    /// Pragmatic goal: write books and lab texts that record the mage's knowledge.
    /// Delegates entirely to WritingHelper to decide what is worth writing and how.
    /// </summary>
    public class WriteTextGoal : AGoal
    {
        private readonly HermeticMagus _mage;

        public WriteTextGoal(HermeticMagus mage, double desire)
            : base(mage, null, desire)
        {
            _mage = mage;
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, Desires desires, IList<string> log)
        {
            double distillRate = _mage.GetVisDistillationRate();

            // Scale writing desire by the economic value of what's produced, expressed
            // in units of vis distillation seasons so it's commensurable with other activities.
            CalculateDesireFunc desireFunc = (value, depth) =>
                Desire * (value / Math.Max(0.01, distillRate));

            uint horizon = (uint)(_mage.SeasonalAge + 40);
            var writingHelper = new WritingHelper(_mage, horizon, 1, desireFunc);
            writingHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);
        }

        public override bool IsComplete() => false;
    }
}
