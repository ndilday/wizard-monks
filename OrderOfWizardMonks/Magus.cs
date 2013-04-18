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

        public Magus(Ability magicAbility, Ability writingLanguage, Ability writingAbility, Dictionary<Preference, double> preferences)
            : base(writingLanguage, writingAbility, preferences)
        {
            _magicAbility = magicAbility;
        }

		public Houses House { get; set; }

        public Arts Arts { get; set; }

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

        public override EvaluatedBook EstimateBestBookToWrite()
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
                // TODO: how to decide what audience the magus is writing for?
                // when art > 10, magus will write a /2 book
                // when art >=20, magus will write a /4 book
                if ((MagicArts.IsArt(art) && ability.GetValue() >= 10) || ability.GetValue() >= 4)
                {
                    // start with no q/l switching
                    CharacterAbilityBase theoreticalPurchaser;
                    if (MagicArts.IsArt(art))
                    {
                        theoreticalPurchaser = new AcceleratedAbility(art);
                    }
                    else
                    {
                        theoreticalPurchaser = new CharacterAbility(art);
                    }
                    theoreticalPurchaser.AddExperience(ability.Experience / 2);
                    Summa s = new Summa
                    {
                        Quality = Communication.Value + 6,
                        Level = ability.GetValue() / 2.0,
                        Topic = art
                    };
                    double value = RateLifetimeBookValue(s, theoreticalPurchaser);
                    if (value > bestBook.PerceivedValue)
                    {
                        bestBook = new EvaluatedBook
                        {
                            Book = s,
                            PerceivedValue = value
                        };
                    }
                }
                if ((MagicArts.IsArt(art) && ability.GetValue() >= 20) || ability.GetValue() >= 6)
                {
                    // start with no q/l switching
                    CharacterAbilityBase theoreticalPurchaser;
                    if (MagicArts.IsArt(art))
                    {
                        theoreticalPurchaser = new AcceleratedAbility(art);
                    }
                    else
                    {
                        theoreticalPurchaser = new CharacterAbility(art);
                    }
                    theoreticalPurchaser.AddExperience(ability.Experience / 4);

                    double qualityAdd = ability.GetValue() / 4;
                    if (qualityAdd > (Communication.Value + 6))
                    {
                        qualityAdd = Communication.Value + 6;
                    }

                    Summa s = new Summa
                    {
                        Quality = Communication.Value + 6 + qualityAdd,
                        Level = (ability.GetValue() / 2.0) - qualityAdd,
                        Topic = art
                    };
                    double seasonsNeeded = s.GetWritingPointsNeeded() / (Communication.Value + GetAbility(_writingAbility).GetValue());
                    double value = RateLifetimeBookValue(s, theoreticalPurchaser) / seasonsNeeded;
                    if (value > bestBook.PerceivedValue)
                    {
                        bestBook = new EvaluatedBook
                        {
                            Book = s,
                            PerceivedValue = value
                        };
                    }
                }
            }
            return bestBook;
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
