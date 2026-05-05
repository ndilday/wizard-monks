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
    /// It generates the experimental spells that magi must invent to make progress on their research projects.
    /// This keeps complex procedural generation logic separate from the data models.
    /// </summary>
    public class ResearchService
    {
        public ResearchProjectPhase GenerateExperimentalSpellPhase(BreakthroughDefinition breakthrough, HermeticMagus researcher)
        {
            var researchablePrinciples = GetResearchablePrinciples(breakthrough);
            if (!researchablePrinciples.Any())
            {
                researcher.Log.Add($"[Research] Project '{breakthrough.Name}' has no defined principles to research.");
                return null;
            }

            object principle = SelectPrincipleByProgress(researchablePrinciples, breakthrough, researcher);

            return principle switch
            {
                SpellAttribute sa => GenerateForNewAttribute(sa, breakthrough, researcher),
                SpellBase sb => GenerateForNewSpellBase(sb, researcher),
                Ability => GenerateForNewAbility(breakthrough, researcher),
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
                double weight = Math.Max(0.1, GetLabTotalForPrinciple(principle, breakthrough, researcher));
                weighted.Add((principle, weight));
                total += weight;
            }

            double roll = Die.Instance.RollDouble() * total;
            foreach (var (p, w) in weighted)
            {
                roll -= w;
                if (roll <= 0) return p;
            }
            return principles.Last();
        }

        private double GetLabTotalForPrinciple(object principle, BreakthroughDefinition breakthrough, HermeticMagus researcher)
        {
            if (principle is SpellBase sb)
                return researcher.GetLabTotal(sb.ArtPair, Activity.InventSpells);

            if ((principle is SpellAttribute || principle is Ability) && breakthrough.AssociatedArtPairs.Any())
            {
                return breakthrough.AssociatedArtPairs
                    .Select(pair => researcher.GetLabTotal(pair, Activity.InventSpells))
                    .Max();
            }

            return 1;
        }

        private ResearchProjectPhase GenerateForNewAttribute(SpellAttribute principle, BreakthroughDefinition definition, HermeticMagus researcher)
        {
            ArtPair chosenArts = SelectExperimentalArtPair(definition.AssociatedArtPairs, researcher);
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
            PadSpellToTargetMagnitudes(ref spell, researcher, (ushort)(totalMagnitudesNeeded - currentMagnitudes));

            string experimentalName = $"{researcher.Name}'s {SpellLevelMath.GetMagnitudesFromLevel(spell.Level)}-Mag Experimental {spell.Base.Name}";
            spell = new Spell(spell.Range, spell.Duration, spell.Target, spell.Base, spell.Modifiers, spell.IsRitual, experimentalName);

            return new ResearchProjectPhase(spell, 1);
        }

        /// <summary>
        /// Generates a research phase targeting a new spell base. The RDT parameters are
        /// chosen from those the researcher's tradition already knows, scaled to a target
        /// level derived from the researcher's lab total and personality.
        /// </summary>
        private ResearchProjectPhase GenerateForNewSpellBase(SpellBase principle, HermeticMagus researcher)
        {
            double labTotal = researcher.GetLabTotal(principle.ArtPair, Activity.InventSpells);
            double maxSingleSeasonLevel = Math.Floor(labTotal / 2.0);
            ushort totalMagnitudesNeeded = SpellLevelMath.GetMagnitudesFromLevel(maxSingleSeasonLevel);
            int rdtBudget = totalMagnitudesNeeded - principle.Magnitude;

            EffectRange chosenRange = BestFitRange(researcher, rdtBudget);
            int rangeUsed = chosenRange.Level;

            EffectDuration chosenDuration = BestFitDuration(researcher, rdtBudget - rangeUsed);
            int durationUsed = chosenDuration.Level;

            EffectTarget chosenTarget = BestFitTarget(researcher, rdtBudget - rangeUsed - durationUsed);

            Spell experimentalSpell = new Spell(chosenRange, chosenDuration, chosenTarget, principle, 0, false, "Unnamed Spell");

            ushort currentMagnitudes = (ushort)(principle.Magnitude + chosenRange.Level + chosenDuration.Level + chosenTarget.Level);
            PadSpellToTargetMagnitudes(ref experimentalSpell, researcher, (ushort)(totalMagnitudesNeeded - currentMagnitudes));

            return new ResearchProjectPhase(experimentalSpell, 1);
        }

        /// <summary>
        /// Generates a research phase for a breakthrough whose output is a new ability
        /// rather than a new spell base or attribute. Selects an art pair from the
        /// breakthrough's AssociatedArtPairs and builds an experimental spell in that pair,
        /// using the same level-scaling logic as GenerateForNewSpellBase.
        /// </summary>
        private ResearchProjectPhase GenerateForNewAbility(BreakthroughDefinition breakthrough, HermeticMagus researcher)
        {
            ArtPair chosenArts = SelectExperimentalArtPair(breakthrough.AssociatedArtPairs, researcher);
            if (chosenArts == null) return null;

            double labTotal = researcher.GetLabTotal(chosenArts, Activity.InventSpells);
            double maxSingleSeasonLevel = Math.Floor(labTotal / 2.0);
            ushort totalMagnitudesNeeded = SpellLevelMath.GetMagnitudesFromLevel(maxSingleSeasonLevel);

            var baseEffect = FindBestFitSpellBase(chosenArts, totalMagnitudesNeeded);
            int rdtBudget = totalMagnitudesNeeded - baseEffect.Magnitude;

            EffectRange chosenRange = BestFitRange(researcher, rdtBudget);
            EffectDuration chosenDuration = BestFitDuration(researcher, rdtBudget - chosenRange.Level);
            EffectTarget chosenTarget = BestFitTarget(researcher, rdtBudget - chosenRange.Level - chosenDuration.Level);

            Spell experimentalSpell = new Spell(chosenRange, chosenDuration, chosenTarget, baseEffect, 0, false, "Unnamed Spell");

            ushort currentMagnitudes = (ushort)(baseEffect.Magnitude + chosenRange.Level + chosenDuration.Level + chosenTarget.Level);
            PadSpellToTargetMagnitudes(ref experimentalSpell, researcher, (ushort)(totalMagnitudesNeeded - currentMagnitudes));

            string experimentalName = $"{researcher.Name}'s {SpellLevelMath.GetMagnitudesFromLevel(experimentalSpell.Level)}-Mag Experimental {experimentalSpell.Base.Name}";
            experimentalSpell = new Spell(experimentalSpell.Range, experimentalSpell.Duration, experimentalSpell.Target, experimentalSpell.Base, experimentalSpell.Modifiers, experimentalSpell.IsRitual, experimentalName);

            return new ResearchProjectPhase(experimentalSpell, 1);
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

        private ArtPair SelectExperimentalArtPair(List<ArtPair> candidates, HermeticMagus researcher)
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

        /// <summary>
        /// Pads a spell up toward a target magnitude by upgrading to the highest known
        /// duration that fits within the remaining budget. If no known duration improves
        /// on the spell's current duration, the spell is returned unchanged.
        /// </summary>
        private void PadSpellToTargetMagnitudes(ref Spell spell, HermeticMagus researcher, ushort magnitudesToAdd)
        {
            if (magnitudesToAdd <= 0) return;

            int currentDurationLevel = spell.Duration.Level;
            int maxDurationLevel = currentDurationLevel + magnitudesToAdd;

            EffectDuration best = researcher.Tradition.GetConceptsOfType<DurationPrinciple>()
                .Select(c => ((DurationPrinciple)c.Principle).Duration)
                .Where(d => d.Level > currentDurationLevel && d.Level <= maxDurationLevel)
                .OrderByDescending(d => d.Level)
                .FirstOrDefault();

            if (best != null)
                spell = new Spell(spell.Range, best, spell.Target, spell.Base, spell.Modifiers, spell.IsRitual, spell.Name);
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
