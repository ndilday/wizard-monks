using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WizardMonks.Activities;

namespace WizardMonks.Models
{
    public class Laboratory : LabFeature
    {
        public double TotalSize { get; private set; }
        public double Refinement { get; private set; }

        private List<LabFeature> _features;
        private Magus _owner;
        private Aura _aura;

        public Laboratory(Magus owner, Aura aura, double size) : base("Laboratory", 0, 0, 0, 0, 0, 0, 0, [], [])
        {
            _features = [];
            _owner = owner;
            TotalSize = size;
            _aura = aura;
            Refinement = 0;
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

        public double GetAvailableSpace()
        {
            return TotalSize + Refinement - Size;
        }

        public void Refine()
        {
            Refinement++;
        }

        public bool HasFeature(LabFeature feature)
        {
            return _features.Contains(feature);
        }

        public void AddFeature(LabFeature feature)
        {
            if (GetAvailableSpace() < feature.Size)
            {
                throw new ArgumentException("Feature too large for lab");
            }
            else
            {
                _features.Add(feature);
                AddFeatureStats(feature);
                foreach (KeyValuePair<Ability, double> artModifier in feature.ArtModifiers)
                {
                    if (!ArtModifiers.ContainsKey(artModifier.Key))
                    {
                        ArtModifiers[artModifier.Key] = artModifier.Value;
                    }
                    else
                    {
                        ArtModifiers[artModifier.Key] += artModifier.Value;
                    }
                }
                foreach (KeyValuePair<Activity, double> activityModifier in feature.ActivityModifiers)
                {
                    if (!ActivityModifiers.ContainsKey(activityModifier.Key))
                    {
                        ActivityModifiers[activityModifier.Key] = activityModifier.Value;
                    }
                    else
                    {
                        ActivityModifiers[activityModifier.Key] += activityModifier.Value;
                    }
                }
            }
        }

        private void AddFeatureStats(LabFeature feature)
        {
            Aesthetics += feature.Aesthetics;
            Quality += feature.Quality;
            Safety += feature.Safety;
            Upkeep += feature.Upkeep;
            Warping += feature.Warping;
        }

        public void RemoveFeature(LabFeature feature)
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

        private void SubtractFeatureStats(LabFeature feature)
        {
            Aesthetics -= feature.Aesthetics;
            Quality -= feature.Quality;
            Safety -= feature.Safety;
            Upkeep -= feature.Upkeep;
            Warping -= feature.Warping;
        }
    }
}
