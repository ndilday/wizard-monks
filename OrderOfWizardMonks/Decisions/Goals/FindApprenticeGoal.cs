using System.Collections.Generic;
using WizardMonks.Decisions.Conditions.Helpers;
using WizardMonks.Models.Characters;

namespace WizardMonks.Decisions.Goals
{
    public class FindApprenticeGoal : AGoal
    {
        // This goal could be for the magus themself, or for another.
        // For now, we'll assume it's for the character pursuing the goal.
        public FindApprenticeGoal(Magus magus, double desire)
            : base(magus, magus.SeasonalAge + 20, desire) // Deadline: find one within 5 years.
        {
            // The desire for an apprentice increases with age and is influenced by sociability.
            Desire = desire * (magus.SeasonalAge / 100.0) * magus.Personality.GetDesireMultiplier(HexacoFacet.Sociability);
        }

        // The goal is complete once the magus has an apprentice.
        public override bool IsComplete() => ((Magus)Character).Apprentice != null;

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, Desires desires, IList<string> log)
        {
            if (IsComplete()) return;

            var magus = (Magus)Character;

            log.Add($"[Goal] Seeking an apprentice. Desire: {Desire:F2}");

            // The core logic for finding an apprentice is now fully encapsulated in the helper.
            // The desire function passes the goal's overall desire, weighted by the probability of success.
            CalculateDesireFunc desireFunc = (gain, depth) => (Desire / (AgeToCompleteBy - magus.SeasonalAge ?? 1)) * gain / depth;

            var findHelper = new FindApprenticeHelper(magus, (uint)AgeToCompleteBy, 1, desireFunc);
            findHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);
        }
    }
}