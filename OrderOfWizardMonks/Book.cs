
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WizardMonks.Instances;

namespace WizardMonks
{
	[Serializable]
	public class IBook
	{
        private string _authorName;
        private Character _author;
        public string Title { get; set; }
        public Character Author
        {
            get
            {
                return _author;
            }
            set
            {
                _author = value;
                if (_author != null)
                {
                    _authorName = value.Name;
                }
            }
        }
        public string AuthorName
        {
            get
            {
                return _authorName;
            }
            set
            {
                if(Author == null)
                {
                    _authorName = value;
                }
            }
        }
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
	public class Summa : IBook
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
                this.AuthorName == otherBook.AuthorName && 
                this.Level == otherBook.Level && 
                this.Quality == otherBook.Quality && 
                this.Title == otherBook.Title && 
                this.Topic == otherBook.Topic;
        }

        public override int GetHashCode()
        {
            return this.AuthorName.GetHashCode() ^ this.Level.GetHashCode() ^ this.Quality.GetHashCode() ^ this.Title.GetHashCode() ^ this.Topic.GetHashCode();
        }
    }

    [Serializable]
    public class Tractatus : IBook
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
                this.AuthorName == otherBook.AuthorName &&
                this.Quality == otherBook.Quality &&
                this.Title == otherBook.Title &&
                this.Topic == otherBook.Topic;
        }

        public override int GetHashCode()
        {
            return this.AuthorName.GetHashCode() ^ this.Quality.GetHashCode() ^ this.Title.GetHashCode() ^ this.Topic.GetHashCode();
        }
    }

    public class EvaluatedBook
    {
        public IBook Book { get; set; }
        public double PerceivedValue { get; set; }
        public Ability ExposureAbility { get; set; }
    }
}
