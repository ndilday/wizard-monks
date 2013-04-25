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
        void Act(Character character);
        double Desire { get; }
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

        public void Act(Character character)
        {
            character.ReadBook(_book);
        }

        public double Desire { get; private set; }
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

        public virtual void Act(Character character)
        {
            character.GetAbility(_ability).AddExperience(4);
        }

        public double Desire { get; private set; }
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

        public Writing(Ability topic, double level, double desire)
        {
            _topic = topic;
            _level = level;
            Desire = desire;
        }

        public ushort? SeasonId { get; private set; }

        public Activity Action
        {
            get { return Activity.WriteBook; }
        }

        public void Act(Character character)
        {

        }

        public double Desire { get; private set; }
    }
}
