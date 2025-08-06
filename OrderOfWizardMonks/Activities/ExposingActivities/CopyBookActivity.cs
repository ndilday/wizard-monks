using System;
using WizardMonks.Instances;
using WizardMonks.Models;

namespace WizardMonks.Activities.ExposingActivities
{
    [Serializable]
    public class CopyBook : AExposingActivity
    {
        // TODO: handle multiple books to copy
        public bool CopyQuickly { get; private set; }
        public ABook Book { get; private set; }

        public CopyBook(bool copyQuickly, ABook bookToCopy, Ability exposure, double desire)
            : base(exposure, desire)
        {
            CopyQuickly = copyQuickly;
            Book = bookToCopy;
            Action = Activity.CopyBook;
        }

        protected override void DoAction(Character character)
        {
            double scribeAbilityValue = character.GetAbility(Abilities.Scribing).Value;
            if (Book.Level == 1000)
            {
                Tractatus tract = new()
                {
                    Author = Book.Author,
                    Quality = Book.Quality,
                    Title = Book.Title,
                    Topic = Book.Topic
                };
                character.AddBookToCollection(tract);
            }
            else
            {
                // TODO: implement logic for copying summae
            }
        }

        public override bool Matches(IActivity action)
        {
            if (action.Action != Activity.CopyBook)
            {
                return false;
            }
            CopyBook copy = (CopyBook)action;
            return copy.Book == Book && copy.CopyQuickly == CopyQuickly;
        }

        public override string Log()
        {
            throw new NotImplementedException();
        }
    }

}
