using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Activities;
using WizardMonks.Core;
using WizardMonks.Instances;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Projects;
using WizardMonks.Models.Spells;

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

            int principleIndex = (int)(Die.Instance.RollDouble() * researchablePrinciples.Count);
            object principle = researchablePrinciples[principleIndex];

            return principle switch
            {
                SpellAttribute sa => GenerateForNewAttribute(sa, breakthrough, researcher),
                SpellBase sb => GenerateForNewSpellBase(sb, researcher),
                _ => throw new NotImplementedException($"No research generation handler for type {principle.GetType().Name}")
            };
        }

        private List<object> GetResearchablePrinciples(BreakthroughDefinition breakthrough)
        {
            var pool = new List<object>();
            pool.AddRange(breakthrough.NewSpellAttributes);
            pool.AddRange(breakthrough.NewSpellBases);
            return pool;
        }

        private ResearchProjectPhase GenerateForNewAttribute(SpellAttribute principle, BreakthroughDefinition definition, HermeticMagus researcher)
        {
            ArtPair chosenArts = SelectExperimentalArtPair(definition.AssociatedArtPairs, researcher);
            if (chosenArts == null) return null;

            double labTotal = researcher.GetLabTotal(chosenArts, Activity.OriginalResearch);
            double maxSingleSeasonLevel = Math.Floor(labTotal / 2.0);

            double personalityModifier = (researcher.Personality.GetFacet(HexacoFacet.Creativity) - 1.0) * 5;
            personalityModifier -= (researcher.Personality.GetFacet(HexacoFacet.Prudence) - 1.0) * 5;

            double targetLevel = Math.Max(5, maxSingleSeasonLevel - 5 + personalityModifier);

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
            ApplyInstabilityFactors(ref spell, (ushort)(totalMagnitudesNeeded - currentMagnitudes));

            string experimentalName = $"{researcher.Name}'s {SpellLevelMath.GetMagnitudesFromLevel(spell.Level)}-Mag Experimental {spell.Base.Name}";
            spell = new Spell(spell.Range, spell.Duration, spell.Target, spell.Base, spell.Modifiers, spell.IsRitual, experimentalName);

            return new ResearchProjectPhase(spell, 1);
        }

        private SpellBase FindBestFitSpellBase(ArtPair arts, ushort desiredMagnitude)
        {
            // CORRECTED: Call the now-public GetSpellBasesByArtPair method.
            var bestFit = SpellBases.GetSpellBasesByArtPair(arts)
                .OrderByDescending(b => b.Magnitude)
                .FirstOrDefault(b => b.Magnitude <= desiredMagnitude);

            // CORRECTED: Properly construct the fallback SpellBase.
            return bestFit ?? new SpellBase(
                TechniqueEffects.Detect, // Generic fallback effect
                FormEffects.Aura,       // Generic fallback effect
                ConvertAbilitiesToSpellArts(arts.Technique, arts.Form), // Correctly generate SpellArts flags
                arts,                   // Pass the correct ArtPair object
                SpellTag.Knowledge,
                1,
                "Generic Foundational Effect");
        }

        private ResearchProjectPhase GenerateForNewSpellBase(SpellBase principle, HermeticMagus researcher)
        {
            Spell experimentalSpell = new Spell(EffectRanges.Touch, EffectDurations.Concentration, EffectTargets.Individual, principle, 0, false, "Unnamed Spell");
            ApplyInstabilityFactors(ref experimentalSpell, (ushort)(Die.Instance.RollSimpleDie() / 2));
            return new ResearchProjectPhase(experimentalSpell, 1);
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

        private void ApplyInstabilityFactors(ref Spell spell, ushort magnitudesToAdd)
        {
            if (magnitudesToAdd <= 0) return;

            EffectDuration newDuration = spell.Duration;
            if (magnitudesToAdd >= 2) newDuration = EffectDurations.Sun;
            else if (magnitudesToAdd >= 1) newDuration = EffectDurations.Concentration;

            spell = new Spell(spell.Range, newDuration, spell.Target, spell.Base, spell.Modifiers, spell.IsRitual, spell.Name);
        }

        // Helper method to convert Ability objects to the correct SpellArts flags for the SpellBase constructor.
        private static SpellArts ConvertAbilitiesToSpellArts(Ability technique, Ability form)
        {
            SpellArts techFlag = (SpellArts)Enum.Parse(typeof(SpellArts), technique.AbilityName);
            SpellArts formFlag = (SpellArts)Enum.Parse(typeof(SpellArts), form.AbilityName);
            return techFlag | formFlag;
        }
    }
}