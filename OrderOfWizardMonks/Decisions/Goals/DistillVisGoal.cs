using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Activities.MageActivities;
using WizardMonks.Decisions.Conditions.Helpers;
using WizardMonks.Economy;
using WizardMonks.Instances;
using WizardMonks.Models.Characters;
using WizardMonks.Services.Characters;

namespace WizardMonks.Decisions.Goals
{
    /// <summary>
    /// Pragmatic goal: build vis reserves up to a prudent stockpile level.
    /// Recommends distillation directly, and delegates to FindVisSourceHelper
    /// to surface improvements to vis access (better sources, better aura) as
    /// assistive actions.
    /// </summary>
    public class DistillVisGoal : AGoal
    {
        private readonly HermeticMagus _mage;

        public DistillVisGoal(HermeticMagus mage, double desire)
            : base(mage, null, desire)
        {
            _mage = mage;
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, Desires desires, IList<string> log)
        {
            if (_mage.Covenant == null) return;

            double distillRate = _mage.GetVisDistillationRate();
            if (distillRate <= 0) return;

            log.Add($"[DistillVisGoal] Extracting vis worth {Desire:0.000}");
            alreadyConsidered.Add(new ExtractVisActivity(Abilities.MagicTheory, Desire));

            // Each additional pawn per season is worth as much as one season's worth of
            // current distillation effort relative to this goal's desire.
            double currentRate = distillRate;
            CalculateDesireFunc desireFunc = (gain, depth) =>
                Desire * (gain / Math.Max(0.01, currentRate));

            uint horizon = (uint)(_mage.SeasonalAge + 40);
            var visTypes = MagicArts.GetEnumerator().ToList();
            var findVisHelper = new FindVisSourceHelper(_mage, visTypes, horizon, 1, desireFunc);
            findVisHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);
        }

        public override bool IsComplete() => false;
    }
}
