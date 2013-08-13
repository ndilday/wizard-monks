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
		ReadBook,
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

    #region IAction Classes
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
                return Activity.ReadBook;
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
    public class Exposure : Practice
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
    #endregion

    #region ExposingAction classes
    public abstract class ExposingAction : IAction
    {
        private Ability _exposure;

        public ushort? SeasonId { get; protected set; }

        public Activity Action { get; protected set; }

        public double Desire { get; protected set; }

        protected ExposingAction(Ability exposure, double desire)
        {
            _exposure = exposure;
            Desire = desire;
        }

        public void Act(Character character)
        {
            DoAction(character);
 	        character.GetAbility(_exposure).AddExperience(2);
        }

        protected abstract void DoAction(Character character);
    }

    [Serializable]
    public class Writing : ExposingAction
    {
        private Ability _topic;
        private double _level;

        public Writing(Ability topic, Ability exposure, double level, double desire)
            : base(exposure, desire)
        {
            _topic = topic;
            _level = level;
            Action = Activity.WriteBook;
        }

        protected override void DoAction(Character character)
        {
            character.WriteBook(_topic, _level);
        }
    }

    [Serializable]
    public class CopyBook : ExposingAction
    {
        bool _copyQuickly;

        public CopyBook(bool copyQuickly, Ability exposure, double desire)
            : base(exposure, desire)
        {
            _copyQuickly = copyQuickly;
            Action = Activity.CopyBook;
        }

        protected override void DoAction(Character character)
        {
            // TODO: implement
        }
    }

    public class FindAura : ExposingAction
    {
        public FindAura(Ability exposureAbility, double desire)
            : base(exposureAbility, desire)
        {
            Action = Activity.FindAura;
        }

        protected override void DoAction(Character character)
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
        }
    }

    public class FindApprentice : ExposingAction
    {
        public FindApprentice(Ability exposureAbility, double desire) : base(exposureAbility, desire)
        {
            Action = Activity.FindApprentice;
        }

        protected override void DoAction(Character character)
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
            // TODO: gradual reduction in chance?
        }
    }

    [Serializable]
    public class Teach : ExposingAction
    {
        private Character _student;
        private Ability _abilityToTeach;
        public Teach(Character student, Ability abilityToTeach, Ability exposure, double desire) : base(exposure, desire)
        {
            Action = Activity.Teach;
            _student = student;
            _abilityToTeach = abilityToTeach;
        }

        protected override void DoAction(Character character)
        {
            // TODO: implement
            if (_student.GetAbility(_abilityToTeach).GetValue() >= character.GetAbility(_abilityToTeach).GetValue())
            {
                throw new ArgumentOutOfRangeException("Teacher has nothing to teach this student!");
            }
        }
    }
    #endregion

    #region MageAction Classes
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
    #endregion

    #region ExposingMageAction Classes
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

    public class BuildLaboratory : ExposingMageAction
    {
        public BuildLaboratory(Ability exposure, double desire)
            : base(exposure, desire)
        {
            Action = Activity.BuildLaboratory;
        }

        protected override void DoMageAction(Magus mage)
        {
            // TODO: build size
            // TODO: pre-existing conditions
            mage.BuildLaboratory();
        }
    }

    public class RefineLaboratory : ExposingMageAction
    {
        public RefineLaboratory(Ability exposure, double desire)
            : base(exposure, desire)
        {
            Action = Activity.BuildLaboratory;
        }

        protected override void DoMageAction(Magus mage)
        {
            // TODO: implement
        }
    }

    public class InventSpell : ExposingMageAction
    {
        private Spell _spell;

        public InventSpell(Spell spell, Ability exposure, double desire)
            : base(exposure, desire)
        {
            _spell = spell;
            Action = Activity.InventSpells;
        }

        protected override void DoMageAction(Magus mage)
        {
            // TODO: multiple spells
            mage.InventSpell(_spell);
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
    #endregion
}
