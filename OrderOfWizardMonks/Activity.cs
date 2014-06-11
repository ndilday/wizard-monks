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
        Train,
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
        double Desire { get; set; }
        void Act(Character character);
        bool Matches(IAction action);
	}

    [Serializable]
    public class Reading : IAction
    {
        public Reading(IBook book, double desire)
        {
            Book = book;
            Desire = desire;
        }

        public IBook Book { get; private set; }

        public ushort? SeasonId { get; private set; }

        public Activity Action
        {
            get
            {
                return Activity.ReadBook;
            }
        }

        public double Desire { get; set; }

        public void Act(Character character)
        {
            character.ReadBook(Book);
        }

        public bool Matches(IAction action)
        {
            if (action.Action != Activity.ReadBook)
            {
                return false;
            }
            Reading reading = (Reading)action;
            return reading.Book == this.Book;
        }
    }

    [Serializable]
    public class Practice : IAction
    {
        public Practice(Ability ability, double desire)
        {
            Ability = ability;
            Desire = desire;
        }

        public Ability Ability { get; private set; }

        public ushort? SeasonId { get; private set; }

        public virtual Activity Action
        {
            get { return Activity.Practice; }
        }

        public double Desire { get; set; }

        public virtual void Act(Character character)
        {
            character.Log.Add("Practicing " + Ability.AbilityName);
            character.GetAbility(Ability).AddExperience(4);
        }

        public virtual bool Matches(IAction action)
        {
            if (action.Action != Activity.Practice)
            {
                return false;
            }
            Practice practice = (Practice)action;
            return practice.Ability == this.Ability;
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
            character.GetAbility(Ability).AddExperience(2);
        }

        public override bool Matches(IAction action)
        {
            if (action.Action != Activity.Sundry)
            {
                return false;
            }
            Exposure exposure = (Exposure)action;
            return exposure.Ability == this.Ability;
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

        public double Desire { get; set; }

        public void Act(Character character)
        {
            // TODO: currently this logic is over in teach; how do we get this to work?
        }

        public bool Matches(IAction action)
        {
            if (action.Action != Activity.Learn)
            {
                return false;
            }
            Learn learn = (Learn)action;
            // TODO: add logic here once we flesh out learning
            return true;
        }
    }
    #endregion

    #region ExposingAction classes
    public abstract class ExposingAction : IAction
    {
        public Ability Exposure { get; private set; }

        public ushort? SeasonId { get; protected set; }

        public Activity Action { get; protected set; }

        public double Desire { get; set; }

        protected ExposingAction(Ability exposure, double desire)
        {
            Exposure = exposure;
            Desire = desire;
        }

        public void Act(Character character)
        {
            DoAction(character);
 	        character.GetAbility(Exposure).AddExperience(2);
        }

        protected abstract void DoAction(Character character);

        public abstract bool Matches(IAction action);
    }

    [Serializable]
    public class Writing : ExposingAction
    {
        public Ability Topic { get; private set; }
        public double Level { get; private set; }

        public Writing(Ability topic, Ability exposure, double level, double desire)
            : base(exposure, desire)
        {
            Topic = topic;
            Level = level;
            Action = Activity.WriteBook;
        }

        protected override void DoAction(Character character)
        {
            character.WriteBook(Topic, Exposure, Level);
        }

        public override bool Matches(IAction action)
        {
            if (action.Action != Activity.WriteBook)
            {
                return false;
            }
            Writing writing = (Writing)action;
            return writing.Topic == this.Topic && writing.Level == this.Level;
        }
    }

    [Serializable]
    public class CopyBook : ExposingAction
    {
        // TODO: handle multiple books to copy
        public bool CopyQuickly { get; private set; }
        public IBook Book { get; private set; }

        public CopyBook(bool copyQuickly, IBook bookToCopy, Ability exposure, double desire)
            : base(exposure, desire)
        {
            CopyQuickly = copyQuickly;
            Book = bookToCopy;
            Action = Activity.CopyBook;
        }

        protected override void DoAction(Character character)
        {
            double scribeAbilityValue = character.GetAbility(Abilities.Scribing).Value;
            if(Book.Level == 1000)
            {
                Tractatus tract = new Tractatus
                {
                    Author = Book.Author,
                    Quality = Book.Quality,
                    Title = Book.Title,
                    Topic = Book.Topic
                };
                character.AddBookToCollection(tract);
            }
            else
            {
                // TODO: implement logic for copying summae
            }
        }

        public override bool Matches(IAction action)
        {
            if (action.Action != Activity.CopyBook)
            {
                return false;
            }
            CopyBook copy = (CopyBook)action;
            return copy.Book == this.Book && copy.CopyQuickly == this.CopyQuickly;
        }
    }

    [Serializable]
    public class FindAura : ExposingAction
    {
        public FindAura(Ability exposureAbility, double desire)
            : base(exposureAbility, desire)
        {
            Action = Activity.FindAura;
        }

        protected override void DoAction(Character character)
        {
            character.Log.Add("Searched for an aura");
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
            // TODO: once spells are implemented, increase finding chances based on aura-detection spells
            double areaLore = mage.GetAbility(Abilities.AreaLore).Value;
            areaLore += mage.GetCastingTotal(MagicArtPairs.InVi) / 5;
            double roll = Die.Instance.RollDouble() * 5;

            // die roll will be 0-5; area lore will be between 0 and 15, giving auras between 0 and 9
            double auraFound = Math.Sqrt(roll * areaLore);
            if (auraFound > 1)
            {
                mage.Log.Add("Found an aura of strength " + auraFound.ToString("0.00"));
                mage.FoundCovenant(auraFound);
            }
        }

        public override bool Matches(IAction action)
        {
            return action.Action == Activity.FindAura;
        }
    }

    [Serializable]
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
            double areaLore = mage.GetAbility(Abilities.AreaLore).Value;
            areaLore += mage.GetCastingTotal(MagicArtPairs.InVi) / 5;
            double roll = Die.Instance.RollDouble();
            if (roll > 1)
            {
                mage.TakeApprentice(CharacterFactory.GenerateNewMagus());
            }
            // TODO: gradual reduction in chance?
        }

        public override bool Matches(IAction action)
        {
            return action.Action == Activity.FindApprentice;
        }
    }

    [Serializable]
    public class Teach : ExposingAction
    {
        // TODO: enable multiple students
        public Character Student { get; private set; }
        public Ability AbilityToTeach { get; private set; }
        public Teach(Character student, Ability abilityToTeach, Ability exposure, double desire) : base(exposure, desire)
        {
            Action = Activity.Teach;
            Student = student;
            AbilityToTeach = abilityToTeach;
        }

        protected override void DoAction(Character character)
        {
            double abilityDifference = character.GetAbility(AbilityToTeach).Value - Student.GetAbility(AbilityToTeach).Value;
            if (abilityDifference <= 0)
            {
                throw new ArgumentOutOfRangeException("Teacher has nothing to teach this student!");
            }
            double amountTaught = 6 + character.GetAttribute(AttributeType.Communication).Value + character.GetAbility(Abilities.Teaching).Value;
            if (amountTaught > abilityDifference)
            {
                amountTaught = abilityDifference;
            }
            Student.GetAbility(AbilityToTeach).AddExperience(amountTaught);
        }

        public override bool Matches(IAction action)
        {
            if (action.Action != Activity.Teach)
            {
                return false;
            }
            Teach teach = (Teach)action;
            return teach.Student == this.Student && teach.AbilityToTeach == this.AbilityToTeach;
        }
    }

    [Serializable]
    public class Train : ExposingAction
    {
        // TODO: enable multiple students
        public Character Student { get; private set; }
        public Ability AbilityToTrain { get; private set; }
        public Train(Character student, Ability abilityToTrain, Ability exposure, double desire) : base(exposure, desire)
        {
            Action = Activity.Train;
            Student = student;
            AbilityToTrain = abilityToTrain;
        }

        protected override void DoAction(Character character)
        {
            double abilityDifference = character.GetAbility(AbilityToTrain).Value - Student.GetAbility(AbilityToTrain).Value;
            if (abilityDifference <= 0)
            {
                throw new ArgumentOutOfRangeException("Trainer has nothing to teach this student!");
            }
            double amountTrained = 3 + character.GetAbility(AbilityToTrain).Value;
            if (amountTrained > abilityDifference)
            {
                amountTrained = abilityDifference;
            }
            Student.GetAbility(AbilityToTrain).AddExperience(amountTrained);
        }

        public override bool Matches(IAction action)
        {
            if (action.Action != Activity.Train)
            {
                return false;
            }
            Train train = (Train)action;
            return train.AbilityToTrain == this.AbilityToTrain && train.Student == this.Student;
        }
    }
    #endregion

    #region MageAction Classes
    public abstract class MageAction : IAction
    {
        public ushort? SeasonId { get; protected set; }

        public Activity Action { get; protected set; }

        public double Desire { get; set; }

        public abstract void Act(Character character);

        protected Magus ConfirmCharacterIsMage(Character character)
        {
            if (typeof(Magus) != character.GetType())
            {
                throw new InvalidCastException("Only magi can extract vis!");
            }
            return (Magus)character;
        }

        public abstract bool Matches(IAction action);
    }

    [Serializable]
    public class VisStudying : MageAction
    {
        public Ability Art { get; private set; }

        public VisStudying(Ability art, double desire)
        {
            if (!MagicArts.IsArt(art))
            {
                throw new ArgumentException("Only magic arts have vis associated with them!");
            }
            Art = art;
            Desire = desire;
            Action = Activity.StudyVis;
        }

        public override void Act(Character character)
        {
            Magus mage = ConfirmCharacterIsMage(character);

            // determine the amount of vis needed
            CharacterAbilityBase charAbility = mage.GetAbility(Art);
            double visNeeded = 0.5 + (charAbility.Value / 10.0);

            // decrement the used vis
            mage.UseVis(Art, visNeeded);

            // add experience
            charAbility.AddExperience(Die.Instance.RollExplodingDie());
        }

        public override bool Matches(IAction action)
        {
            if (action.Action != Activity.StudyVis)
            {
                return false;
            }
            VisStudying study = (VisStudying)action;
            return study.Art == this.Art;
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
            double amount = mage.GetLabTotal(MagicArtPairs.CrVi, Activity.DistillVis) / 10.0;
            mage.Log.Add("Extracted " + amount.ToString("0.00") + " pawns of vis from aura");
            mage.Covenant.AddVis(MagicArts.Vim, mage.GetLabTotal(MagicArtPairs.CrVi, Activity.DistillVis) / 10);
        }

        public override bool Matches(IAction action)
        {
            return action.Action == Activity.DistillVis;
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
            mage.Log.Add("Built laboratory");
            mage.BuildLaboratory();
        }

        public override bool Matches(IAction action)
        {
            return action.Action == Activity.BuildLaboratory;
        }
    }

    public class RefineLaboratory : ExposingMageAction
    {
        public RefineLaboratory(Ability exposure, double desire)
            : base(exposure, desire)
        {
            Action = Activity.RefineLaboratory;
        }

        protected override void DoMageAction(Magus mage)
        {
            // TODO: we may want to do the check here as well to be safe
            mage.RefineLaboratory();
        }

        public override bool Matches(IAction action)
        {
            return action.Action == Activity.RefineLaboratory;
        }
    }

    public class InventSpell : ExposingMageAction
    {
        public Spell Spell { get; private set; }

        public InventSpell(Spell spell, Ability exposure, double desire)
            : base(exposure, desire)
        {
            Spell = spell;
            Action = Activity.InventSpells;
        }

        protected override void DoMageAction(Magus mage)
        {
            // TODO: multiple spells
            mage.InventSpell(Spell);
        }

        public override bool Matches(IAction action)
        {
            if (action.Action != Activity.InventSpells)
            {
                return false;
            }
            InventSpell invent = (InventSpell)action;
            return invent.Spell == this.Spell;
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

        public override bool Matches(IAction action)
        {
            return action.Action == Activity.CopyLabText;
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

        public override bool Matches(IAction action)
        {
            return action.Action == Activity.WriteLabText;
        }
    }
    
    public class LongevityRitual : ExposingMageAction
    {
        public LongevityRitual(Ability exposure, double desire) : base(exposure, desire)
        {
            Action = Activity.LongevityRitual;
        }

        protected override void DoMageAction(Magus mage)
        {
            uint strength = Convert.ToUInt16(mage.GetLabTotal(MagicArtPairs.CrVi, Activity.LongevityRitual));
            mage.Log.Add("Created a longevity ritual of strength " + strength);
            mage.ApplyLongevityRitual(Convert.ToUInt16(mage.GetLabTotal(MagicArtPairs.CrVi, Activity.LongevityRitual)));
        }

        public override bool Matches(IAction action)
        {
            return action.Action == Activity.LongevityRitual;
        }
    }
    #endregion
}
