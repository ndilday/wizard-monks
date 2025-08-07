using System;
public static class BeliefNormalizer
{
    // Arts & Abilities: A score of 5 is a significant milestone. Let's map it to a Magnitude of ~5.
    public static double FromSkillScore(double score) => score;

    // Attributes: A +3 is exceptional. A -3 is equally so. We'll map the -5 to +5 range to a positive scale.
    public static double FromAttributeScore(double score) => Math.Abs(score) * 2; // Maps [-5, 0, 5] to [10, 0, 10]

    // Personality: Scores range from ~0 to 2. Let's scale this up.
    public static double FromPersonalityFacet(double score) => Math.Abs(score-1) * 10; // Maps [0, 1, 2] to [10, 0, 10]

    // Book/Spell Quality: Directly use the Quality score as a base.
    public static double CommunicationFromQuality(double quality) => quality - 2;

    public static double FromArtScore(double score) => score / 2.0;

    public static double ArtFromSumma(double quality, double level) => ((quality - 6) + level) * 2;
}