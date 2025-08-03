using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Core;

namespace WizardMonks.Models
{
    public enum HexacoFactor
    {
        HonestyHumility,
        Emotionality,
        Extraversion,
        Agreeableness,
        Conscientiousness,
        OpennessToExperience
    }

    public enum HexacoFacet
    {
        // Honesty-Humility
        Sincerity,
        Fairness,
        GreedAvoidance,
        Modesty,

        // Emotionality
        Fearfulness,
        Anxiety,
        Dependence,
        Sentimentality,

        // Extraversion
        SocialSelfEsteem,
        SocialBoldness,
        Sociability,
        Liveliness,

        // Agreeableness
        Forgiveness,
        Gentleness,
        Flexibility,
        Patience,

        // Conscientiousness
        Organization,
        Diligence,
        Perfectionism,
        Prudence,

        // Openness to Experience
        AestheticAppreciation,
        Inquisitiveness,
        Creativity,
        Unconventionality
    }

    [Serializable]
    public class Personality
    {
        private readonly Dictionary<HexacoFacet, double> _facets;
        private const double MEAN = 1.0;
        private const double STANDARD_DEVIATION = 0.35; // This is our main tuning knob
        private const double MIN_VALUE = 0.001; // Epsilon to avoid exactly zero
        private const double MAX_VALUE = 2.0;

        #region Constructors
        /// <summary>
        /// Creates a Personality with randomized facet scores.
        /// Each facet is assigned a random value in the range (0.0, 2.0].
        /// </summary>
        public Personality()
        {
            _facets = new Dictionary<HexacoFacet, double>();
            foreach (HexacoFacet facet in Enum.GetValues(typeof(HexacoFacet)))
            {
                // Generate a standard normal value (mean 0, std dev 1)
                double standardNormalValue = Die.Instance.RollNormal();

                // Scale and shift it to our desired mean and standard deviation
                double generatedValue = MEAN + (standardNormalValue * STANDARD_DEVIATION);

                // Clamp the result to our desired range [0.001, 2.0]
                _facets[facet] = Math.Max(MIN_VALUE, Math.Min(MAX_VALUE, generatedValue));
            }
        }

        /// <summary>
        /// Creates a Personality from a predefined set of facet scores.
        /// Useful for loading saved characters or initializing specific NPCs like Founders.
        /// </summary>
        /// <param name="initialFacets">A dictionary containing a score for every HexacoFacet.</param>
        public Personality(Dictionary<HexacoFacet, double> initialFacets)
        {
            if (initialFacets == null || initialFacets.Count != 24)
            {
                throw new ArgumentException("InitialFacets dictionary must contain exactly 24 values, one for each facet.");
            }
            _facets = new Dictionary<HexacoFacet, double>(initialFacets);
        }
        #endregion

        #region Factor Accessors (Calculated)
        public double HonestyHumility => (_facets[HexacoFacet.Sincerity] + _facets[HexacoFacet.Fairness] + _facets[HexacoFacet.GreedAvoidance] + _facets[HexacoFacet.Modesty]) / 4.0;
        public double Emotionality => (_facets[HexacoFacet.Fearfulness] + _facets[HexacoFacet.Anxiety] + _facets[HexacoFacet.Dependence] + _facets[HexacoFacet.Sentimentality]) / 4.0;
        public double Extraversion => (_facets[HexacoFacet.SocialSelfEsteem] + _facets[HexacoFacet.SocialBoldness] + _facets[HexacoFacet.Sociability] + _facets[HexacoFacet.Liveliness]) / 4.0;
        public double Agreeableness => (_facets[HexacoFacet.Forgiveness] + _facets[HexacoFacet.Gentleness] + _facets[HexacoFacet.Flexibility] + _facets[HexacoFacet.Patience]) / 4.0;
        public double Conscientiousness => (_facets[HexacoFacet.Organization] + _facets[HexacoFacet.Diligence] + _facets[HexacoFacet.Perfectionism] + _facets[HexacoFacet.Prudence]) / 4.0;
        public double OpennessToExperience => (_facets[HexacoFacet.AestheticAppreciation] + _facets[HexacoFacet.Inquisitiveness] + _facets[HexacoFacet.Creativity] + _facets[HexacoFacet.Unconventionality]) / 4.0;
        #endregion

        #region Facet Accessors and Mutators
        /// <summary>
        /// Gets the score for a specific facet.
        /// </summary>
        public double GetFacet(HexacoFacet facet)
        {
            return _facets[facet];
        }

        /// <summary>
        /// Sets the score for a specific facet.
        /// </summary>
        public void SetFacet(HexacoFacet facet, double value)
        {
            // Optional: Add validation to keep value within a specific range, e.g., [0, 2].
            _facets[facet] = value;
        }
        #endregion
    }
}