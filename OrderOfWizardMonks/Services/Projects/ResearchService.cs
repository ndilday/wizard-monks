using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Activities;
using WizardMonks.Core;
using WizardMonks.Instances;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Projects;
using WizardMonks.Models.Spells;
using WizardMonks.Models.Traditions;

namespace WizardMonks.Services.Characters
{
    /// <summary>
    /// A service dedicated to the logic of magical research and breakthroughs.
    /// It selects the experimental spells that magi must invent to make progress on their research projects.
    /// This keeps complex procedural generation logic separate from the data models.
    /// </summary>
    public class ResearchService
    {
        /// <summary>
        /// Selects the experimental spell a researcher should work on next for the given breakthrough.
        /// Returns null if no principle is within the researcher's current lab total capability.
        /// Call this at planning time; store the result on the activity so the decision is made
        /// before the season begins, not lazily during execution.
        /// </summary>
        public Spell SelectExperimentalSpell(BreakthroughDefinition breakthrough, HermeticMagus researcher)
        {
            var researchablePrinciples = GetResearchablePrinciples(breakthrough);
            if (!researchablePrinciples.Any())
            {
                researcher.Log.Add($"[Research] Project '{breakthrough.Name}' has no defined principles to research.");
                return null;
            }

            object principle = SelectPrincipleByProgress(researchablePrinciples, breakthrough, researcher);

            if (principle == null)
            {
                researcher.Log.Add($"[Research] No principles in '{breakthrough.Name}' are within current lab total capabilities.");
                return null;
            }

            return principle switch
            {
                SpellAttribute sa => GenerateSpellForNewAttribute(sa, breakthrough, researcher),
                SpellBase sb => GenerateSpellForNewSpellBase(sb, researcher),
                Ability => GenerateSpellForNewAbility(breakthrough, researcher),
                _ => throw new NotImplementedException($"No research generation handler for type {principle.GetType().Name}")
            };
        }

        private List<object> GetResearchablePrinciples(BreakthroughDefinition breakthrough)
        {
            var pool = new List<object>();
            pool.AddRange(breakthrough.NewSpellAttributes);
            pool.AddRange(breakthrough.NewSpellBases);
            foreach (var tag in breakthrough.ResearchTags)
                pool.AddRange(SpellBases.GetSpellBasesByTag(tag).Cast<object>());
            pool.AddRange(breakthrough.NewAbilities);
            return pool;
        }

        /// <summary>
        /// Selects a principle to research, weighted by how much progress the researcher
        /// can make per season on each candidate.
        /// </summary>
        private object SelectPrincipleByProgress(List<object> principles, BreakthroughDefinition breakthrough, HermeticMagus researcher)
        {
            var weighted = new List<(object principle, double weight)>();
            double total = 0;

            foreach (var principle in principles)
            {
                double weight = GetLabTotalForPrinciple(principle, breakthrough, researcher);
                if (weight <= 0) continue;
                weighted.Add((principle, weight));
                total += weight;
            }

            if (!weighted.Any()) return null;

            double roll = Die.Instance.RollDouble() * total;
            foreach (var (p, w) in weighted)
            {
                roll -= w;
                if (roll <= 0) return p;
            }
            return weighted.Last().principle;
        }

        private double GetLabTotalForPrinciple(object principle, BreakthroughDefinition breakthrough, HermeticMagus researcher)
        {
            if (principle is SpellBase sb)
            {
                double labTotal = researcher.GetLabTotal(sb.ArtPair, Activity.InventSpells);
                double maxSingleSeasonLevel = Math.Floor(labTotal / 2.0);
                ushort maxMagnitudes = SpellLevelMath.GetMagnitudesFromLevel(maxSingleSeasonLevel);
                if (sb.Magnitude > maxMagnitudes) return 0;
                return labTotal;
            }

            if (principle is SpellAttribute || principle is Ability)
            {
                var pairs = GetResearchableArtPairs(breakthrough, researcher);
                if (pairs.Any())
                    return pairs.Max(p => researcher.GetLabTotal(p, Activity.InventSpells));
            }

            return 1;
        }

