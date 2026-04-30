using System.Collections.Generic;
using WizardMonks.Activities;
using WizardMonks.Instances;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Spells;

namespace WizardMonks.Models.Projects
{
    /// <summary>
    /// Defines the breakthrough that produces Parma Magica — Bonisagus's discovery
    /// of a personal ward providing general resistance against magical effects.
    ///
    /// The research is conducted via Rego Vim experimental spells (the natural
    /// art pair for warding effects). Sixty breakthrough points are required,
    /// matching the canonical Hermetic Breakthrough threshold.
    ///
    /// On completion, the breakthrough unlocks the existing ReVi ward spell base
    /// as a fully formalized Hermetic effect, and — outside the simulation's
    /// current scope — the special mechanics of Parma Magica as a seasonal ritual.
    /// </summary>
    public class ParmaMagicaBreakthrough : BreakthroughDefinition
    {
        public ParmaMagicaBreakthrough() : base(
            name: "Parma Magica",
            desc: "A personal ritual that provides general magical resistance, allowing magi to " +
                  "work alongside each other without threatening one another with their Gift. " +
                  "Achieved through extensive Rego Vim experimental research into ward effects.",
            points: 60,
            newAttributes: new List<SpellAttribute>(),
            newSpellBases: new List<SpellBase>
            {
                // The Ward Against Magic spell base drives the experimental phases.
                // Completing the breakthrough formalizes it as a standard Hermetic effect.
                SpellBases.GetSpellBaseForEffect(TechniqueEffects.Ward, FormEffects.Aura)
            },
            newActivities: new List<Activity>(),
            newRefinements: new List<object>(),
            associatedArtPairs: new List<ArtPair> { MagicArtPairs.ReVi }
        )
        {
        }
    }
}
