using System;
using WizardMonks.Instances;
using WizardMonks.Models.Characters;

namespace WizardMonks.Models.Books
{
    [Serializable]
    public abstract class ABook : AWritable
    {
        public string Title { get; set; }

        public Ability Topic { get; set; }
        public string TopicName
        {
            get
            {
                return Topic.AbilityName;
            }
        }
        public double Quality { get; set; }
        public virtual double Level { get; set; }
        public double Value { get; set; }
    }

    [Serializable]
    public class Summa : ABook
    {
        public double GetWritingPointsNeeded()
        {
            return MagicArts.IsArt(Topic) ? Level : Level * 5;
        }
        public double PointsComplete { get; set; }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(Summa))
            {
                return false;
            }
            Summa otherBook = (Summa)obj;
            return
                Author == otherBook.Author &&
                Level == otherBook.Level &&
                Quality == otherBook.Quality &&
                Title == otherBook.Title &&
                Topic == otherBook.Topic;
        }

        public override int GetHashCode()
        {
            return Author.GetHashCode() ^ Level.GetHashCode() ^ Quality.GetHashCode() ^ Title.GetHashCode() ^ Topic.GetHashCode();
        }
    }

    [Serializable]
    public class Tractatus : ABook
    {
        public override double Level
        {
            get
            {
                return 1000;
            }
            set
            {
            }
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(Tractatus))
            {
                return false;
            }
            Tractatus otherBook = (Tractatus)obj;
            return
                Author == otherBook.Author &&
                Quality == otherBook.Quality &&
                Title == otherBook.Title &&
                Topic == otherBook.Topic;
        }

        public override int GetHashCode()
        {
            return Author.GetHashCode() ^ Quality.GetHashCode() ^ Title.GetHashCode() ^ Topic.GetHashCode();
        }
    }
}
