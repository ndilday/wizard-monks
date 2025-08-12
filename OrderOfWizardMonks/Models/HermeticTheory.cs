using System;
using System.Collections.Generic;
using WizardMonks.Activities;
using WizardMonks.Models.Spells;

namespace WizardMonks.Models
{
    public class HermeticTheory
    {
        private string _lineage;
        private ushort _version;

        public string Lineage
        {
            get
            {
                return $"{_lineage}:{MageName}{_version}";
            }
        }

        // Core Spell Parameters (What is known to be possible)

        public string MageName { get; set; }
        public HashSet<Ranges> KnownRanges { get; private set; }
        public HashSet<Targets> KnownTargets { get; private set; }
        public HashSet<Durations> KnownDurations { get; private set; }

        // Repertoire of Effects (The "recipes" of magic)
        public HashSet<SpellBase> KnownSpellBases { get; private set; }

        // Methodologies (How magic is practiced)
        public HashSet<Activity> KnownLabActivities { get; private set; }
        public HashSet<Ability> KnownHermeticAbilities { get; private set; }

        // Foundational Mechanics (The core rules of this theory)
        public bool RitualMagicIntegrated { get; private set; }
        public double SpontaneousMagicMultiplier { get; private set; }
        public bool ArcaneConnectionsIntegrated { get; private set; }

        // Specializations & Advanced Concepts
        //public HashSet<HermeticVirtue> KnownVirtues { get; private set; } // Requires a HermeticVirtue class

        // Philosophical Underpinnings (The safety protocols)
        //public HashSet<LimitOfMagic> UnderstoodLimits { get; private set; }

        public HermeticTheory(string lineage)
        {
            _lineage = lineage;
            _version = 1;

            // Initialize all collections
            KnownRanges = [];
            KnownTargets = [];
            KnownDurations = [];
            KnownSpellBases = [];
            KnownLabActivities = [];
            KnownHermeticAbilities = [];
            RitualMagicIntegrated = false;
            ArcaneConnectionsIntegrated = false;
            SpontaneousMagicMultiplier = 0;
        }

        public HermeticTheory CloneTheory()
        {
            HermeticTheory newTheory = new(Lineage);
            newTheory.KnownRanges = new(this.KnownRanges);
            newTheory.KnownTargets = new(this.KnownTargets);
            newTheory.KnownDurations = new(this.KnownDurations);
            newTheory.KnownSpellBases = new(this.KnownSpellBases);
            newTheory.KnownLabActivities = new(this.KnownLabActivities);
            newTheory.KnownHermeticAbilities = new(this.KnownHermeticAbilities);
            newTheory.RitualMagicIntegrated = this.RitualMagicIntegrated;
            newTheory.SpontaneousMagicMultiplier = this.SpontaneousMagicMultiplier;
            newTheory.ArcaneConnectionsIntegrated = this.ArcaneConnectionsIntegrated;

            return newTheory;
        }

        // A method to merge another theory into this one, creating a new version.
        public void LearnFrom(HermeticTheory otherTheory)
        {
            if (otherTheory == null)
            {
                throw new ArgumentNullException(nameof(otherTheory), "Cannot learn from a null theory.");
            }
            _version += 1;
            // Merge known ranges, targets, and durations
            KnownRanges.UnionWith(otherTheory.KnownRanges);
            KnownTargets.UnionWith(otherTheory.KnownTargets);
            KnownDurations.UnionWith(otherTheory.KnownDurations);
            // Merge spell bases
            KnownSpellBases.UnionWith(otherTheory.KnownSpellBases);
            // Merge lab activities and abilities
            KnownLabActivities.UnionWith(otherTheory.KnownLabActivities);
            KnownHermeticAbilities.UnionWith(otherTheory.KnownHermeticAbilities);
            // Update foundational mechanics
            RitualMagicIntegrated |= otherTheory.RitualMagicIntegrated;
            SpontaneousMagicMultiplier = Math.Max(SpontaneousMagicMultiplier, otherTheory.SpontaneousMagicMultiplier);
            ArcaneConnectionsIntegrated |= otherTheory.ArcaneConnectionsIntegrated;
        }
    }
}