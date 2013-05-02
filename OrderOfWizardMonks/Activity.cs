using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WizardMonks.Instances;

namespace WizardMonks
{
	[Serializable]
	public enum Activity
	{
		ReadAbilityBook,
		ReadArtBook,
		InventSpells,
		InventItem,
		LongevityRitual,
		EnchantFamiliar,
		OriginalResearch,
		InitiateOther,
		InitiateSelf,
		Adventure,
		DistillVis,
		StudyVis,
        Teach,
        Learn,
		WriteBook,
		CopyBook,
		BuildLaboratory,
		RefineLaboratory,
		FindApprentice,
		FindVisSource,
		Sundry,
        Practice
	}

	public interface IAction
	{
		ushort? SeasonId { get; }
        Activity Action { get; }
        double Desire { get; }
        void Act(Character character);
	}

    [Serializable]
    public class Reading : IAction
    {
        private IBook _book;
        public Reading(IBook book, double desire)
        {
            _book = book;
            Desire = desire;
        }

        public ushort? SeasonId { get; private set; }
        
        public Activity Action
        {
            get
            {
                if (this._book != null && MagicArts.IsArt(this._book.Topic))
                {
                    return Activity.ReadArtBook;
                }
                return Activity.ReadAbilityBook;
            }
        }

        public double Desire { get; private set; }

        public void Act(Character character)
        {
            character.ReadBook(_book);
        }
    }

    [Serializable]
    public class Practice : IAction
    {
        protected Ability _ability;
        public Practice(Ability ability, double desire)
        {
            _ability = ability;
            Desire = desire;
        }

        public ushort? SeasonId { get; private set; }
        
        public virtual Activity Action
        {
            get { return Activity.Practice; }
        }

        public double Desire { get; private set; }

        public virtual void Act(Character character)
        {
            character.GetAbility(_ability).AddExperience(4);
        }
    }

    [Serializable]
    public class Exposure: Practice
    {
        public Exposure(Ability ability, double desire) : base(ability, desire) { }

        public override Activity Action
        {
            get { return Activity.Sundry; }
        }

        public override void Act(Character character)
        {
            character.GetAbility(_ability).AddExperience(2);
        }
    }

    [Serializable]
    public class Writing : IAction
    {
        private Ability _topic;
        private double _level;
        private Ability _exposure;

        public Writing(Ability topic, Ability exposure, double level, double desire)
        {
            _topic = topic;
            _exposure = exposure;
            _level = level;
            Desire = desire;
        }

        public ushort? SeasonId { get; private set; }

        public Activity Action
        {
            get { return Activity.WriteBook; }
        }

        public double Desire { get; private set; }

        public void Act(Character character)
        {
            character.WriteBook(_topic, _level);
            character.GetAbility(_exposure).AddExperience(2);
        }
    }

    [Serializable]
    public class VisExtracting : IAction
    {
        private Ability _exposure;

        public VisExtracting(Ability exposure, double desire)
        {
            _exposure = exposure;
            Desire = desire;
        }

        public ushort? SeasonId { get; private set; }

        public Activity Action
        {
            get { return Activity.DistillVis; }
        }

        public double Desire { get; private set; }

        public void Act(Character character)
        {
            if (typeof(Magus) != character.GetType())
            {
                throw new InvalidCastException("Only magi can extract vis!");
            }
            Magus mage = (Magus)character;
            if (mage.Covenant == null)
            {
                throw new ArgumentNullException("Magi can only extract vis in an aura!");
            }
            mage.Covenant.AddVis(MagicArts.Vim, mage.GetLabTotal(MagicArts.Creo, MagicArts.Vim) / 10);
            mage.GetAbility(_exposure).AddExperience(2);
        }
    }

    [Serializable]
    public class VisStudying : IAction
    {
        private Ability _art;

        public VisStudying(Ability art, double desire)
        {
            if (!MagicArts.IsArt(art))
            {
                throw new ArgumentException("Only magic arts have vis associated with them!");
            }
            _art = art;
            Desire = desire;
        }

        public ushort? SeasonId { get; private set; }

        public Activity Action
        {
            get { return Activity.StudyVis; }
        }

        public double Desire { get; private set; }

        public void Act(Character character)
        {
            if (typeof(Magus) != character.GetType())
            {
                throw new InvalidCastException("Only magi can extract vis!");
            }
            Magus mage = (Magus)character;

            // determine the amount of vis needed
            CharacterAbilityBase charAbility = mage.GetAbility(_art);
            double visNeeded = 0.5 + (charAbility.GetValue() / 10.0);
            
            // decrement the used vis
            mage.RemoveVis(_art, visNeeded);

            // add experience
            charAbility.AddExperience(Die.Instance.RollExplodingDie());
        }
    }

    [Serializable]
    public class CopyBook : IAction
    {
        public ushort? SeasonId { get; private set; }

        public Activity Action
        {
            get { return Activity.CopyBook; }
        }

        public double Desire { get; private set; }

        public void Act(Character character)
        {
            // TODO: implement
        }
    }

    [Serializable]
    public class Teach : IAction
    {
        public ushort? SeasonId { get; private set; }

        public Activity Action
        {
            get { return Activity.Teach; }
        }

        public double Desire { get; private set; }

        public void Act(Character character)
        {
            // TODO: implement
        }
    }

    [Serializable]
    public class Learn : IAction
    {
        public ushort? SeasonId { get; private set; }

        public Activity Action
        {
            get { return Activity.Learn; }
        }

        public double Desire { get; private set; }

        public void Act(Character character)
        {
            // TODO: implement
        }
    }

    public class BuildLaboratory : IAction
    {
        public ushort? SeasonId { get; private set; }

        public Activity Action
        {
            get { return Activity.BuildLaboratory; }
        }

        public double Desire { get; private set; }

        public void Act(Character character)
        {
            // TODO: implement
        }
    }

    public class RefineLaboratory : IAction
    {
        public ushort? SeasonId { get; private set; }

        public Activity Action
        {
            get { return Activity.RefineLaboratory; }
        }

        public double Desire { get; private set; }

        public void Act(Character character)
        {
            // TODO: implement
        }
    }
}
