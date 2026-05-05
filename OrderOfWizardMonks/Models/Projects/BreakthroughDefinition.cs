using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Activities;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Spells;
using WizardMonks.Models.Traditions;


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
        public List<Ability> NewAbilities { get; private set; }

        public List<ArtPair> AssociatedArtPairs { get; private set; }

        /// <summary>
        /// Tags used to dynamically select spell bases for experimental research from the
        /// global registry, instead of curating a fixed NewSpellBases list. Any spell base
        /// whose Tags field contains any of these tags becomes a valid research target.
        /// </summary>
        public List<SpellTag> ResearchTags { get; private set; }

        /// <summary>
        /// Returns true if all of this breakthrough's outputs are already present
        /// in the given tradition, meaning the mage who holds it has no need to
        /// research or gain points toward it.
        /// </summary>
        public bool IsIntegratedInto(MagicalTradition tradition)
        {
            return NewAbilities.All(ability =>
                tradition.GetConceptsOfType<MagicalAbilityPrinciple>()
                    .Any(c => ((MagicalAbilityPrinciple)c.Principle).Ability.AbilityId == ability.AbilityId));
        }

        protected BreakthroughDefinition(string name, string desc, ushort points,
            List<SpellAttribute> newAttributes, List<SpellBase> newSpellBases, List<Activity> newActivities, List<object> newRefinements,
            List<ArtPair> associatedArtPairs, List<Ability> newAbilities = null,
            List<SpellTag> researchTags = null)
        {
            Name = name;
            Description = desc;
            BreakthroughPointsRequired = points;
            NewSpellAttributes = newAttributes;
            NewSpellBases = newSpellBases;
            NewLabActivities = newActivities;
            PrincipleRefinements = newRefinements;
            AssociatedArtPairs = associatedArtPairs;
            NewAbilities = newAbilities ?? new List<Ability>();
            ResearchTags = researchTags ?? new List<SpellTag>();
        }
    }
}
