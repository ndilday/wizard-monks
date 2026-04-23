using System;

namespace WizardMonks.Models.Characters
{
    /// <summary>
    /// A live emotion state for one character, with intensity that decays over time.
    ///
    /// Tokens are held in a character's EmotionLedger. The same emotion type
    /// can only have one active token at a time — new events of the same type
    /// add to the existing token's intensity rather than creating a duplicate.
    /// </summary>
    public sealed class EmotionToken
    {
        public EmotionType Type { get; }

        /// <summary>Current intensity. Decays each tick. Expires when below MinIntensity.</summary>
        public float Intensity { get; private set; }

        /// <summary>Fraction of intensity lost per tick. Personality-modulated at creation.</summary>
        public float DecayRate { get; }

        /// <summary>Simulation tick at which this token was created or last reinforced.</summary>
        public int OriginTick { get; private set; }

        /// <summary>Intensity below which this token is considered expired.</summary>
        public const float MinIntensity = 0.01f;

        public EmotionToken(EmotionType type, float intensity, float decayRate, int originTick)
        {
            Type = type;
            Intensity = Math.Clamp(intensity, 0f, 1f);
            DecayRate = Math.Clamp(decayRate, 0f, 1f);
            OriginTick = originTick;
        }

        /// <summary>
        /// Applies one tick of decay. Returns true if the token is still active
        /// after decay, false if it has expired.
        /// </summary>
        public bool Decay()
        {
            Intensity *= (1f - DecayRate);
            return Intensity >= MinIntensity;
        }

        /// <summary>
        /// Adds intensity from a new event of the same type.
        /// Intensity is clamped to [0, 1]. The origin tick is refreshed.
        /// </summary>
        public void Reinforce(float additionalIntensity, int currentTick)
        {
            Intensity = Math.Clamp(Intensity + additionalIntensity, 0f, 1f);
            OriginTick = currentTick;
        }

        /// <summary>Snapshot copy used when recording a MemoryEntry.</summary>
        public EmotionToken Snapshot() => new(Type, Intensity, DecayRate, OriginTick);
    }
}