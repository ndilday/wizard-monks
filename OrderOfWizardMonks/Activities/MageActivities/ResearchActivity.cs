using System;
using System.Linq;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Projects;
using WizardMonks.Services.Characters;

namespace WizardMonks.Activities.MageActivities
{
    [Serializable]
    public class OriginalResearchActivity : AExposingMageActivity
    {
        public Guid ProjectId { get; private set; }

        public OriginalResearchActivity(Guid projectId, Ability exposure, double desire)
            : base(exposure, desire)
        {
            ProjectId = projectId;
            Action = Activity.OriginalResearch;
        }

        protected override void DoMageAction(Magus mage)
        {
            var project = mage.ActiveProjects.OfType<ResearchProject>().FirstOrDefault(p => p.ProjectId == ProjectId);
            if (project == null)
            {
                mage.Log.Add("Attempted to work on a non-existent research project.");
                return;
            }
            if (project.CurrentPhase == null)
            {
                // This would be the point to create the first phase if needed.
                mage.Log.Add($"Research project '{project.Description}' has no active phase to work on.");
                // project.StartNewPhase(); // Example of what a helper might do.
                return;
            }

            var phase = project.CurrentPhase;

            if (!phase.IsInvented)
            {
                // Work on inventing the experimental spell
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
                    mage.Log.Add($"Experimental spell '{phase.ExperimentalSpell.Name}' has been successfully invented!");
                }
            }
            else if (!phase.IsStabilized)
            {
                // Work on stabilizing the spell
                phase.WorkOnStabilization();
                mage.Log.Add($"Worked on stabilizing '{phase.ExperimentalSpell.Name}'. Seasons remaining: {phase.SeasonsToStabilize}");
                if (phase.IsStabilized)
                {
                    mage.Log.Add($"Research phase complete! Gained {phase.BreakthroughPointsGained} breakthrough points.");
                    project.CompletedPhases.Add(phase);

                    if (project.BreakthroughPointsAccumulated >= project.BreakthroughPointsRequired)
                    {
                        project.HasAchievedDiscovery = true;
                        mage.Log.Add($"BREAKTHROUGH! The secrets of '{project.Breakthrough.Name}' have been discovered!");
                        // The effect is applied when the project is removed/formalized, not immediately.
                    }
                    else
                    {
                        // Start the next phase
                        project.StartNewPhase();
                        mage.Log.Add("Beginning the next phase of research.");
                    }
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