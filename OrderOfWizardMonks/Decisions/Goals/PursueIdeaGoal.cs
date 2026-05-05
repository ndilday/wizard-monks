using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Activities;
using WizardMonks.Activities.MageActivities;
using WizardMonks.Decisions.Conditions.Helpers;
using WizardMonks.Instances;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Ideas;
using WizardMonks.Models.Projects;
using WizardMonks.Models.Spells;
using WizardMonks.Services.Characters;

namespace WizardMonks.Decisions.Goals
{
    public class PursueIdeaGoal : AGoal
    {
        public AIdea Idea { get; }
        private Spell _targetSpell; // The concrete spell we decide to invent

        public PursueIdeaGoal(HermeticMagus magus, AIdea idea, uint? ageToCompleteBy = null)
            : base(magus, ageToCompleteBy, 0) // Desire is calculated dynamically
        {
            Idea = idea;
            Desire = CalculateDesire(magus);
        }

        private double CalculateDesire(HermeticMagus magus)
        {
            // Base value by idea type.
            // BreakthroughIdeas represent major theoretical discoveries whose value is
            // high and intrinsic — they don't depend on utility or reputation calculations.
            // 0.6 before personality scaling ensures breakthrough research competes
            // successfully with instinctive self-preservation goals (longevity ritual
            // bootstraps at desire ≈ 1.0 × Prudence multiplier ≈ 1.1 for most magi).
            // TODO: Scale BreakthroughIdea base by proximity to completion and expected
            //       seasons remaining once ResearchProject lookup is available here.
            double baseValue = Idea is BreakthroughIdea ? 0.6 : 0.0;

            // Placeholder for reputation gain. This makes it forward-compatible.
            double reputationValue = 0; // e.g., ReputationSystem.EstimateGain(magus, _Idea);

            // Placeholder for tangible benefits (e.g., a spell that heats the lab in winter).
            double utilityValue = 0; // e.g., UtilitySystem.EstimateValue(_Idea);

            double totalValue = baseValue + reputationValue + utilityValue;

            // Apply personality modifiers. A mage high in Creativity and Inquisitiveness
            // pursues breakthrough research with greater urgency.
            totalValue *= magus.Personality.GetDesireMultiplier(HexacoFacet.Creativity);
            totalValue *= magus.Personality.GetDesireMultiplier(HexacoFacet.Inquisitiveness);

            return totalValue;
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, Desires desires, IList<string> log)
        {
            if (_completed) return;

            var magus = (HermeticMagus)Character;
            if (Idea is BreakthroughIdea breakthroughIdea)
            {
                // Find an existing project for this breakthrough, or create one if absent.
                var project = magus.ActiveProjects
                    .OfType<ResearchProject>()
                    .FirstOrDefault(p => p.Breakthrough.Id == breakthroughIdea.TargetBreakthrough.Id);

                if (project == null)
                {
                    project = new ResearchProject(magus, breakthroughIdea.TargetBreakthrough);
                    magus.ActiveProjects.Add(project);
                    magus.Log.Add($"Began tracking original research project: {project.Description}");
                }

                if (project.IsComplete)
                {
                    _completed = true;
                    return;
                }

                var artPairs = breakthroughIdea.TargetBreakthrough.AssociatedArtPairs;
                double remaining = Math.Max(1, project.BreakthroughPointsRequired - project.BreakthroughPointsAccumulated);
                double bestLabTotal = (artPairs != null && artPairs.Count > 0)
                    ? Math.Max(1, artPairs.Max(p => magus.GetLabTotal(p, Activity.InventSpells)))
                    : 1.0;
                double totalSeasonsNeeded = Math.Max(1, 20.0 * remaining / bestLabTotal);

                // One season of direct research always reduces remaining by exactly 1 season.
                alreadyConsidered.Add(new OriginalResearchActivity(
                    project.ProjectId,
                    new ResearchService(),
                    Abilities.MagicTheory,
                    Desire / totalSeasonsNeeded));

                if (artPairs != null && artPairs.Count > 0)
                    ConsiderIncreasingResearchTotal(alreadyConsidered, desires, log,
                        artPairs, remaining, bestLabTotal, totalSeasonsNeeded, magus);
            }
            else if (Idea is SpellIdea spellIdea)
            {
                // If we haven't defined a target spell yet, create one
                if (_targetSpell == null)
                {
                    // For now, invent a simple, generic spell based on the inspired Arts.
                    // This logic can become more sophisticated.
                    _targetSpell = new Spell(
                        EffectRanges.Touch, EffectDurations.Sun, EffectTargets.Individual,
                        new SpellBase(TechniqueEffects.Manipulate, FormEffects.Animal, SpellArts.Rego | SpellArts.Animal, spellIdea.Arts, SpellTag.Utility, 2, "Inspired Spell"),
                        0, false, $"Invention from {magus.Name}'s Insight");
                }

                // Check if the spell is already known
                if (magus.SpellList.Contains(_targetSpell))
                {
                    _completed = true;
                    return;
                }

                // Use a helper to plan the invention of the target spell
                var spellHelper = new LearnSpellHelper(magus, magus.SeasonalAge, 1, _targetSpell.Base,
                    (gain, depth) => this.Desire * (gain / _targetSpell.Level));
                spellHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);
            }
        }

        private void ConsiderIncreasingResearchTotal(
            ConsideredActions alreadyConsidered, Desires desires, IList<string> log,
            IReadOnlyList<ArtPair> artPairs,
            double remaining, double bestLabTotal, double totalSeasonsNeeded,
            HermeticMagus magus)
        {
            uint planningHorizon = (uint)(magus.SeasonalAge + totalSeasonsNeeded + 1);

            CalculateDesireFunc desireFunc = (gain, depth) =>
            {
                double deltaSeasons = 20.0 * remaining * gain / (bestLabTotal * (bestLabTotal + gain));
                double netDeltaSeasons = deltaSeasons - depth;
                if (netDeltaSeasons <= 0) return 0;
                return Desire * netDeltaSeasons / totalSeasonsNeeded;
            };

            var helper = new MultiPairLabTotalIncreaseHelper(
                magus, planningHorizon, 1, artPairs, Activity.InventSpells, desireFunc);
            helper.AddActionPreferencesToList(alreadyConsidered, desires, log);
        }
    }
}