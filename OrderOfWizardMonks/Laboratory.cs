using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WizardMonks.Activities;

namespace WizardMonks
{
    public class Feature
    {
        public double Refinement { get; protected set; }
        public double Aesthetics { get; protected set; }
        public double Quality { get; protected set; }
        public double Safety { get; protected set; }
        public double Upkeep { get; protected set; }
        public double Warping { get; protected set; }
        public Dictionary<Ability, double> ArtModifiers { get; protected set; }
        public Dictionary<Activity, double> ActivityModifiers { get; protected set; }

        public Feature()
        {
            ArtModifiers = new Dictionary<Ability, double>();
            ActivityModifiers = new Dictionary<Activity, double>();
            Refinement = 0;
            Aesthetics = 0;
            Quality = 0;
            Safety = 0;
            Upkeep = 0;
            Warping = 0;
        }
    }

    public class Laboratory : Feature
    {
        public double Size { get; private set; }

        private List<Feature> _features;
        private Magus _owner;
        private Aura _aura;
        private double _availableRefinement;

        public Laboratory(Magus owner, Aura aura, double size) : base()
        {
            _features = new List<Feature>();
            _owner = owner;
            Size = size;
            _availableRefinement = size;
            _aura = aura;
        }

        public double GetModifier(ArtPair artPair, Activity activity)
        {
            double totalModifier = Quality;
            if (ArtModifiers.ContainsKey(artPair.Technique))
            {
                totalModifier += ArtModifiers[artPair.Technique];
            }
            if (ArtModifiers.ContainsKey(artPair.Form))
            {
                totalModifier += ArtModifiers[artPair.Form];
            }
            if (ActivityModifiers.ContainsKey(activity))
            {
                totalModifier += ActivityModifiers[activity];
            }
            totalModifier += _aura.Strength;
            return totalModifier;
        }

        public void Refine()
        {
            Refinement++;
            _availableRefinement++;
        }

        public void AddFeature(Feature feature)
        {
            _features.Add(feature);
            AddFeatureStats(feature);
            foreach (KeyValuePair<Ability, double> artModifier in feature.ArtModifiers)
            {
                ArtModifiers[artModifier.Key] += artModifier.Value;
            }
            foreach (KeyValuePair<Activity, double> activityModifier in feature.ActivityModifiers)
            {
                ActivityModifiers[activityModifier.Key] += activityModifier.Value;
            }
        }

        private void AddFeatureStats(Feature feature)
        {
            Aesthetics += feature.Aesthetics;
            Quality += feature.Quality;
            Safety += feature.Safety;
            Upkeep += feature.Upkeep;
            Warping += feature.Warping;
        }

        public void RemoveFeature(Feature feature)
        {
            if (_features.Contains(feature))
            {
                _features.Remove(feature);
                SubtractFeatureStats(feature);
                foreach (KeyValuePair<Ability, double> artModifier in feature.ArtModifiers)
                {
                    ArtModifiers[artModifier.Key] -= artModifier.Value;
                }
                foreach (KeyValuePair<Activity, double> activityModifier in feature.ActivityModifiers)
                {
                    ActivityModifiers[activityModifier.Key] -= activityModifier.Value;
                }
            }
        }

        private void SubtractFeatureStats(Feature feature)
        {
            Aesthetics -= feature.Aesthetics;
            Quality -= feature.Quality;
            Safety -= feature.Safety;
            Upkeep -= feature.Upkeep;
            Warping -= feature.Warping;
        }
    }
}
