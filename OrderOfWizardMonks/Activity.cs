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
        FindAura,
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

    public abstract class MageAction : IAction
    {
        public ushort? SeasonId { get; protected set; }

        public Activity Action { get; protected set; }

        public double Desire { get; protected set; }

        public abstract void Act(Character character);

        protected Magus ConfirmCharacterIsMage(Character character)
        {
            if (typeof(Magus) != character.GetType())
            {
                throw new InvalidCastException("Only magi can extract vis!");
            }
            return (Magus)character;
        }
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
    public class VisExtracting : MageAction
    {
        private Ability _exposure;

        public VisExtracting(Ability exposure, double desire)
        {
            _exposure = exposure;
            Desire = desire;
            Action = Activity.DistillVis;
        }

        public override void Act(Character character)
        {
            Magus mage = ConfirmCharacterIsMage(character);
            if (mage.Covenant == null)
            {
                throw new ArgumentNullException("Magi can only extract vis in an aura!");
            }
            mage.Covenant.AddVis(MagicArts.Vim, mage.GetLabTotal(MagicArts.Creo, MagicArts.Vim, Activity.DistillVis) / 10);
            mage.GetAbility(_exposure).AddExperience(2);
        }
    }

    [Serializable]
    public class VisStudying : MageAction
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
            Action = Activity.StudyVis;
        }

        public override void Act(Character character)
        {
            Magus mage = ConfirmCharacterIsMage(character);

            // determine the amount of vis needed
            CharacterAbilityBase charAbility = mage.GetAbility(_art);
            double visNeeded = 0.5 + (charAbility.GetValue() / 10.0);
            
            // decrement the used vis
            mage.UseVis(_art, visNeeded);

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

    public class BuildLaboratory : MageAction
    {
        private Ability _exposure;

        public BuildLaboratory(Ability exposure, double desire)
        {
            Desire = desire;
            Action = Activity.BuildLaboratory;
            _exposure = exposure;
        }

        public override void Act(Character character)
        {
            Magus mage = ConfirmCharacterIsMage(character);
            mage.BuildLaboratory();
            mage.GetAbility(_exposure).AddExperience(2);
        }
    }

    public class RefineLaboratory : MageAction
    {
        private Ability _exposure;

        public RefineLaboratory(Ability exposure, double desire)
        {
            Desire = desire;
            Action = Activity.BuildLaboratory;
            _exposure = exposure;
        }

        public override void Act(Character character)
        {
            Magus mage = ConfirmCharacterIsMage(character);
            // TODO: implement
            // grant exposure experience
            mage.GetAbility(_exposure).AddExperience(2);
        }
    }

    public class FindAura : IAction
    {
        private Ability _exposureAbility;

        public FindAura(Ability exposureAbility, double desire)
        {
            _exposureAbility = exposureAbility;
            Desire = desire;
        }

        public ushort? SeasonId { get; private set; }

        public Activity Action
        {
            get { return Activity.FindAura; }
        }

        public double Desire { get; private set; }

        public void Act(Character character)
        {
            // see if the character can safely spont aura-finding spells
            if (typeof(Magus) == character.GetType())
            {
                MageAuraSearch((Magus)character);
            }
            else
            {
                CharacterAuraSearch(character);
            }
            // TODO: store knowledge of locations
            // TODO: as we go, eventually, do we want locations to be set, rather than generated upon finding?
        }

        private void CharacterAuraSearch(Character character)
        {
            // TODO: eventually characters will be able to use magical items to do the search
            // making them work similar to the mage
        }

        private void MageAuraSearch(Magus mage)
        {
            // add bonus to area lore equal to casting total div 5?
            double areaLore = mage.GetAbility(Abilities.AreaLore).GetValue();
            areaLore += mage.GetCastingTotal(MagicArts.Intellego, MagicArts.Vim) / 5;
            double roll = Die.Instance.RollDouble() * 5;
            double auraFound = Math.Sqrt(roll * areaLore);
            if (auraFound > 1)
            {
                mage.FoundCovenant(auraFound);
            }
            mage.GetAbility(_exposureAbility).AddExperience(2);
        }
    }
}