using System;

namespace WizardMonks.Activities
{
    [Serializable]
    public class ReadActivity(ABook book, double desire) : IActivity
    {
        public ABook Book { get; private set; } = book;

        public Activity Action
        {
            get
            {
                return Activity.ReadBook;
            }
        }

        public double Desire { get; set; } = desire;

        public void Act(Character character)
        {
            character.ReadBook(Book);
        }

        public bool Matches(IActivity action)
        {
            if (action.Action != Activity.ReadBook)
            {
                return false;
            }
            ReadActivity reading = (ReadActivity)action;
            return reading.Book == Book;
        }

        public string Log()
        {
            return "Reading " + Book.Title + " worth " + Desire.ToString("0.000");
        }
    }

}
