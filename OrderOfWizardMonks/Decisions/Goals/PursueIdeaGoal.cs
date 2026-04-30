using System.Collections.Generic;
using System.Linq;
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
            // Add value based on projected future benefits
            // Placeholder for reputation gain. This makes it forward-compatible.
            double reputationValue = 0; // e.g., ReputationSystem.EstimateGain(magus, _Idea);

            // Placeholder for tangible benefits, like the lab-heating spell example
            double utilityValue = 0; // e.g., UtilitySystem.EstimateValue(_Idea);

            double totalValue = reputationValue + utilityValue;

            // Apply personality modifiers
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

                // TODO: Replace the fallback desire with a properly computed value once
                // CalculateDesire handles BreakthroughIdea. The desire is currently 0 because
                // reputationValue and utilityValue are placeholder zeros, so any non-zero
                // fallback is better than letting this goal starve against other goals.
                double activityDesire = Desire > 0 ? Desire : 0.8;
                alreadyConsidered.Add(new OriginalResearchActivity(
                    project.ProjectId,
                    new ResearchService(),
                    Abilities.MagicTheory,
                    activityDesire));
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
    }
}