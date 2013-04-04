using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks
{
    public abstract class GoalBase
    {
        protected bool _isCached = false;
        protected double _score = 0;

        public uint SeasonsToComplete { get; set; }
        public double Priority { get; set; }
        public abstract double Score(Character character);
        public abstract IAction GetSeasonalActivity(Character character);
        public void Flush()
        {
            _isCached = false;
        }
    }

    public class AbilityGoal : GoalBase
    {
        public Ability Ability { get; set; }
        public double Level { get; set; }

        private IBook _bookToRead;

        public override double Score(Character character)
        {
            if (_isCached) return _score;
            CharacterAbilityBase ability = character.GetAbility(this.Ability);
            double distance = ability.GetExperienceUntilLevel(this.Level);

            // the farther someone is from their goal, the more important it is  
            _score = distance * this.Priority / this.SeasonsToComplete;
            _isCached = true;
            return _score;
        }

        public override IAction GetSeasonalActivity(Character character)
        {
            IBook bookToRead = character.GetBestBookFromCollection(this.Ability);
            if (bookToRead == null)
            {
                return new Practice(this.Ability);
            }
            else
            {
                return new Reading(bookToRead);
            }
        }
    }

    public class WritingGoal : AbilityGoal
    {
        public double Quality { get; set; }

        public override double Score(Character character)
        {
            if (_isCached) return _score;
            CharacterAbilityBase ability = character.GetAbility(this.Ability);
            double charLevel = ability.GetValue();
            if (this.Level != 0 && (this.Level * 2 > charLevel || this.Level * 2 + this.Quality > charLevel + character.Communication.Value + 9))
            {
                _score = 0;
                _isCached = true;
                return 0;
            }

            // TODO: we may need some other way to weight the writing goal to keep it in line with ability-seeking
            _isCached = true;
            _score = this.Priority / this.SeasonsToComplete;
            return _score;
        }
    }

    public class VisGoal : AbilityGoal
    {
        public override double Score(Character character)
        {
            if (_isCached) return _score;
            double multiplier = GetMultiplier();
            if (multiplier == 0)
            {
                _isCached = true;
                _score = 0;
                return 0;
            }

            double labTotal = character.GetLabTotal(MagicArts.Creo, MagicArts.Vim);
            _isCached = true;
            _score = labTotal * multiplier * this.Priority * this.Level / (10 * this.SeasonsToComplete);
            return _score;
        }

        private double GetMultiplier()
        {
            if (this.Ability.AbilityId == MagicArts.Vim.AbilityId)
            {
                return 1;
            }
            else if (MagicArts.IsForm(this.Ability))
            {
                return 2;
            }
            else if (MagicArts.IsTechnique(this.Ability))
            {
                return 4;
            }
            return 0;
        }
    }
}
