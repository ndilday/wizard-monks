namespace WizardMonks.Models.Beliefs
{
    public static class BeliefToReputationNormalizer
    {
        // Arts & Abilities: A score of 5 is a significant milestone. Let's map it to a Magnitude of ~10.
        public static double FromSkillScore(double score) => score * 2;

        // Attributes: A +3 is exceptional. A -3 is equally so. We'll map the -5 to +5 range to a positive scale.
        public static double FromAttributeScore(double score) => score * 2; // Maps [-5, 0, 5] to [-20, 0, 20]

        // Personality: Scores range from ~0 to 2. Let's scale this up.
        public static double FromPersonalityFacet(double score) => score - 1 * 20; // Maps [0, 1, 2] to [-20, 0, 20]

        // Book Quality: Directly use the Quality score as a base.
        public static double CommunicationFromQuality(double quality) => FromAttributeScore(quality - 6);

        // Art scores are the effective default
        public static double FromArtScore(double score) => score;

        public static double ArtFromSumma(double quality, double level) => (quality - 6 + level) * 2;
    }
}