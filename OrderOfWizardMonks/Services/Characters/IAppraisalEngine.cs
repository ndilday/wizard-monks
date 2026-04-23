using System.Collections.Generic;
using WizardMonks.Decisions;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Events;

namespace WizardMonks.Services.Characters
{
    /// <summary>
    /// Evaluates a world event for a specific character and produces a MemoryEntry.
    ///
    /// The AppraisalEngine is stateless. All character-specific context is passed per call.
    /// Personality does not modulate emotion intensity or decay rates — those are determined
    /// by event relevance to active goals and beliefs, and by emotion type respectively.
    /// Personality's influence on emotional life is expressed in the GoalGenerator, through
    /// generation thresholds and commitment strength.
    /// </summary>
    public interface IAppraisalEngine
    {
        /// <summary>
        /// Appraises a world event from this character's perspective.
        ///
        /// Returns null if the event is not meaningful to this character
        /// (below a minimum importance threshold), in which case no entry should
        /// be recorded.
        /// </summary>
        MemoryEntry? Appraise(
            Character character,
            WorldEvent worldEvent,
            IReadOnlyList<Intention> activeIntentions,
            EmotionLedger currentLedger);
    }
}