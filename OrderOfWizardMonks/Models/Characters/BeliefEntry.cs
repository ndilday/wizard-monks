using System;

namespace WizardMonks.Models.Characters
{
    /// <summary>
    /// A single belief held about a specific dimension of a specific subject.
    ///
    /// Value is normalized to [-1.0, 1.0]:
    ///   -1.0 = strongly negative belief
    ///    0.0 = neutral / no opinion formed
    ///   +1.0 = strongly positive belief
    ///
    /// Confidence is [0.0, 1.0]:
    ///   0.0 = newly formed, based on minimal evidence
    ///   1.0 = deeply held, reinforced by many consistent experiences
    /// </summary>
    public sealed class BeliefEntry
    {
        /// <summary>
        /// The dimension this belief is about. Uses the canonical BeliefTopic string keys
        /// from the existing Models.Beliefs.BeliefTopic registry.
        /// Examples: "Creo", "Inquisitiveness", "Trustworthiness", "Vulnerability"
        /// </summary>
        public string Dimension { get; }

        /// <summary>Normalized belief value. Range: [-1.0, 1.0].</summary>
        public float Value { get; private set; }

        /// <summary>Confidence in this belief. Range: [0.0, 1.0].</summary>
        public float Confidence { get; private set; }

        /// <summary>Tick at which this entry was last revised by reflection.</summary>
        public int LastRevisedTick { get; private set; }

        public BeliefEntry(string dimension, float value, float confidence, int tick)
        {
            Dimension = dimension;
            Value = Math.Clamp(value, -1f, 1f);
            Confidence = Math.Clamp(confidence, 0f, 1f);
            LastRevisedTick = tick;
        }

        /// <summary>
        /// Updates this belief during a reflection pass.
        /// The new value is a confidence-weighted blend of the existing belief
        /// and the incoming evidence, nudging toward the evidence proportionally
        /// to how weak the existing confidence is.
        /// </summary>
        public void Update(float evidenceValue, float evidenceWeight, int tick)
        {
            float resistance = Confidence;
            float openness = 1f - resistance;

            Value = Math.Clamp(
                Value + openness * evidenceWeight * (evidenceValue - Value),
                -1f, 1f);

            Confidence = Math.Clamp(Confidence + evidenceWeight * 0.1f, 0f, 1f);
            LastRevisedTick = tick;
        }

        /// <summary>
        /// Applies confidence decay. Called periodically when no corroborating
        /// evidence has arrived.
        /// </summary>
        public void DecayConfidence(float decayAmount)
        {
            Confidence = Math.Clamp(Confidence - decayAmount, 0f, 1f);
        }
    }
}