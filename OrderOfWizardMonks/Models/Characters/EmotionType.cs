namespace WizardMonks.Models.Characters
{
    /// <summary>
    /// The subset of OCC emotion types relevant to this simulation.
    /// Organized as opposing pairs for reference clarity.
    /// </summary>
    public enum EmotionType
    {
        // Prospect-based (anticipation of future events)
        Hope,
        Fear,

        // Well-being (confirmed outcomes relevant to current goals)
        Joy,
        Distress,

        // Attribution to self
        Pride,
        Shame,

        // Attribution to others
        Admiration,
        Reproach,

        // Interaction with others (action directed at self)
        Gratitude,
        Anger,

        // Comparison with others
        Envy,
        Gloating
    }
}