using System;
using WizardMonks.Core;

namespace WizardMonks.Characters
{
    public class Personality
    {
        public double Openness { get; set; }
        public double Conscientiousness { get; set; }
        public double Extroversion { get; set; }
        public double Agreeableness { get; set; }
        public double Neuroticism { get; set; }

        public Personality()
        {
            Openness = Die.Instance.RollDouble();
            Conscientiousness = Die.Instance.RollDouble();
            Extroversion = Die.Instance.RollDouble();
            Agreeableness = Die.Instance.RollDouble();
            Neuroticism = Die.Instance.RollDouble();
        }

        public Personality(double openness, double conscientiousness, double extroversion, double agreeableness, double neuroticism)
        {
            Openness = openness;
            Conscientiousness = conscientiousness;
            Extroversion = extroversion;
            Agreeableness = agreeableness;
            Neuroticism = neuroticism;
        }
    }

    public class PersonalityPreference(double opennessMultiplier, double conscientiousnessMultiplier, double extroversionMultiplier, double agreeablenessMultiplier, double neuroticismMultiplier)
    {
        public double OpennessMultiplier { get; private set; } = opennessMultiplier;
        public double ConscientiousnessMultiplier { get; private set; } = conscientiousnessMultiplier;
        public double ExtroversionMultiplier { get; private set; } = extroversionMultiplier;
        public double AgreeablenessMultiplier { get; private set; } = agreeablenessMultiplier;
        public double NeuroticismMultiplier { get; private set; } = neuroticismMultiplier;

        public double CalculatePreferenceMultiplier(Personality personality)
        {
            // personality traits are measured on a scale of 0-1
            // we want to turn them into a modifier from -1 to 1
            // Then we multiply this modifier by its corresponding multiplier
            // (which will generally be -1, 0, or 1)
            double opennessFactor = (personality.Openness * 2 - 1) * OpennessMultiplier;
            double conscientiousnessFactor = (personality.Conscientiousness * 2 - 1) * ConscientiousnessMultiplier;
            double extroversionFactor = (personality.Extroversion * 2 - 1) * ExtroversionMultiplier;
            double agreeablenessFactor = (personality.Agreeableness * 2 - 1) * AgreeablenessMultiplier;
            double neuroticismFactor = (personality.Neuroticism * 2 - 1) * NeuroticismMultiplier;

            // At the end, we want to sum all the factors and scale them back to a value between -1 and 1
            double scaler = Math.Abs(OpennessMultiplier) + Math.Abs(ConscientiousnessMultiplier) +
                Math.Abs(ExtroversionMultiplier) + Math.Abs(AgreeablenessMultiplier) + Math.Abs(NeuroticismMultiplier);
            return (opennessFactor + conscientiousnessFactor + extroversionFactor + agreeablenessFactor + neuroticismFactor) / scaler;
        }
    }
}
