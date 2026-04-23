using System.Collections.Generic;
using WizardMonks.Models.Characters;

namespace WizardMonks.Decisions
{
    /// <summary>
    /// Produces new Intentions at the start of each tick based on the character's
    /// current emotional state, beliefs, and personality.
    /// </summary>
    public interface IGoalGenerator
    {
        /// <summary>
        /// Evaluates the character's current cognitive state and returns any new
        /// Intentions that should be added to their active intention list.
        ///
        /// Does not modify the active intention list directly — the simulation loop
        /// is responsible for merging new intentions and running reconsideration checks.
        /// </summary>
        IEnumerable<Intention> GenerateIntentions(
            Character character,
            EmotionLedger emotions,
            CharacterBeliefStore beliefs,
            IReadOnlyList<Intention> activeIntentions,
            int currentTick);
    }
}