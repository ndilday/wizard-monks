using System;
using System.Collections.Generic;
using WizardMonks.Activities;

namespace WizardMonks.Models
{
    public class LabFeature
    {
        public string Name { get; protected set; }
        public byte Size { get; protected set; }
        public double Aesthetics { get; protected set; }
        public double Health { get; protected set; }
        public double Quality { get; protected set; }
        public double Safety { get; protected set; }
        public double Upkeep { get; protected set; }
        public double Warping { get; protected set; }
        public Tuple<Ability, double> ArtModifier { get; protected set; }
        public Tuple<Activity, double> ActivityModifier { get; protected set; }

        public LabFeature(string name, byte size, double aesthetics, double health, double quality, double safety, double upkeep, double warping, Tuple<Ability, double> artMod, Tuple<Activity, double> activityMod)
        {
            Name = name;
            Size = size;
            Aesthetics = aesthetics;
            Health = health;
            Quality = quality;
            Safety = safety;
            Upkeep = upkeep;
            Warping = warping;
            ArtModifier = artMod;
            ActivityModifier = activityMod;
        }
    }
}
