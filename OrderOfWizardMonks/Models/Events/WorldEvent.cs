using System;
using System.Collections.Generic;
using WizardMonks.Models.Characters;

namespace WizardMonks.Models.Events
{
    /// <summary>
    /// An immutable record of something that happened in the simulation world.
    /// WorldEvents are the sole input to the AppraisalEngine; the engine produces
    /// a MemoryEntry from each event for every character that observes it.
    ///
    /// Events are created by simulation systems (activities, aging, scenario manager, etc.)
    /// and broadcast to the set of characters who can observe them. The observer set
    /// is determined by the simulation layer, not by this record.
    /// </summary>
    public sealed class WorldEvent
    {
        /// <summary>Unique identifier for this event instance.</summary>
        public Guid Id { get; }

        /// <summary>Simulation tick at which this event occurred.</summary>
        public int Tick { get; }

        /// <summary>High-level classification used by the AppraisalEngine for dispatch.</summary>
        public WorldEventCategory Category { get; }

        /// <summary>
        /// The character who is the primary subject of this event — typically the one
        /// whose action caused it, or who it happened to. May be null for world-state events
        /// (e.g., aura changes) with no single subject.
        /// </summary>
        public Character? Subject { get; }

        /// <summary>
        /// Other characters involved. For interpersonal events, this includes targets,
        /// witnesses, or co-participants. Order is not semantically significant.
        /// </summary>
        public IReadOnlyList<Character> Participants { get; }

        /// <summary>
        /// Numeric outcome value, interpreted per category.
        /// For lab results: the lab total achieved.
        /// For aging: the die result.
        /// For breakthrough: breakthrough points gained.
        /// Null when not applicable.
        /// </summary>
        public float? NumericOutcome { get; }

        /// <summary>
        /// Arbitrary string payload for categories that need to convey domain-specific
        /// detail without introducing strong coupling. Examples:
        ///   LabSuccess → spell name or project name
        ///   ScenarioEvent → scenario-defined event key
        ///   BookReceived → book title
        /// </summary>
        public string? Detail { get; }

        /// <summary>
        /// Whether the outcome of this event was positive (true), negative (false),
        /// or ambiguous/neutral (null) from an objective standpoint.
        /// The AppraisalEngine uses this in combination with goal relevance and attribution
        /// to determine emotion polarity. Characters may appraise the same outcome differently.
        /// </summary>
        public bool? IsPositiveOutcome { get; }

        private WorldEvent(
            int tick,
            WorldEventCategory category,
            Character? subject,
            IReadOnlyList<Character> participants,
            float? numericOutcome,
            string? detail,
            bool? isPositiveOutcome)
        {
            Id = Guid.NewGuid();
            Tick = tick;
            Category = category;
            Subject = subject;
            Participants = participants;
            NumericOutcome = numericOutcome;
            Detail = detail;
            IsPositiveOutcome = isPositiveOutcome;
        }

        // -----------------------------------------------------------------
        // Factory methods — one per logical event shape.
        // Prefer these over direct construction to keep call sites readable.
        // -----------------------------------------------------------------

        /// <summary>Creates an event representing a lab activity outcome.</summary>
        public static WorldEvent LabOutcome(
            int tick,
            WorldEventCategory category,
            Character subject,
            float labTotal,
            string projectName,
            bool success)
            => new(tick, category, subject, Array.Empty<Character>(), labTotal, projectName, success);

        /// <summary>Creates an event representing an aging roll outcome.</summary>
        public static WorldEvent AgingOutcome(
            int tick,
            WorldEventCategory category,
            Character subject,
            float dieResult,
            bool isCrisis)
            => new(tick, category, subject, Array.Empty<Character>(), dieResult, null, !isCrisis);

        /// <summary>Creates an interpersonal event involving a subject and at least one other participant.</summary>
        public static WorldEvent Interpersonal(
            int tick,
            WorldEventCategory category,
            Character subject,
            IReadOnlyList<Character> participants,
            bool? isPositive,
            string? detail = null)
            => new(tick, category, subject, participants, null, detail, isPositive);

        /// <summary>Creates a scenario-injected event with a named key.</summary>
        public static WorldEvent ScenarioInjected(
            int tick,
            string eventKey,
            Character? subject,
            IReadOnlyList<Character>? participants,
            bool? isPositive)
            => new(tick, WorldEventCategory.ScenarioEvent,
                   subject,
                   participants ?? Array.Empty<Character>(),
                   null, eventKey, isPositive);
    }
}