        /// <summary>
        /// Returns the art pairs relevant to researching this breakthrough.
        /// For tag- and spell-base-driven breakthroughs these are derived from the
        /// matching spell bases. For attribute-only breakthroughs (where any TeFo
        /// is valid) the researcher's own tradition's known spell bases are used.
        /// </summary>
        public IReadOnlyList<ArtPair> GetResearchableArtPairs(BreakthroughDefinition breakthrough, HermeticMagus researcher)
        {
            var pairs = breakthrough.ResearchTags
                .SelectMany(SpellBases.GetSpellBasesByTag)
                .Concat(breakthrough.NewSpellBases)
                .Select(sb => sb.ArtPair)
                .Distinct()
                .ToList();

            if (!pairs.Any() && breakthrough.NewSpellAttributes.Any())
            {
                pairs = researcher.Tradition.GetKnownSpellBases()
                    .Select(sb => sb.ArtPair)
                    .Distinct()
                    .ToList();
            }

            return pairs;
        }

        /// <summary>
        /// Returns the highest lab total the researcher can bring to bear on
        /// any of the breakthrough's researchable art pairs.
        /// </summary>
        public double GetBestLabTotal(BreakthroughDefinition breakthrough, HermeticMagus researcher)
        {
            var pairs = GetResearchableArtPairs(breakthrough, researcher);
            if (!pairs.Any()) return 1.0;
            return Math.Max(1, pairs.Max(p => researcher.GetLabTotal(p, Activity.InventSpells)));
        }

        private Spell GenerateSpellForNewAttribute(SpellAttribute principle, BreakthroughDefinition definition, HermeticMagus researcher)
        {
            ArtPair chosenArts = SelectExperimentalArtPair(GetResearchableArtPairs(definition, researcher), researcher);
            if (chosenArts == null) return null;

            double labTotal = researcher.GetLabTotal(chosenArts, Activity.InventSpells);
            double maxSingleSeasonLevel = Math.Floor(labTotal / 2.0);
            double targetLevel = Math.Max(5, maxSingleSeasonLevel - 5);

            ushort totalMagnitudesNeeded = SpellLevelMath.GetMagnitudesFromLevel(targetLevel);
            ushort principleMagnitudes = principle.Level;
            int baseEffectMagnitudesNeeded = totalMagnitudesNeeded - principleMagnitudes;
            if (baseEffectMagnitudesNeeded < 1) baseEffectMagnitudesNeeded = 1;

            var baseEffect = FindBestFitSpellBase(chosenArts, (ushort)baseEffectMagnitudesNeeded);

            var spell = new Spell(EffectRanges.Touch, EffectDurations.Instant, EffectTargets.Individual, baseEffect, 0, false, "Unnamed Spell");

            if (principle is EffectRange range) spell = new Spell(range, spell.Duration, spell.Target, spell.Base, 0, false, spell.Name);
            if (principle is EffectDuration duration) spell = new Spell(spell.Range, duration, spell.Target, spell.Base, 0, false, spell.Name);
            if (principle is EffectTarget target) spell = new Spell(spell.Range, spell.Duration, target, spell.Base, 0, false, spell.Name);

            ushort currentMagnitudes = (ushort)(baseEffect.Magnitude + principle.Level);
            PadSpellToTargetMagnitudes(ref spell, (ushort)(totalMagnitudesNeeded - currentMagnitudes));

            string experimentalName = $"{researcher.Name}'s {SpellLevelMath.GetMagnitudesFromLevel(spell.Level)}-Mag Experimental {spell.Base.Name}";
            return new Spell(spell.Range, spell.Duration, spell.Target, spell.Base, spell.Modifiers, spell.IsRitual, experimentalName);
        }

