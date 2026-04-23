using System;
using System.Collections.Generic;
using WizardMonks.Models.Beliefs;

namespace WizardMonks.Models.Characters
{
    /// <summary>
    /// A character's accumulated beliefs about a single subject (another character,
    /// themselves, a covenant, etc.), organized by belief dimension.
    ///
    /// Self-beliefs use the same type with the owning character as the subject.
    /// This is the type that ReflectionEngine writes to and GoalGenerator reads from.
    ///
    /// This type coexists with the existing Models.Beliefs.BeliefProfile during the
    /// transition period. The existing type is retained for the prestige/reputation
    /// system until that system is migrated.
    /// </summary>
    public sealed class CharacterBeliefProfile
    {
        /// <summary>The subject these beliefs are about.</summary>
        public IBeliefSubject Subject { get; }

        private readonly Dictionary<string, BeliefEntry> _entries = new();

        public CharacterBeliefProfile(IBeliefSubject subject)
        {
            Subject = subject;
        }

        /// <summary>Returns the belief entry for a given dimension, or null if none exists.</summary>
        public BeliefEntry? Get(string dimension)
            => _entries.TryGetValue(dimension, out var entry) ? entry : null;

        /// <summary>Returns the belief value for a dimension, or 0.0 (neutral) if no belief exists.</summary>
        public float GetValue(string dimension)
            => Get(dimension)?.Value ?? 0f;

        /// <summary>Returns the confidence for a dimension, or 0.0 if no belief exists.</summary>
        public float GetConfidence(string dimension)
            => Get(dimension)?.Confidence ?? 0f;

        /// <summary>
        /// Upserts a belief entry. If a belief for this dimension already exists,
        /// its Update() method is called. Otherwise a new entry is created.
        /// Called by ReflectionEngine.
        /// </summary>
        public void Upsert(string dimension, float evidenceValue, float evidenceWeight, int tick)
        {
            if (_entries.TryGetValue(dimension, out var existing))
            {
                existing.Update(evidenceValue, evidenceWeight, tick);
            }
            else
            {
                _entries[dimension] = new BeliefEntry(
                    dimension,
                    value: evidenceValue * evidenceWeight,
                    confidence: evidenceWeight * 0.1f,
                    tick: tick);
            }
        }

        /// <summary>Returns all current belief entries. Used by reflection and goal generation.</summary>
        public IReadOnlyDictionary<string, BeliefEntry> All => _entries;

        /// <summary>
        /// Applies confidence decay across all entries. Called periodically by the
        /// simulation loop for beliefs that have not received recent corroboration.
        /// </summary>
        public void DecayAllConfidence(float decayAmount)
        {
            foreach (var entry in _entries.Values)
                entry.DecayConfidence(decayAmount);
        }
    }
}