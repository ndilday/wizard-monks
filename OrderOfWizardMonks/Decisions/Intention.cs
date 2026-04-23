using System;
using WizardMonks.Decisions.Goals;

namespace WizardMonks.Decisions
{
    /// <summary>
    /// A committed plan of action. Wraps an IGoal and adds commitment and
    /// desire tracking that the revised architecture requires.
    ///
    /// Intentions are sticky: once formed, they persist unless a reconsideration
    /// trigger fires. Desire is recomputed each tick; commitment strength is set
    /// at formation (formation-fixed model) and does not change.
    /// </summary>
    public sealed class Intention
    {
        /// <summary>Unique identifier. Used by MemoryEntry to reference relevant intentions.</summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>The goal this intention is committed to achieving.</summary>
        public IGoal UnderlyingGoal { get; }

        /// <summary>
        /// Governs how easily this intention can be displaced by reconsideration triggers.
        /// Set at formation from the generating emotion token intensity and personality modifiers.
        /// Range: [0.0, 1.0].
        /// </summary>
        public float CommitmentStrength { get; }

        /// <summary>
        /// Priority score among competing active intentions.
        /// Recomputed each tick from current emotion tokens, beliefs, and personality.
        /// </summary>
        public float DesireScore { get; private set; }

        /// <summary>Number of ticks invested in pursuing this intention.</summary>
        public int TicksInvested { get; private set; }

        /// <summary>Simulation tick at which this intention was formed.</summary>
        public int FormationTick { get; }

        /// <summary>
        /// Maximum number of ticks without meaningful progress before a stagnation
        /// reconsideration trigger fires. Modulated by Prudence at formation.
        /// </summary>
        public int MaxStagnationTicks { get; }

        private int _lastProgressTick;

        public Intention(
            IGoal underlyingGoal,
            float commitmentStrength,
            float initialDesireScore,
            int formationTick,
            int maxStagnationTicks)
        {
            UnderlyingGoal = underlyingGoal;
            CommitmentStrength = Math.Clamp(commitmentStrength, 0f, 1f);
            DesireScore = Math.Clamp(initialDesireScore, 0f, 1f);
            FormationTick = formationTick;
            MaxStagnationTicks = maxStagnationTicks;
            TicksInvested = 0;
            _lastProgressTick = formationTick;
        }

        /// <summary>Updates the desire score for the current tick.</summary>
        public void UpdateDesireScore(float newScore)
            => DesireScore = Math.Clamp(newScore, 0f, 1f);

        /// <summary>Records one tick of investment without marking progress.</summary>
        public void RecordTick() => TicksInvested++;

        /// <summary>Records that meaningful progress was made this tick.</summary>
        public void RecordProgress(int currentTick)
        {
            TicksInvested++;
            _lastProgressTick = currentTick;
        }

        /// <summary>
        /// Evaluates whether this intention should be reconsidered.
        ///
        /// Reconsideration triggers (PRD §2.2.3):
        ///   1. Incoming event importance exceeds CommitmentStrength.
        ///   2. A belief revision occurred this tick.
        ///   3. A conflicting emotion token exceeds CommitmentStrength.
        ///   4. Too many ticks without meaningful progress.
        /// </summary>
        public bool ShouldReconsider(
            float incomingEventImportance,
            bool beliefRevisionOccurred,
            float conflictingEmotionIntensity,
            int currentTick)
        {
            if (UnderlyingGoal.IsComplete()) return true;
            if (incomingEventImportance > CommitmentStrength) return true;
            if (beliefRevisionOccurred) return true;
            if (conflictingEmotionIntensity > CommitmentStrength) return true;
            if ((currentTick - _lastProgressTick) > MaxStagnationTicks) return true;

            return false;
        }
    }
}