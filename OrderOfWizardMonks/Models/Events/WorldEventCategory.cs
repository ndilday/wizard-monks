namespace WizardMonks.Models.Events
{
    /// <summary>
    /// High-level classification of a world event.
    /// The AppraisalEngine dispatches on this to determine which OCC axes apply.
    /// The ReflectionEngine dispatches on this to determine what belief evidence to extract.
    /// Add new categories here only alongside corresponding handling in both consumers.
    /// </summary>
    public enum WorldEventCategory
    {
        // --- Self-generated outcomes ---
        LabSuccess,
        LabFailure,
        AgingCrisis,
        AgingNormal,
        SpellInvented,
        BreakthroughMade,
        LongevityRitualVoided,

        // --- Social / interpersonal ---
        CharacterObserved,
        BookReceived,
        LabTextReceived,

        /// <summary>
        /// A character received direct instruction from another character.
        /// Subject is the teacher; the student must be in Participants.
        /// </summary>
        TeachingReceived,

        RecruitmentAttempted,
        RecruitmentSucceeded,
        RecruitmentFailed,
        ApprenticeFound,
        ApprenticeGauntleted,
        ApprenticeLost,

        // --- World / environmental ---
        AuraChanged,
        VisSourceDiscovered,
        CovenantFounded,

        // --- Scenario-injected ---
        ScenarioEvent
    }
}