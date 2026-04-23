using System;
using System.Collections.Generic;
using System.Linq;

namespace WizardMonks.Models.Characters
{
    /// <summary>
    /// Append-only log of interpreted memory entries for a single character.
    ///
    /// The stream is bounded by importance-weighted forgetting. Low-importance,
    /// processed entries become candidates for expiration after a configurable
    /// number of ticks. High-importance entries are retained indefinitely.
    /// </summary>
    public sealed class CharacterMemoryStream
    {
        private readonly List<MemoryEntry> _entries = new();

        /// <summary>
        /// Accumulated importance of entries that have not yet been processed
        /// by a reflection pass. Used by threshold-triggered reflection policy.
        /// </summary>
        public float AccumulatedUnprocessedImportance { get; private set; }

        /// <summary>Minimum importance weight below which an entry is a forgetting candidate.</summary>
        public float ForgettingThreshold { get; }

        /// <summary>
        /// Number of ticks after processing before a low-importance entry is
        /// eligible for expiration.
        /// </summary>
        public int ExpirationAgeTicks { get; }

        public CharacterMemoryStream(float forgettingThreshold = 0.25f, int expirationAgeTicks = 20)
        {
            ForgettingThreshold = forgettingThreshold;
            ExpirationAgeTicks = expirationAgeTicks;
        }

        /// <summary>Appends a new entry. Never mutates existing entries.</summary>
        public void Append(MemoryEntry entry)
        {
            _entries.Add(entry);
            if (!entry.IsProcessed)
                AccumulatedUnprocessedImportance += entry.ImportanceWeight;
        }

        /// <summary>Returns all unprocessed entries in chronological order.</summary>
        public IEnumerable<MemoryEntry> GetUnprocessed()
            => _entries.Where(e => !e.IsProcessed);

        /// <summary>Returns all entries, processed and unprocessed.</summary>
        public IReadOnlyList<MemoryEntry> All => _entries.AsReadOnly();

        /// <summary>
        /// Marks an entry as processed and reduces the accumulated unprocessed importance.
        /// Called by ReflectionEngine.
        /// </summary>
        public void MarkProcessed(Guid entryId)
        {
            var entry = _entries.FirstOrDefault(e => e.Id == entryId);
            if (entry is null || entry.IsProcessed) return;

            entry.MarkProcessed();
            AccumulatedUnprocessedImportance =
                Math.Max(0f, AccumulatedUnprocessedImportance - entry.ImportanceWeight);
        }

        /// <summary>
        /// Removes entries that are processed, low-importance, and older than
        /// ExpirationAgeTicks since their last corroboration. Called once per tick
        /// by the simulation loop after reflection.
        /// </summary>
        public void ExpireStaleEntries(int currentTick)
        {
            _entries.RemoveAll(e =>
                e.IsProcessed &&
                e.ImportanceWeight < ForgettingThreshold &&
                (currentTick - e.LastCorroboratedTick) > ExpirationAgeTicks);
        }

        /// <summary>
        /// Marks a prior entry as corroborated by a subsequent event.
        /// Resets its expiration clock.
        /// </summary>
        public void Corroborate(Guid entryId, int currentTick)
        {
            var entry = _entries.FirstOrDefault(e => e.Id == entryId);
            entry?.Corroborate(currentTick);
        }
    }
}