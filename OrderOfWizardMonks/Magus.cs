using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
		public Houses House { get; set; }

        public Arts Arts { get; set; }

        private Ability _magicAbility;

        protected override void GeneratePreferences()
        {
            _preferences = PreferenceFactory.CreatePreferenceList(true);
        }

        public override void GenerateNewGoals()
        {
            foreach (Ability art in MagicArts.GetEnumerator())
            {
                double apprenticeAge = GetDesire(PreferenceType.AgeToApprentice, null);
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
            double desire = GetDesire(PreferenceType.Ability, art);
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
            double desire = GetDesire(PreferenceType.Writing, art);
            uint timeFrame = (uint)(20 / desire);
            int tractLimit = GetAbility(art).GetTractatiiLimit();
            if (tractLimit > _booksWritten.Where(b => b.Topic == art && b.Level == 0).Count())
            {
                _goals.Add(new WritingGoal(art, 0, 0, timeFrame, desire));
            }
        }

        public override EvaluatedBook EstimateBestBook()
        {
            EvaluatedBook bestBook = new EvaluatedBook
            {
                Book = null,
                PerceivedValue = 0
            };
            foreach (Ability art in MagicArts.GetEnumerator())
            {
                CharacterAbilityBase ability = GetAbility(art);
                if (ability.GetTractatiiLimit() > _booksWritten.Where(b => b.Topic == art && b.Level == 0).Count())
                {
                    //TODO: add in value of exposure?
                    // calculate tractatus value
                    Tractatus t = new Tractatus
                    {
                        Quality = Communication.Value + 3,
                        Topic = art
                    };
                    EvaluatedBook tract = new EvaluatedBook
                    {
                        Book = t,
                        PerceivedValue = RateLifetimeBookValue(t)
                    };
                    if (tract.PerceivedValue > bestBook.PerceivedValue)
                    {
                        bestBook = tract;
                    }
                }

                // calculate summa value
                if ((MagicArts.IsArt(art) && ability.GetValue() > 5) || ability.GetValue() > 2)
                {
                    // start with no q/l switching
                    Summa s = new Summa
                    {
                        Quality = Communication.Value + 6,
                        Level = ability.GetValue() / 2.0,
                        Topic = art
                    };
                    EvaluatedBook summa = new EvaluatedBook
                    {
                        Book = s,
                        PerceivedValue = RateLifetimeBookValue(s)
                    };
                }
            }
        }

        protected void CheckTwilight()
        {
        }

        public override double GetLabTotal(Ability technique, Ability form)
        {
            double magicTheory = GetAbility(_magicAbility).GetValue();
            double techValue = Arts.GetAbility(technique).GetValue();
            double formValue = Arts.GetAbility(form).GetValue();

            return magicTheory + techValue + formValue;
            //TODO: laboratory
            //TODO: foci
            //TODO: lab assistant
            //TODO: familiar
        }
	}
}
