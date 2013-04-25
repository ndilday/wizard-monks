using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WizardMonks.Instances;

namespace WizardMonks
{
    public class TwilightEventArgs : EventArgs
    {
        DateTime _duration;
        ushort _extraWarping;

        public TwilightEventArgs(DateTime duration, ushort extraWarping)
        {
            _duration = duration;
            _extraWarping = extraWarping;
        }
    }

	[Serializable]
	public class Magus : Character
	{
        private Ability _magicAbility;
        private Covenant _covenant;

        public Magus(Ability magicAbility, Ability writingLanguage, Ability writingAbility, Dictionary<Preference, double> preferences)
            : base(writingLanguage, writingAbility, preferences)
        {
            _magicAbility = magicAbility;
            Arts = new Arts();
            _covenant = null;
        }

		public Houses House { get; set; }

        public Arts Arts { get; private set; }

        public override CharacterAbilityBase GetAbility(Ability ability)
        {
            if (MagicArts.IsArt(ability))
            {
                return Arts.GetAbility(ability);
            }
            else
            {
                return base.GetAbility(ability);
            }
        }

        #region Covenant Functions
        public void Join(Covenant covenant)
        {
            _covenant = covenant;
            covenant.AddMagus(this);
        }

        public Covenant FoundCovenant(int auraLevel = 0)
        {
            Covenant coventant = new Covenant();
            Join(coventant);
            coventant.Aura = auraLevel;
            return coventant;
        }
        #endregion

        #region Book Functions
        public override IEnumerable<IBook> GetBooksFromCollection(Ability ability)
        {
            IEnumerable<IBook> books = _booksOwned.Where(b => b.Topic == ability);
            if (_covenant != null)
            {
                books = books.Union(_covenant.GetLibrary(ability));
            }
            return books;
        }

        public override EvaluatedBook EstimateBestBookToWrite()
        {
            EvaluatedBook bestBook = base.EstimateBestBookToWrite();
            foreach (Ability art in MagicArts.GetEnumerator())
            {
                bestBook = CompareToBestBook(bestBook, Arts.GetAbility(art));
            }
            return bestBook;
        }
        #endregion

        #region Goal/Preference Functions
        public override void GenerateNewGoals()
        {
            foreach (Ability art in MagicArts.GetEnumerator())
            {
                double apprenticeAge = GetDesire(new Preference(PreferenceType.AgeToApprentice, null));
                GenerateArtWritingGoal(art);
                int seasonsLived = 20 + _seasonList.Count();
                if (apprenticeAge < seasonsLived)
                {
                    GenerateArtLearningGoal(art, seasonsLived, 5);
                }
                else
                {
                    GenerateArtLearningGoal(art, seasonsLived, 50);
                }
            }
        }

        private void GenerateArtLearningGoal(Ability art, int seasonsLived, double level)
        {
            double desire = GetDesire(new Preference(PreferenceType.Ability, art));
            _goals.Add(new AbilityGoal
                {
                    Ability = art,
                    Level = level,
                    SeasonsToComplete = (uint)(400 - seasonsLived),
                    Priority = desire
                });
        }

        private void GenerateArtWritingGoal(Ability art)
        {
            double desire = GetDesire(new Preference(PreferenceType.Writing, art));
            uint timeFrame = (uint)(20 / desire);
            int tractLimit = GetAbility(art).GetTractatiiLimit();
            if (tractLimit > _booksWritten.Where(b => b.Topic == art && b.Level == 0).Count())
            {
                _goals.Add(new WritingGoal(art, 0, 0, timeFrame, desire));
            }
        }
        #endregion

        protected void CheckTwilight()
        {
        }

        public override IAction DecideSeasonalActivity()
        {
            // TODO: make sure all preference values are scaled the same
            IAction start = base.DecideSeasonalActivity();

            if (start.Desire < 1 && _covenant != null && _covenant.Aura > 0)
            {
                double visPerSeason = GetLabTotal(MagicArts.Creo, MagicArts.Vim) / 10;

            }

            // TODO: study vis

            return start;
        }

        public override double GetLabTotal(Ability technique, Ability form)
        {
            double magicTheory = GetAbility(_magicAbility).GetValue();
            double techValue = Arts.GetAbility(technique).GetValue();
            double formValue = Arts.GetAbility(form).GetValue();
            double labTotal =  magicTheory + techValue + formValue;
            if (_covenant != null)
            {
                labTotal += _covenant.Aura;
            }

            //TODO: laboratory
            //TODO: foci
            //TODO: lab assistant
            //TODO: familiar
            return labTotal;
        }
	}
}
