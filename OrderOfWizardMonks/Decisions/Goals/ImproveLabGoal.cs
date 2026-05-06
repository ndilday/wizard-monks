using System.Collections.Generic;
using WizardMonks.Activities.MageActivities;
using WizardMonks.Economy;
using WizardMonks.Instances;
using WizardMonks.Models.Characters;

namespace WizardMonks.Decisions.Goals
{
    /// <summary>
    /// Pragmatic goal: keep the laboratory in the best shape possible.
    /// Desire is supplied by PragmaticGoalGenerator (already personality-weighted),
    /// so the constructor stores it as-is.
    /// </summary>
    public class ImproveLabGoal : AGoal
    {
        private readonly HermeticMagus _mage;

        public ImproveLabGoal(HermeticMagus mage, double desire)
            : base(mage, null, desire)
        {
            _mage = mage;
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, Desires desires, IList<string> log)
        {
            if (_mage.Laboratory == null) return;

            log.Add($"[ImproveLabGoal] Refining laboratory worth {Desire:0.000}");
            alreadyConsidered.Add(new RefineLaboratoryActivity(Abilities.MagicTheory, Desire));
        }

        // Refinement always has potential; stagnation tolerance handles retirement.
        public override bool IsComplete() => false;
    }
}
