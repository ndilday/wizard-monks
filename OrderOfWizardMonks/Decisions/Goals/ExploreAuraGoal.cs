using System.Collections.Generic;
using WizardMonks.Decisions.Conditions.Helpers;
using WizardMonks.Economy;
using WizardMonks.Models.Characters;

namespace WizardMonks.Decisions.Goals
{
    /// <summary>
    /// Pragmatic goal: search for stronger auras or new vis sources in the surrounding area.
    /// Delegates to FindNewAuraHelper, which cascades into area lore improvement,
    /// InVi casting total improvements, and spell learning as assistive actions.
    /// </summary>
    public class ExploreAuraGoal : AGoal
    {
        private readonly HermeticMagus _mage;

        public ExploreAuraGoal(HermeticMagus mage, double desire)
            : base(mage, null, desire)
        {
            _mage = mage;
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, Desires desires, IList<string> log)
        {
            // Aura gain is measured in aura strength points; scale linearly against goal desire.
            CalculateDesireFunc desireFunc = (gain, depth) => Desire * gain;

            uint horizon = (uint)(_mage.SeasonalAge + 40);
            var auraHelper = new FindNewAuraHelper(_mage, horizon, 1, desireFunc);
            auraHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);
        }

        public override bool IsComplete() => false;
    }
}
