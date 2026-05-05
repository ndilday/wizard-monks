using System.Collections.Generic;
using WizardMonks.Activities;
using WizardMonks.Instances;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Spells;

namespace WizardMonks.Models.Projects
{
    /// <summary>
    /// Defines the breakthrough that produces Parma Magica — Bonisagus's discovery
    /// of a personal ritual providing general resistance against magical effects.
    ///
    /// The research is conducted via Rego experimental spells across all ten Forms,
    /// reflecting Bonisagus's exploration of magical resistance in every domain.
    /// Sixty breakthrough points are required, matching the canonical Hermetic
    /// Breakthrough threshold.
    ///
    /// On completion, the Parma Magica ability is added to the researcher's tradition,
    /// allowing it to be taught to and learned by other magi.
    /// </summary>
    public class ParmaMagicaBreakthrough : BreakthroughDefinition
    {
        public ParmaMagicaBreakthrough() : base(
            name: "Parma Magica",
            desc: "A personal ritual that provides general magical resistance, allowing magi to " +
                  "work alongside each other without threatening one another with their Gift. " +
                  "Achieved through extensive experimental research into Rego ward effects across all Forms.",
            points: 60,
            newAttributes: new List<SpellAttribute>(),
            newSpellBases: new List<SpellBase>(),
            newActivities: new List<Activity>(),
            newRefinements: new List<object>(),
            associatedArtPairs: new List<ArtPair>
            {
                MagicArtPairs.ReAn, MagicArtPairs.ReAq, MagicArtPairs.ReAu,
                MagicArtPairs.ReCo, MagicArtPairs.ReHe, MagicArtPairs.ReIg,
                MagicArtPairs.ReIm, MagicArtPairs.ReMe, MagicArtPairs.ReTe,
                MagicArtPairs.ReVi
            },
            newAbilities: new List<Ability> { Abilities.ParmaMagica },
            researchTags: new List<SpellTag> { SpellTag.Protection }
        )
        {
        }
    }
}
