using System.Collections.Generic;
using System.Linq;

namespace WizardMonks.Models.Characters
{
    /// <summary>
    /// Maintains the set of currently active emotion tokens for one character.
    /// One token per EmotionType maximum. Decay is applied at the start of each tick.
    /// </summary>
    public sealed class EmotionLedger
    {
        private readonly Dictionary<EmotionType, EmotionToken> _active = new();

        /// <summary>
        /// Applies one tick of decay to all active tokens.
        /// Removes tokens whose intensity has dropped below the expiry threshold.
        /// Call this at the start of each tick before appraisal.
        /// </summary>
        public void Tick()
        {
            var expired = _active
                .Where(kv => !kv.Value.Decay())
                .Select(kv => kv.Key)
                .ToList();

            foreach (var key in expired)
                _active.Remove(key);
        }

        /// <summary>
        /// Adds a new emotion token. If a token of the same type is already active,
        /// the new intensity is added to the existing token (clamped to 1.0).
        /// </summary>
        public void Add(EmotionToken token)
        {
            if (_active.TryGetValue(token.Type, out var existing))
                existing.Reinforce(token.Intensity, token.OriginTick);
            else
                _active[token.Type] = token;
        }

        /// <summary>Returns the current intensity of a token type, or 0 if not active.</summary>
        public float GetIntensity(EmotionType type)
            => _active.TryGetValue(type, out var token) ? token.Intensity : 0f;

        /// <summary>Returns true if any token of the given type is currently active.</summary>
        public bool HasActive(EmotionType type) => _active.ContainsKey(type);

        /// <summary>Read-only view of all active tokens. Used by GoalGenerator.</summary>
        public IReadOnlyDictionary<EmotionType, EmotionToken> Active => _active;

        /// <summary>Snapshots all active tokens for inclusion in a MemoryEntry.</summary>
        public IReadOnlyList<EmotionToken> Snapshot()
            => _active.Values.Select(t => t.Snapshot()).ToList();
    }
}