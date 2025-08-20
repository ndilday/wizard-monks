using System;
using System.Linq;
using WizardMonks.Instances;
using WizardMonks.Models.Books;
using WizardMonks.Models.Characters;

namespace WizardMonks.Activities.ExposingActivities
{
    [Serializable]
    public class CopyBookActivity : AExposingActivity
    {
        // TODO: handle multiple books to copy
        public bool CopyQuickly { get; private set; }
        public ABook Book { get; private set; }

        public CopyBookActivity(bool copyQuickly, ABook bookToCopy, Ability exposure, double desire)
            : base(exposure, desire)
        {
            CopyQuickly = copyQuickly;
            Book = bookToCopy;
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
            else if (Book is Summa summaToCopy)
            {
                // Find if a copy is already in progress. We identify it by the original's title and author.
                Summa existingCopy = character.IncompleteCopies.FirstOrDefault(c => c.Title == summaToCopy.Title && c.Author == summaToCopy.Author);

                if (existingCopy == null)
                {
                    // Start a new copy project
                    existingCopy = new Summa
                    {
                        Author = summaToCopy.Author,
                        Quality = summaToCopy.Quality,
                        Title = summaToCopy.Title, // We use the original title to track the project
                        Topic = summaToCopy.Topic,
                        Level = summaToCopy.Level,
                        PointsComplete = 0
                    };
                    character.IncompleteCopies.Add(existingCopy);
                    character.Log.Add($"Started copying the summa '{summaToCopy.Title}'.");
                }

                // Calculate and add this season's progress
                double progressThisSeason = 6 + character.GetAbility(Abilities.Scribing).Value;
                existingCopy.PointsComplete += progressThisSeason;

                // Check for completion
                if (existingCopy.PointsComplete >= existingCopy.GetWritingPointsNeeded())
                {
                    // The copy is finished.
                    existingCopy.Title = "Copy of " + existingCopy.Title; // Finalize the title
                    character.AddBookToCollection(existingCopy);
                    character.IncompleteCopies.Remove(existingCopy); // Remove from the in-progress list
                    character.Log.Add($"Completed copying the summa '{summaToCopy.Title}'. Gained a book of L{existingCopy.Level}/Q{existingCopy.Quality}.");
                }
                else
                {
                    // Not finished yet, log the progress.
                    character.Log.Add($"Continued copying '{summaToCopy.Title}'. Progress: {existingCopy.PointsComplete:F0}/{existingCopy.GetWritingPointsNeeded():F0}.");
                }
            }
        }

        public override bool Matches(IActivity action)
        {
            if (action.Action != Activity.CopyBook)
            {
                return false;
            }
            CopyBookActivity copy = (CopyBookActivity)action;
            return copy.Book == Book && copy.CopyQuickly == CopyQuickly;
        }

        public override string Log()
        {
            throw new NotImplementedException();
        }
    }

}
