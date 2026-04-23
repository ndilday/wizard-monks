namespace WizardMonks.Models.Events
{
    /// <summary>
    /// High-level classification of a world event.
    /// The AppraisalEngine dispatches on this to determine which OCC axes apply.
    /// Add new categories here as new simulation systems are built out.
    /// </summary>
    public enum WorldEventCategory
    {
        // --- Self-generated outcomes ---
        LabSuccess,          // A lab project completed successfully
        LabFailure,          // A lab project failed or botched
        AgingCrisis,         // An aging crisis occurred for this character
        AgingNormal,         // A normal aging roll with no crisis
        SpellInvented,       // A formulaic spell was successfully invented
        BreakthroughMade,    // A research breakthrough was achieved
        LongevityRitualVoided, // Longevity ritual was voided by an aging crisis

        // --- Social / interpersonal ---
        CharacterObserved,   // This character witnessed something happen to another
        BookReceived,        // A book arrived (gift, trade, or library access)
        LabTextReceived,     // A lab text arrived
        RecruitmentAttempted,// An attempt was made to recruit a hedge mage
        RecruitmentSucceeded,
        RecruitmentFailed,
        ApprenticeFound,
        ApprenticeGauntleted,
        ApprenticeLost,

        // --- World / environmental ---
        AuraChanged,         // Aura strength at the covenant changed
        VisSourceDiscovered,
        CovenantFounded,

        // --- Scenario-injected ---
        ScenarioEvent        // Catch-all for Scenario Manager injections (§2.9.1)
    }
}