        /// <summary>
        /// Generates a spell targeting a new spell base. The RDT parameters are
        /// chosen from those the researcher's tradition already knows, scaled to a target
        /// level derived from the researcher's lab total.
        /// </summary>
        private Spell GenerateSpellForNewSpellBase(SpellBase principle, HermeticMagus researcher)
        {
            double labTotal = researcher.GetLabTotal(principle.ArtPair, Activity.InventSpells);
            double maxSingleSeasonLevel = Math.Floor(labTotal / 2.0);
            ushort totalMagnitudesNeeded = SpellLevelMath.GetMagnitudesFromLevel(maxSingleSeasonLevel);
            int rdtBudget = totalMagnitudesNeeded - principle.Magnitude;

            EffectDuration chosenDuration = BestFitDuration(researcher, rdtBudget);
            int durationUsed = chosenDuration.Level;

            EffectRange chosenRange = BestFitRange(researcher, rdtBudget - durationUsed);
            int rangeUsed = chosenRange.Level;

            EffectTarget chosenTarget = BestFitTarget(researcher, rdtBudget - rangeUsed - durationUsed);

            Spell experimentalSpell = new Spell(chosenRange, chosenDuration, chosenTarget, principle, 0, false, "Unnamed Spell");

            ushort currentMagnitudes = (ushort)(principle.Magnitude + chosenRange.Level + chosenDuration.Level + chosenTarget.Level);
            PadSpellToTargetMagnitudes(ref experimentalSpell, (ushort)(totalMagnitudesNeeded - currentMagnitudes));

            return experimentalSpell;
        }

        /// <summary>
        /// Generates a spell for a breakthrough whose output is a new ability.
        /// The experimental effect is drawn from the same tag-based pool used for
        /// general principle selection, so the art pair is determined by whichever
        /// tagged spell base the researcher is best positioned to invent.
        /// </summary>
        private Spell GenerateSpellForNewAbility(BreakthroughDefinition breakthrough, HermeticMagus researcher)
        {
            var candidates = breakthrough.ResearchTags
                .SelectMany(SpellBases.GetSpellBasesByTag)
                .Concat(breakthrough.NewSpellBases)
                .Cast<object>()
                .ToList();

            if (!(SelectPrincipleByProgress(candidates, breakthrough, researcher) is SpellBase chosenBase))
                return null;

            double labTotal = researcher.GetLabTotal(chosenBase.ArtPair, Activity.InventSpells);
            double maxSingleSeasonLevel = Math.Floor(labTotal / 2.0);
            ushort totalMagnitudesNeeded = SpellLevelMath.GetMagnitudesFromLevel(maxSingleSeasonLevel);
            int rdtBudget = totalMagnitudesNeeded - chosenBase.Magnitude;

            EffectRange chosenRange = BestFitRange(researcher, rdtBudget);
            EffectDuration chosenDuration = BestFitDuration(researcher, rdtBudget - chosenRange.Level);
            EffectTarget chosenTarget = BestFitTarget(researcher, rdtBudget - chosenRange.Level - chosenDuration.Level);

            Spell experimentalSpell = new Spell(chosenRange, chosenDuration, chosenTarget, chosenBase, 0, false, "Unnamed Spell");

            ushort currentMagnitudes = (ushort)(chosenBase.Magnitude + chosenRange.Level + chosenDuration.Level + chosenTarget.Level);
            PadSpellToTargetMagnitudes(ref experimentalSpell, (ushort)(totalMagnitudesNeeded - currentMagnitudes));

            string experimentalName = $"{researcher.Name}'s {SpellLevelMath.GetMagnitudesFromLevel(experimentalSpell.Level)}-Mag Experimental {experimentalSpell.Base.Name}";
            return new Spell(experimentalSpell.Range, experimentalSpell.Duration, experimentalSpell.Target, experimentalSpell.Base, experimentalSpell.Modifiers, experimentalSpell.IsRitual, experimentalName);
        }

        /// <summary>
        /// Applies a completed breakthrough's effects to the researcher: adds new abilities
        /// to their tradition, spell bases, and lab activities as defined by the breakthrough.
        /// </summary>
        public void ApplyBreakthroughEffects(BreakthroughDefinition breakthrough, HermeticMagus researcher)
        {
            if (breakthrough.NewAbilities.Count == 0) return;

            double xpPerAbility = (double)breakthrough.BreakthroughPointsRequired / breakthrough.NewAbilities.Count;
            foreach (var ability in breakthrough.NewAbilities)
            {
                researcher.UnlockTraditionAbility(ability);
                researcher.GetAbility(ability).AddExperience(xpPerAbility);
                researcher.Log.Add($"[Breakthrough] Unlocked {ability.AbilityName}, gained {xpPerAbility:F0} XP.");
            }
        }

