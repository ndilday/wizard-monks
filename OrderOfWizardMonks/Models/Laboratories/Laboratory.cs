using System;
using System.Collections.Generic;
using WizardMonks.Activities;
using WizardMonks.Instances;

namespace WizardMonks.Models.Laboratories
{
    public class Laboratory : LabFeature
    {
        public double TotalSize { get; private set; }
        public double Refinement { get; private set; }
        public LabSpecialization Specialization { get; private set; }
        public Dictionary<Ability, double> ArtModifiers { get; private set; }
        public Dictionary<Activity, double> ActivityModifiers { get; private set; }

        private List<LabFeature> _features;
        private Magus _owner;
        private Aura _aura;

        public Laboratory(Magus owner, Aura aura, double size) : base("Laboratory", 0, 0, 0, 0, 0, 0, 0, null, null)
        {
            _features = [];
            _owner = owner;
            TotalSize = size;
            _aura = aura;
            Refinement = 0;
            Specialization = null;
            ArtModifiers = [];
            ActivityModifiers = [];
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
            if (Specialization != null)
            {
                var currentSpecialization = Specialization.GetCurrentBonuses();
                totalModifier -= currentSpecialization.Quality;
                if(artPair.Technique == Specialization.ArtTopic || artPair.Form == Specialization.ArtTopic || activity == Specialization.ActivityTopic)
                {
                    totalModifier += currentSpecialization.Bonus;
                }
            }
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
                if (feature.ArtModifier != null)
                {
                    if(!ArtModifiers.ContainsKey(feature.ArtModifier.Item1))
                    {
                        ArtModifiers[feature.ArtModifier.Item1] = 0;
                    }
                    ArtModifiers[feature.ArtModifier.Item1] += feature.ArtModifier.Item2;
                }
                if (feature.ActivityModifier != null)
                {
                    if (!ActivityModifiers.ContainsKey(feature.ActivityModifier.Item1))
                    {
                        ActivityModifiers[feature.ActivityModifier.Item1] = 0;
                    }
                    ActivityModifiers[feature.ActivityModifier.Item1] += feature.ActivityModifier.Item2;
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
                if (feature.ArtModifier != null)
                {
                    ArtModifiers[feature.ArtModifier.Item1] -= feature.ArtModifier.Item2;
                }
                if (feature.ActivityModifier != null)
                {
                    ActivityModifiers[feature.ActivityModifier.Item1] -= feature.ActivityModifier.Item2;
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

        public void Specialize(Ability art)
        {
            if(Specialization != null && Specialization.ArtTopic != null && Specialization.ArtTopic == art)
            {
                // check prereqs
                var prereqs = Specialization.GetPrerequisitesForNextStage();
                if(prereqs.MagicTheory > _owner.GetAbility(Abilities.MagicTheory).Value)
                {
                    throw new InvalidOperationException($"{_owner.Name} does not have enough Magic Theory for this lab specialization, needs {prereqs.MagicTheory}");
                }
                Specialization.Upgrade();
                Refine();
            }
            else
            {
                Specialization = new LabSpecialization(art);
            }
        }

        public void Specialize(Activity activity)
        {
            if (Specialization != null && Specialization.ActivityTopic == activity)
            {
                var prereqs = Specialization.GetPrerequisitesForNextStage();
                if(prereqs.MagicTheory > _owner.GetAbility(Abilities.MagicTheory).Value)
                {
                    throw new InvalidOperationException($"{_owner.Name} does not have enough Magic Theory for this lab specialization, needs {prereqs.MagicTheory}");
                }
                Specialization.Upgrade();
                Refine();
            }
            else
            {
                Specialization = new LabSpecialization(activity);
            }
        }
    }
}
