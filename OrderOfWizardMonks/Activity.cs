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
        WriteLabText,
        CopyLabText,
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

    public abstract class ExposingMageAction : MageAction
    {
        private Ability _exposure;

        protected ExposingMageAction(Ability exposure, double desire)
        {
            _exposure = exposure;
            Desire = desire;
        }

        public override sealed void Act(Character character)
        {
            DoMageAction(ConfirmCharacterIsMage(character));
 	        character.GetAbility(_exposure).AddExperience(2);
        }

        protected abstract void DoMageAction(Magus mage);
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
    public class VisExtracting : ExposingMageAction
    {
        public VisExtracting(Ability exposure, double desire) : base(exposure, desire)
        {
            Action = Activity.DistillVis;
        }

        protected override void DoMageAction(Magus mage)
        {
            if (mage.Covenant == null)
            {
                throw new ArgumentNullException("Magi can only extract vis in an aura!");
            }
            mage.Covenant.AddVis(MagicArts.Vim, mage.GetLabTotal(MagicArtPairs.CrVi, Activity.DistillVis) / 10);
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

    public class BuildLaboratory : ExposingMageAction
    {
        public BuildLaboratory(Ability exposure, double desire) : base(exposure, desire)
        {
            Action = Activity.BuildLaboratory;
        }

        protected override void DoMageAction(Magus mage)
        {
            mage.BuildLaboratory();
        }
    }

    public class RefineLaboratory : ExposingMageAction
    {
        public RefineLaboratory(Ability exposure, double desire) : base(exposure, desire)
        {
            Action = Activity.BuildLaboratory;
        }

        protected override void DoMageAction(Magus mage)
        {
            // TODO: implement
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
            areaLore += mage.GetCastingTotal(MagicArtPairs.InVi) / 5;
            double roll = Die.Instance.RollDouble() * 5;
            double auraFound = Math.Sqrt(roll * areaLore);
            if (auraFound > 1)
            {
                mage.FoundCovenant(auraFound);
            }
            mage.GetAbility(_exposureAbility).AddExperience(2);
        }
    }

    public class FindApprentice : IAction
    {
        private Ability _exposureAbility;

        public FindApprentice(Ability exposureAbility, double desire)
        {
            _exposureAbility = exposureAbility;
            Desire = desire;
        }

        public ushort? SeasonId { get; private set; }

        public Activity Action
        {
            get { return Activity.FindApprentice; }
        }

        public double Desire { get; private set; }

        public void Act(Character character)
        {
            // see if the character can safely spont gift-finding spells
            if (typeof(Magus) == character.GetType())
            {
                MageApprenticeSearch((Magus)character);
            }
            else
            {
                CharacterApprenticeSearch(character);
            }
        }

        private void CharacterApprenticeSearch(Character character)
        {
            // TODO: eventually characters will be able to use magical items to do the search
            // making them work similar to the mage
        }

        private void MageApprenticeSearch(Magus mage)
        {
            // add bonus to area lore equal to casting total div 5?
            double areaLore = mage.GetAbility(Abilities.AreaLore).GetValue();
            areaLore += mage.GetCastingTotal(MagicArtPairs.InVi) / 5;
            double roll = Die.Instance.RollDouble();
            if (roll > 1)
            {
                mage.TakeApprentice(CharacterFactory.GenerateNewMagus());
            }
            mage.GetAbility(_exposureAbility).AddExperience(2);
            // TODO: gradual reduction in chance?
        }
    }

    public class CopyLabText : ExposingMageAction
    {
        public CopyLabText(Ability exposure, double desire) : base(exposure, desire)
        {
            Action = Activity.CopyLabText;
        }

        protected override void DoMageAction(Magus mage)
        {
            //TODO: implement
        }
    }

    public class WriteLabText : ExposingMageAction
    {
        public WriteLabText(Ability exposure, double desire) : base(exposure, desire)
        {
            Action = Activity.WriteLabText;
        }

        protected override void DoMageAction(Magus mage)
        {
            //TODO: implement
        }
    }
}