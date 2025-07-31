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
        public static readonly LabFeature Altar;

        static LabFeatures()
        {
            //Altar = new LabFeature("Altar", 3, )
            HighlyOrganized = new LabFeature("Highly Organized", 0, 0, 0, 1, 0, 0, 0, [], []);

            Spotless = new LabFeature("Spotless", 0, 1, 1, 0, 0, 0, 0, new Dictionary<Ability, double> { { MagicArts.Creo, 1 } }, []);

            HiddenDefect = new LabFeature("Hidden Defect", 0, 0, 0, 0, -3, 0, 0, [], []);

            AllFeatures = [HighlyOrganized, Spotless, HiddenDefect];
            FeaturesByArt = [];
            FeaturesByActivity = [];
            foreach (var feature in AllFeatures)
            {
                foreach(var art in feature.ArtModifiers.Keys)
                {
                    if(!FeaturesByArt.ContainsKey(art))
                    {
                        FeaturesByArt[art] = [];
                    }
                    FeaturesByArt[art].Add(feature);
                }
                foreach(var activity in feature.ActivityModifiers.Keys)
                {
                    if(!FeaturesByActivity.ContainsKey(activity))
                    {
                        FeaturesByActivity[activity] = [];
                    }
                    FeaturesByActivity[activity].Add(feature);
                }
            }
        }
    }
}
