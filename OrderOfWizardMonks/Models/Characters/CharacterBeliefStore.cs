using System;
using System.Collections.Generic;
using WizardMonks.Models.Beliefs;

namespace WizardMonks.Models.Characters
{
    /// <summary>
    /// Holds all CharacterBeliefProfiles for one character:
    /// their self-beliefs and beliefs about every other subject they have encountered.
    ///
    /// This is the primary input to GoalGenerator and the primary output of ReflectionEngine.
    /// </summary>
    public sealed class CharacterBeliefStore
    {
        private readonly Dictionary<Guid, CharacterBeliefProfile> _profiles = new();

        /// <summary>
        /// The self-belief profile. Same structure as profiles for other subjects,
        /// with the owning character as the subject.
        /// </summary>
        public CharacterBeliefProfile SelfBeliefs { get; }

        public CharacterBeliefStore(Character owner)
        {
            SelfBeliefs = new CharacterBeliefProfile(owner);
        }

        /// <summary>
        /// Returns the belief profile for the given subject ID, creating one if it
        /// does not exist. Called by ReflectionEngine when synthesizing beliefs
        /// about an observed character.
        /// </summary>
        public CharacterBeliefProfile GetOrCreate(Guid subjectId)
        {
            if (!_profiles.TryGetValue(subjectId, out var profile))
            {
                profile = new CharacterBeliefProfile(new UnknownSubject(subjectId));
                _profiles[subjectId] = profile;
            }
            return profile;
        }

        /// <summary>Returns a belief profile for a known subject, or null if none exists.</summary>
        public CharacterBeliefProfile? Get(Guid subjectId)
            => _profiles.TryGetValue(subjectId, out var p) ? p : null;

        /// <summary>All profiles for other subjects (excludes self-beliefs).</summary>
        public IReadOnlyDictionary<Guid, CharacterBeliefProfile> Others => _profiles;

        // Placeholder for subjects referenced by ID only (no object reference available).
        private sealed class UnknownSubject : IBeliefSubject
        {
            public Guid Id { get; }
            public UnknownSubject(Guid id) => Id = id;
            public string Name => "Unknown";
        }
    }
}