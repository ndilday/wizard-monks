using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks
{
    public class Feature
    {
        public double Refinement { get; protected set; }
        public double Aesthetics { get; protected set; }
        public double Quality { get; protected set; }
        public double Safety { get; protected set; }
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
            Warping = 0;
        }
    }

    public class Laboratory : Feature
    {
        public double Size { get; private set; }

        private List<Feature> _refinements;
        private Magus _owner;

        private double _availableRefinement;

        public Laboratory(Magus owner, double size) : base()
        {
            _refinements = new List<Feature>();
            _owner = owner;
            Size = size;
            _availableRefinement = size;
        }

        public double GetModifier(Ability technique, Ability form, Activity activity)
        {
            return 0;
        }

        public void Refine()
        {
            Refinement++;
            _availableRefinement++;
        }

        public void AddFeature(Feature refinement)
        {
            _refinements.Add(refinement);
            foreach (KeyValuePair<Ability, double> artModifier in refinement.ArtModifiers)
            {
                ArtModifiers[artModifier.Key] += artModifier.Value;
            }
            foreach (KeyValuePair<Activity, double> activityModifier in refinement.ActivityModifiers)
            {
                ActivityModifiers[activityModifier.Key] += activityModifier.Value;
            }
        }

        public void RemoveFeature(Feature refinement)
        {
            if (_refinements.Contains(refinement))
            {
                _refinements.Remove(refinement);
                foreach (KeyValuePair<Ability, double> artModifier in refinement.ArtModifiers)
                {
                    ArtModifiers[artModifier.Key] -= artModifier.Value;
                }
                foreach (KeyValuePair<Activity, double> activityModifier in refinement.ActivityModifiers)
                {
                    ActivityModifiers[activityModifier.Key] -= activityModifier.Value;
                }
            }
        }
    }
}
