using System;
using System.Linq;
using WizardMonks.Instances;
using WizardMonks.Models.Books;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Projects;

namespace WizardMonks.Activities.ExposingActivities
{
    [Serializable]
    public class CopyBookActivity : AExposingActivity
    {
        // TODO: handle multiple books to copy
        public bool CopyQuickly { get; private set; }
        public Guid? ProjectId { get; private set; }
        public ABook Book { get; private set; }

        public CopyBookActivity(bool copyQuickly, ABook bookToCopy, Guid? projectId, Ability exposure, double desire)
            : base(exposure, desire)
        {
            CopyQuickly = copyQuickly;
            Book = bookToCopy;
            ProjectId = projectId;
            Action = Activity.CopyBook;
        }

        protected override void DoAction(Character character)
        {
            if (Book is Tractatus)
            {
                // This logic is simple: a tractatus is copied in a single season.
                Tractatus tract = new()
                {
                    Author = Book.Author,
                    Quality = Book.Quality,
                    Title = "Copy of " + Book.Title,
                    Topic = Book.Topic
                };
                character.AddBookToCollection(tract);
                character.Log.Add($"Finished copying the tractatus '{Book.Title}'.");
            }
            else if (ProjectId.HasValue)
            {
                var project = character.ActiveProjects.OfType<SummaCopyingProject>().FirstOrDefault(p => p.ProjectId == ProjectId.Value);
                if (project == null)
                {
                    character.Log.Add("Attempted to work on a non-existent book copying project.");
                    return;
                }

                double progressThisSeason = 6 + character.GetAbility(Abilities.Scribing).Value;
                project.AddProgress(progressThisSeason);

                if (project.IsComplete)
                {
                    project.Summa.Title = "Copy of " + project.Summa.Title;
                    character.AddBookToCollection(project.Summa);
                    character.ActiveProjects.Remove(project);
                    character.Log.Add($"Completed copying the summa '{Book.Title}'.");
                }
                else
                {
                    character.Log.Add($"Continued copying '{Book.Title}'. Progress: {project.Progress:F0}/{project.PointsNeeded:F0}.");
                }
            }
        }

        public override bool Matches(IActivity action)
        {
            if (action is not CopyBookActivity copy) return false;
            // Match on project if it exists, otherwise on the book itself (for Tractatus)
            return (copy.ProjectId.HasValue && copy.ProjectId == this.ProjectId) || (copy.Book == this.Book);
        }

        public override string Log()
        {
            throw new NotImplementedException();
        }
    }

}