        private SpellBase FindBestFitSpellBase(ArtPair arts, ushort desiredMagnitude)
        {
            var bases = SpellBases.GetSpellBasesByArtPair(arts);
            var bestFit = bases?
                .OrderByDescending(b => b.Magnitude)
                .FirstOrDefault(b => b.Magnitude <= desiredMagnitude);

            if (bestFit == null)
                throw new InvalidOperationException(
                    $"No spell base found for art pair {arts.Technique.AbilityName}/{arts.Form.AbilityName} " +
                    $"at or below magnitude {desiredMagnitude}. Add a spell base for this pair to SpellBases.");

            return bestFit;
        }

        private ArtPair SelectExperimentalArtPair(IReadOnlyList<ArtPair> candidates, HermeticMagus researcher)
        {
            if (candidates == null || !candidates.Any()) return null;

            var weightedCandidates = new List<(ArtPair pair, double weight)>();
            double totalWeight = 0;

            foreach (var pair in candidates)
            {
                double weight = (researcher.GetAbility(pair.Technique).Value + researcher.GetAbility(pair.Form).Value + 1);
                weightedCandidates.Add((pair, weight));
                totalWeight += weight;
            }

            double roll = Die.Instance.RollDouble() * totalWeight;

            foreach (var (pair, weight) in weightedCandidates)
            {
                roll -= weight;
                if (roll <= 0)
                {
                    return pair;
                }
            }
            return candidates.Last();
        }

        private void PadSpellToTargetMagnitudes(ref Spell spell, ushort magnitudesToAdd)
        {
            if (magnitudesToAdd <= 0) return;

            byte newAdditionalLevels = (byte)(spell.AdditionalLevels + magnitudesToAdd);
            spell = new Spell(spell.Range, spell.Duration, spell.Target, spell.Base,
                spell.Modifiers, spell.IsRitual, spell.Name, newAdditionalLevels);
        }

        /// <summary>
        /// Returns the highest-magnitude range the researcher's tradition knows that fits
        /// within the given budget. Falls back to Personal (0 magnitudes) if none fit.
        /// </summary>
        private EffectRange BestFitRange(HermeticMagus researcher, int magnitudeBudget)
        {
            return researcher.Tradition.GetConceptsOfType<RangePrinciple>()
                .Select(c => ((RangePrinciple)c.Principle).Range)
                .Append(EffectRanges.Personal)
                .Where(r => r.Level <= magnitudeBudget)
                .OrderByDescending(r => r.Level)
                .First();
        }

        /// <summary>
        /// Returns the highest-magnitude duration the researcher's tradition knows that fits
        /// within the given budget. Falls back to Instant (0 magnitudes) if none fit.
        /// </summary>
        private EffectDuration BestFitDuration(HermeticMagus researcher, int magnitudeBudget)
        {
            return researcher.Tradition.GetConceptsOfType<DurationPrinciple>()
                .Select(c => ((DurationPrinciple)c.Principle).Duration)
                .Append(EffectDurations.Instant)
                .Where(d => d.Level <= magnitudeBudget)
                .OrderByDescending(d => d.Level)
                .First();
        }

        /// <summary>
        /// Returns the highest-magnitude target the researcher's tradition knows that fits
        /// within the given budget. Falls back to Individual (0 magnitudes) if none fit.
        /// </summary>
        private EffectTarget BestFitTarget(HermeticMagus researcher, int magnitudeBudget)
        {
            return researcher.Tradition.GetConceptsOfType<TargetPrinciple>()
                .Select(c => ((TargetPrinciple)c.Principle).Target)
                .Append(EffectTargets.Individual)
                .Where(t => t.Level <= magnitudeBudget)
                .OrderByDescending(t => t.Level)
                .First();
        }

        private static SpellArts ConvertAbilitiesToSpellArts(Ability technique, Ability form)
        {
            SpellArts techFlag = (SpellArts)Enum.Parse(typeof(SpellArts), technique.AbilityName);
            SpellArts formFlag = (SpellArts)Enum.Parse(typeof(SpellArts), form.AbilityName);
            return techFlag | formFlag;
        }
    }
}
