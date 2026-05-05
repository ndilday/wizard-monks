using System;
using System.Linq;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Events;
using WizardMonks.Models.Projects;
using WizardMonks.Models.Spells;
using WizardMonks.Services.Characters;

namespace WizardMonks.Activities.MageActivities
{
    [Serializable]
    public class OriginalResearchActivity : AExposingMageActivity
    {
        public Guid ProjectId { get; private set; }
        public Spell ExperimentalSpell { get; private set; }

        private readonly ResearchService _researchService;

        public OriginalResearchActivity(Guid projectId, Spell experimentalSpell, ResearchService researchService, Ability exposure, double desire)
            : base(exposure, desire)
        {
            ProjectId = projectId;
            ExperimentalSpell = experimentalSpell ?? throw new ArgumentNullException(nameof(experimentalSpell));
            _researchService = researchService ?? throw new ArgumentNullException(nameof(researchService));
            Action = Activity.InventSpells;
        }

        protected override void DoMageAction(HermeticMagus mage)
        {
            var project = mage.ActiveProjects.OfType<ResearchProject>().FirstOrDefault(p => p.ProjectId == ProjectId);
            if (project == null)
            {
                mage.Log.Add("Attempted to work on a non-existent research project.");
                return;
            }

            if (project.CurrentPhase == null)
            {
                project.StartNewPhase(new ResearchProjectPhase(ExperimentalSpell, 1));
                mage.Log.Add($"Began experimental effect phase on '{project.Description}'.");
            }

            var phase = project.CurrentPhase;

            if (!phase.IsInvented)
            {
                double labTotal = mage.GetSpellLabTotal(phase.ExperimentalSpell);
                double progress = labTotal - phase.ExperimentalSpell.Level;
                if (progress <= 0)
                {
                    mage.Log.Add($"Lab Total is too low to make progress on experimental spell '{phase.ExperimentalSpell.Name}'.");
                    return;
                }
                phase.AddInventionProgress(progress);
                mage.Log.Add($"Advanced research on '{phase.ExperimentalSpell.Name}'. Progress: {phase.InventionProgress:F1}/{phase.ExperimentalSpell.Level:F0}");
                if (phase.IsInvented)
                {
                    mage.LearnSpell(phase.ExperimentalSpell, project.Breakthrough);
                    mage.Log.Add($"Experimental spell '{phase.ExperimentalSpell.Name}' has been successfully invented!");
                    EmittedEvent = WorldEvent.LabOutcome(
                        (int)mage.SeasonalAge, WorldEventCategory.LabSuccess, mage, (float)progress, phase.ExperimentalSpell.Name, true);
                }
            }
            else if (!phase.IsStabilized)
            {
                phase.WorkOnStabilization();
                mage.Log.Add($"Worked on stabilizing '{phase.ExperimentalSpell.Name}'. Seasons remaining: {phase.SeasonsToStabilize}");
                if (phase.IsStabilized)
                {
                    mage.Log.Add($"Research phase complete! Gained {phase.BreakthroughPointsGained} breakthrough points.");
                    project.CompleteCurrentPhase();

                    if (project.BreakthroughPointsAccumulated >= project.BreakthroughPointsRequired)
                    {
                        project.HasAchievedDiscovery = true;
                        mage.Log.Add($"BREAKTHROUGH! The secrets of '{project.Breakthrough.Name}' have been discovered!");
                        _researchService.ApplyBreakthroughEffects(project.Breakthrough, mage);
                        EmittedEvent = WorldEvent.LabOutcome(
                            (int)mage.SeasonalAge, WorldEventCategory.BreakthroughMade, mage, phase.BreakthroughPointsGained, project.Breakthrough.Name, true);
                    }
                    // Next phase spell selection happens at planning time next season.
                }
            }
        }

        public override bool Matches(IActivity action)
        {
            return action is OriginalResearchActivity research && research.ProjectId == this.ProjectId;
        }

        public override string Log()
        {
            return "Conducting Original Research worth " + Desire.ToString("0.000");
        }
    }
}
