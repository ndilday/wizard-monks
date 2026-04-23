using System;
using System.Collections.Generic;
using WizardMonks.Models.Events;

namespace WizardMonks.Models.Characters
{
    /// <summary>
    /// An immutable, interpreted record of a world event as experienced by one character.
    ///
    /// MemoryEntries are produced exclusively by the AppraisalEngine. They are never
    /// constructed directly by simulation systems. The entry captures not only what
    /// happened, but what it meant to this character at the time it was recorded.
    /// </summary>
    public sealed class MemoryEntry
    {
        /// <summary>Unique identifier for this memory.</summary>
        public Guid Id { get; }

        /// <summary>The simulation tick at which this entry was recorded.</summary>
        public int Tick { get; }

        /// <summary>The originating world event.</summary>
        public WorldEvent SourceEvent { get; }

        /// <summary>
        /// How significant this event was to this character. Character-specific —
        /// the same world event can have a very different importance weight for
        /// different observers depending on their active goals and emotional state.
        /// </summary>
        public float ImportanceWeight { get; }

        /// <summary>
        /// IDs of the intentions that were active and relevant when this entry was recorded.
        /// Used during reflection to identify which goals this experience bears on.
        /// </summary>
        public IReadOnlyList<Guid> RelevantIntentionIds { get; }

        /// <summary>
        /// Emotion tokens generated at the moment of appraisal.
        /// These are snapshots — the live token state lives in the character's EmotionLedger.
        /// </summary>
        public IReadOnlyList<EmotionToken> EmotionSnapshot { get; }

        /// <summary>
        /// Whether this entry has been consumed by a reflection pass.
        /// Processed entries are candidates for expiration if importance is low.
        /// </summary>
        public bool IsProcessed { get; private set; }

        /// <summary>
        /// Tick at which this entry was last corroborated by a subsequent event.
        /// High-corroboration entries are retained longer during memory management.
        /// </summary>
        public int LastCorroboratedTick { get; private set; }

        public MemoryEntry(
            int tick,
            WorldEvent sourceEvent,
            float importanceWeight,
            IReadOnlyList<Guid> relevantIntentionIds,
            IReadOnlyList<EmotionToken> emotionSnapshot)
        {
            Id = Guid.NewGuid();
            Tick = tick;
            SourceEvent = sourceEvent;
            ImportanceWeight = importanceWeight;
            RelevantIntentionIds = relevantIntentionIds;
            EmotionSnapshot = emotionSnapshot;
            IsProcessed = false;
            LastCorroboratedTick = tick;
        }

        /// <summary>
        /// Marks this entry as having been processed by a reflection pass.
        /// Called by ReflectionEngine only.
        /// </summary>
        internal void MarkProcessed() => IsProcessed = true;

        /// <summary>
        /// Updates the corroboration timestamp. Called when a subsequent event
        /// reinforces the pattern this entry represents.
        /// </summary>
        internal void Corroborate(int tick) => LastCorroboratedTick = tick;
    }
}