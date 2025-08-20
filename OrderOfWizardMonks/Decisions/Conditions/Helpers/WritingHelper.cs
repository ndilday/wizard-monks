using System.Collections.Generic;
using System.Linq; // Add this
using WizardMonks.Activities.ExposingActivities;
using WizardMonks.Instances;
using WizardMonks.Models.Books;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Projects; // Add this
using WizardMonks.Services.Characters;

namespace WizardMonks.Decisions.Conditions.Helpers
{
    /// <summary>
    /// This helper evaluates writing a book to gain vis or reputation. 
    /// It identifies the most valuable book to write based on market demand and personal prestige,
    /// and then creates the necessary Project and/or Activity to begin or continue the work.
    /// </summary>
    class WritingHelper : AHelper
    {
        public WritingHelper(Magus mage, uint ageToCompleteBy, ushort conditionDepth, CalculateDesireFunc desireFunc = null) :
            base(mage, ageToCompleteBy, conditionDepth, desireFunc)
        {
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, Desires desires, IList<string> log)
        {
            // The GetBestBookToWrite method encapsulates the complex decision of WHAT to write.
            var bestBook = _mage.GetBestBookToWrite();
            if (bestBook == null)
            {
                return; // No profitable book to write this season.
            }

            double effectiveDesire = _desireFunc(bestBook.Value, _conditionDepth);

            if (bestBook is Summa summa)
            {
                // Multi-season work requires a project.
                // First, check if we're already working on this book.
                var existingProject = _mage.ActiveProjects
                    .OfType<SummaWritingProject>()
                    .FirstOrDefault(p => p.Summa.Topic == summa.Topic);

                if (existingProject == null)
                {
                    // If not, create a new project for it.
                    existingProject = new SummaWritingProject(_mage, summa);
                    _mage.ActiveProjects.Add(existingProject);
                    log.Add($"[Project Created] Began a project to write the summa '{summa.Title}'.");
                }

                log.Add($"Considering continuing to write '{summa.Title}' worth {effectiveDesire:0.000}");
                var writingActivity = new WriteActivity(summa.Topic, summa.Title, existingProject.ProjectId, Abilities.Latin, summa.Level, effectiveDesire);
                alreadyConsidered.Add(writingActivity);
            }
            else if (bestBook is Tractatus tractatus)
            {
                // Single-season work; no project is needed.
                log.Add($"Considering writing the tractatus '{tractatus.Title}' worth {effectiveDesire:0.000}");
                var writingActivity = new WriteActivity(tractatus.Topic, tractatus.Title, null, Abilities.Latin, 1000, effectiveDesire);
                alreadyConsidered.Add(writingActivity);
            }
        }
    }
}