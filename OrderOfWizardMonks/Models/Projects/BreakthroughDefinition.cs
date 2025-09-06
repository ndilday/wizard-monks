using System;
using System.Collections.Generic;
using WizardMonks.Activities;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Spells;


namespace WizardMonks.Models.Projects
{
    public abstract class BreakthroughDefinition
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Name { get; private set; }
        public string Description { get; private set; }
        public ushort BreakthroughPointsRequired { get; private set; }

        public List<SpellAttribute> NewSpellAttributes { get; private set; }
        public List<SpellBase> NewSpellBases { get; private set; }
        public List<Activity> NewLabActivities { get; private set; }
        public List<object> PrincipleRefinements { get; private set; }

        public List<ArtPair> AssociatedArtPairs { get; private set; }

        protected BreakthroughDefinition(string name, string desc, ushort points, 
            List<SpellAttribute> newAttributes, List<SpellBase> newSpellBases, List<Activity> newActivities, List<object> newRefinements,
            List<ArtPair> associatedArtPairs)
        {
            Name = name;
            Description = desc;
            BreakthroughPointsRequired = points;
            NewSpellAttributes = newAttributes;
            NewSpellBases = newSpellBases;
            NewLabActivities = newActivities;
            PrincipleRefinements = newRefinements;
            AssociatedArtPairs = associatedArtPairs;
        }
    }
}
