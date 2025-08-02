using System;
using System.Collections.Generic;
using WizardMonks.Activities;
using WizardMonks.Models;

namespace WizardMonks.Instances
{
    public static class LabFeatures
    {
        // Central list of all defined features
        public static readonly List<LabFeature> AllFeatures;

        // Optimized lookups
        public static readonly Dictionary<Ability, List<LabFeature>> FeaturesByArt;
        public static readonly Dictionary<Activity, List<LabFeature>> FeaturesByActivity;

        public static readonly LabFeature HighlyOrganized;
        public static readonly LabFeature Spotless;
        public static readonly LabFeature HiddenDefect;

        static LabFeatures()
        {
            //Altar = new LabFeature("Altar", 3, )
            HighlyOrganized = new LabFeature("Highly Organized", 0, 0, 0, 1, 0, 0, 0, null, null);

            Spotless = new LabFeature("Spotless", 0, 1, 1, 0, 0, 0, 0, new Tuple<Ability, double>(MagicArts.Creo, 1), null);

            HiddenDefect = new LabFeature("Hidden Defect", 0, 0, 0, 0, -3, 0, 0, null, null);

            AllFeatures = [HighlyOrganized, Spotless, HiddenDefect];
            FeaturesByArt = [];
            FeaturesByActivity = [];
            foreach (var feature in AllFeatures)
            {
                if (feature.ArtModifier != null)
                {
                    if (!FeaturesByArt.ContainsKey(feature.ArtModifier.Item1))
                    {
                        FeaturesByArt[feature.ArtModifier.Item1] = [];
                    }

                    FeaturesByArt[feature.ArtModifier.Item1].Add(feature);
                }
                if (feature.ActivityModifier != null)
                {
                    if (!FeaturesByActivity.ContainsKey(feature.ActivityModifier.Item1))
                    {
                        FeaturesByActivity[feature.ActivityModifier.Item1] = [];
                    }
                    FeaturesByActivity[feature.ActivityModifier.Item1].Add(feature);
                }
            }
        }
    }
}
