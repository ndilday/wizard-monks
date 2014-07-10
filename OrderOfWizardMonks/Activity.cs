using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WizardMonks.Core;
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
        GauntletApprentice,
        FindAura,
		FindVisSource,
		Sundry,
        Practice
	}

    #region IAction Classes
    public interface IAction
	{
        Activity Action { get; }
        double Desire { get; set; }
        void Act(Character character);
        bool Matches(IAction action);
        string Log();
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

        public string Log()
        {
            return "Reading " + Book.Title + " worth " + Desire.ToString("0.00");
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

        public virtual string Log()
        {
            return "Practicing " + Ability.AbilityName + " worth " + Desire.ToString("0.00");
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

        public override string Log()
        {
            return "Exposing " + Ability.AbilityName + " worth " + Desire.ToString("0.00");
        }
    }

    [Serializable]
    public class Learn : IAction
    {
        private double _quality;
        private double _maxLevel;
        private Ability _topic;
        private Character _teacher;

        public Learn(double quality, double maxLevel, Ability topic, Character teacher)
        {
            _quality = quality;
            _topic = topic;
            _teacher = teacher;
            _maxLevel = maxLevel;
        }

        public Activity Action
        {
            get { return Activity.Learn; }
        }

        public double Desire { get; set; }

        public void Act(Character character)
        {
            character.GetAbility(_topic).AddExperience(_quality, _maxLevel);
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

        public string Log()
        {
            return "Learning worth " + Desire.ToString("0.00");
        }
    }
    #endregion

    #region ExposingAction classes
    public abstract class ExposingAction : IAction
    {
        public Ability Exposure { get; private set; }

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

        public abstract string Log();
    }

    [Serializable]
    public class Writing : ExposingAction
    {
        public Ability Topic { get; private set; }
        public double Level { get; private set; }
        public string Name { get; private set; }

        public Writing(Ability topic, string name, Ability exposure, double level, double desire)
            : base(exposure, desire)
        {
            Topic = topic;
            Level = level;
            Action = Activity.WriteBook;
            Name = name;
        }

        protected override void DoAction(Character character)
        {
            IBook book = character.WriteBook(Topic, Name, Exposure, Level);
            if (book != null)
            {
                character.Log.Add("Wrote " + book.Title + ": Q" + book.Quality.ToString("0.00"));
            }
            else
            {
                character.Log.Add("Worked on " + Name);
            }
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

        public override string Log()
        {
            if (Level == 1000)
            {
                return "Writing tractatus on " + Topic.AbilityName + " worth " + Desire.ToString("0.00");
            }
            return "Writing summa on " + Topic.AbilityName + " worth " + Desire.ToString("0.00");
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

        public override string Log()
        {
            throw new NotImplementedException();
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
            // add bonus to area lore equal to casting total div 10?
            // TODO: once spells are implemented, increase finding chances based on aura-detection spells
            double areaLore = mage.GetAbility(Abilities.AreaLore).Value;
            areaLore += mage.GetAttribute(AttributeType.Perception).Value;
            areaLore += mage.GetCastingTotal(MagicArtPairs.InVi) / 10;
            double roll = Die.Instance.RollDouble() * 5;

            // die roll will be 0-5; area lore will be between 0 and 15, giving auras between 0 and 9
            double auraFound = Math.Sqrt(roll * areaLore / (mage.KnownAuras.Count() + 1));
            if (auraFound > 1)
            {
                Aura aura = new Aura(Domain.Magic, auraFound);
                mage.Log.Add("Found an aura of strength " + auraFound.ToString("0.00"));
                mage.KnownAuras.Add(aura);
                if (mage.Covenant == null)
                {
                    mage.FoundCovenant(aura);
                }
            }
        }

        public override bool Matches(IAction action)
        {
            return action.Action == Activity.FindAura;
        }

        public override string Log()
        {
            return "Finding a new aura worth " + Desire.ToString("0.00");
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
            double folkKen = mage.GetAttribute(AttributeType.Perception).Value;
            folkKen += mage.GetCastingTotal(MagicArtPairs.InVi) / 10;
            double roll = Die.Instance.RollExplodingDie() + folkKen;
            if (roll > 12)
            {
                mage.Log.Add("Apprentice found");
                mage.TakeApprentice(CharacterFactory.GenerateNewApprentice());
                mage.Apprentice.Name = "Apprentice filius " + mage.Name;
            }
            // TODO: gradual reduction in chance?
        }

        public override bool Matches(IAction action)
        {
            return action.Action == Activity.FindApprentice;
        }

        public override string Log()
        {
            return "Finding apprentice worth " + Desire.ToString("0.00");
        }
    }

    [Serializable]
    public class FindVisSource : ExposingAction
    {
        public Aura Aura { get; private set; }

        public FindVisSource(Aura auraToSearch, Ability exposure, double desire) : base(exposure, desire)
        {
            Aura = auraToSearch;
            Action = Activity.FindVisSource;
        }

        protected override void DoAction(Character character)
        {
            if (character.GetType() == typeof(Magus))
            {
                character.Log.Add("Searching for a vis site in aura " + Aura.Strength.ToString("0.00"));
                Magus mage = (Magus)character;
                // add bonus to area lore equal to casting total div 5?
                // TODO: once spells are implemented, increase finding chances based on aura-detection spells
                double magicLore = mage.GetAbility(Abilities.MagicLore).Value;
                magicLore += mage.GetAttribute(AttributeType.Perception).Value;
                magicLore += mage.GetCastingTotal(MagicArtPairs.InVi) / 5;
                double roll = Die.Instance.RollDouble() * 5;

                // die roll will be 0-5; area lore will be between 0 and 25; aura will be 0-9, giving vis counts of 0-35
                double visSourceFound = Math.Sqrt(roll * magicLore * Aura.Strength);
                visSourceFound -= Aura.VisSources.Select(v => v.AnnualAmount).Sum();
                if (visSourceFound > 1.0)
                {
                    Season seasons = DetermineSeasons(ref visSourceFound);
                    Ability art = DetermineArt();
                    string logMessage = art.AbilityName + " vis source of size " + visSourceFound.ToString("0.00") + " found: ";
                    if ((seasons & Season.Spring) == Season.Spring)
                    {
                        logMessage += "Sp";
                    }
                    if ((seasons & Season.Summer) == Season.Summer)
                    {
                        logMessage += "Su";
                    }
                    if ((seasons & Season.Autumn) == Season.Autumn)
                    {
                        logMessage += "Au";
                    }
                    if ((seasons & Season.Winter) == Season.Winter)
                    {
                        logMessage += "Wi";
                    }
                    mage.Log.Add(logMessage);
                    Aura.VisSources.Add(new VisSource(Aura, art, seasons, visSourceFound));
                }
            }
        }

        private Ability DetermineArt()
        {
            double artRoll = Die.Instance.RollDouble() * 15;
            switch ((int)artRoll)
            {
                case 0:
                    return MagicArts.Creo;
                case 1:
                    return MagicArts.Intellego;
                case 2:
                    return MagicArts.Muto;
                case 3:
                    return MagicArts.Perdo;
                case 4:
                    return MagicArts.Rego;
                case 5:
                    return MagicArts.Animal;
                case 6:
                    return MagicArts.Aquam;
                case 7:
                    return MagicArts.Auram;
                case 8:
                    return MagicArts.Corpus;
                case 9:
                    return MagicArts.Herbam;
                case 10:
                    return MagicArts.Ignem;
                case 11:
                    return MagicArts.Imaginem;
                case 12:
                    return MagicArts.Mentem;
                case 13:
                    return MagicArts.Terram;
                default:
                    return MagicArts.Vim;
            }

        }

        private Season DetermineSeasons(ref double visSourceFound)
        {
            // we always want at least 1 pawn/harvest season
            // the larger the source, the more likely multiple seasons should be
            // let's call a rook of vis/season the max
            // TODO: make this math better
            int seasons;
            if (visSourceFound <= 4)
            {
                seasons = (int)(visSourceFound * Die.Instance.RollDouble());
            }
            else if (visSourceFound > 10)
            {
                seasons = 4;
            }
            else
            {
                seasons = (int)(visSourceFound / (Die.Instance.RollDouble() * 10)) + 1;
            }
            if (seasons < 1)
            {
                seasons = 1;
            }
            else if (seasons > 4)
            {
                seasons = 4;
            }
            visSourceFound /= seasons;
            switch (seasons)
            {
                case 4:
                    return Season.Spring | Season.Summer | Season.Autumn | Season.Winter;
                case 1:
                    double dieRoll = Die.Instance.RollDouble() * 4;
                    if (dieRoll < 1) return Season.Spring;
                    if (dieRoll < 2) return Season.Summer;
                    if (dieRoll < 3) return Season.Autumn;
                    return Season.Winter;
                case 3:
                    double dieRoll3 = Die.Instance.RollDouble() * 4;
                    if (dieRoll3 < 1) return Season.Summer | Season.Autumn | Season.Winter;
                    if (dieRoll3 < 2) return Season.Spring | Season.Autumn | Season.Winter;
                    if (dieRoll3 < 3) return Season.Spring | Season.Summer | Season.Winter;
                    return Season.Spring | Season.Summer | Season.Autumn;
                default:
                    double dieRoll1 = Die.Instance.RollDouble() * 4;
                    double dieRoll2;
                    do
                    {
                        dieRoll2 = Die.Instance.RollDouble() * 4;
                    } while ((int)dieRoll2 == (int)dieRoll1);
                    Season season;
                    if (dieRoll1 < 1) season = Season.Spring;
                    else if (dieRoll1 < 2) season = Season.Summer;
                    else if (dieRoll1 < 3) season = Season.Autumn;
                    else season = Season.Winter;
                    if (dieRoll2 < 1) season |= Season.Spring;
                    else if (dieRoll2 < 1) season |= Season.Summer;
                    else if (dieRoll2 < 1) season |= Season.Autumn;
                    else season |= Season.Winter;
                    return season;
            }
        }

        public override bool Matches(IAction action)
        {
            return action.Action == Activity.FindVisSource && ((FindVisSource)action).Aura == this.Aura;
        }

        public override string Log()
        {
            return "Finding vis source worth " + Desire.ToString("0.00");
        }
    }

    [Serializable]
    public class Teach : ExposingAction
    {
        public bool Completed { get; private set; }
        // TODO: enable multiple students
        public Character Student { get; private set; }
        public Ability Topic { get; private set; }
        public Teach(Character student, Ability abilityToTeach, Ability exposure, double desire) : base(exposure, desire)
        {
            Action = Activity.Teach;
            Student = student;
            Topic = abilityToTeach;
            Completed = false;
        }

        protected override void DoAction(Character character)
        {
            double experienceDifference = character.GetAbility(Topic).Experience - Student.GetAbility(Topic).Experience;
            if (experienceDifference <= 0)
            {
                throw new ArgumentOutOfRangeException("Teacher has nothing to teach this student!");
            }
            double quality = character.GetAbility(Abilities.Teaching).Value + character.GetAttributeValue(AttributeType.Communication) + 6;
            Student.Advance(new Learn(quality, character.GetAbility(Topic).Value, Topic, character));
            Completed = true;
        }

        public override bool Matches(IAction action)
        {
            if (action.Action != Activity.Teach)
            {
                return false;
            }
            Teach teach = (Teach)action;
            return teach.Student == this.Student && teach.Topic == this.Topic;
        }

        public override string Log()
        {
            return "Teaching worth " + Desire.ToString("0.00");
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

        public override string Log()
        {
            return "Training worth " + Desire.ToString("0.00");
        }
    }
    #endregion

    #region MageAction Classes
    public abstract class MageAction : IAction
    {
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

        public abstract string Log();
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
            ushort roll = Die.Instance.RollExplodingDie();
            double aura = mage.Covenant != null && mage.Covenant.Aura != null ? mage.Covenant.Aura.Strength : 0;
            double gain = roll + aura;
            character.Log.Add("Studying " + visNeeded.ToString("0.00") + " pawns of " + Art.AbilityName + " vis.");
            character.Log.Add("Gained " + gain + " experience.");
            charAbility.AddExperience(gain);
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

        public override string Log()
        {
            return "Studying " + Art.AbilityName + " vis worth " + Desire.ToString("0.00");
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

    public class GauntletApprentice : ExposingMageAction
    {
        public GauntletApprentice(Ability exposure, double desire) : base(exposure, desire) { }

        public override bool Matches(IAction action)
        {
            return action.Action == Activity.GauntletApprentice;
        }

        public override string Log()
        {
            return "Gauntleting apprentice worth " + Desire.ToString("0.00");
        }

        protected override void DoMageAction(Magus mage)
        {
            mage.GauntletApprentice();
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
            double amount = mage.GetLabTotal(MagicArtPairs.CrVi, Activity.DistillVis) / 10.0;
            mage.Log.Add("Extracted " + amount.ToString("0.00") + " pawns of vis from aura");
            mage.Covenant.AddVis(MagicArts.Vim, mage.GetLabTotal(MagicArtPairs.CrVi, Activity.DistillVis) / 10);
        }

        public override bool Matches(IAction action)
        {
            return action.Action == Activity.DistillVis;
        }

        public override string Log()
        {
            return "Extracting vis worth " + Desire.ToString("0.00");
        }
    }

    public class BuildLaboratory : ExposingMageAction
    {
        private Aura _aura;

        public BuildLaboratory(Ability exposure, double desire)
            : base(exposure, desire)
        {
            Action = Activity.BuildLaboratory;
        }

        public BuildLaboratory(Aura aura, Ability exposure, double desire)
            : base(exposure, desire)
        {
            Action = Activity.BuildLaboratory;
            _aura = aura;
        }

        protected override void DoMageAction(Magus mage)
        {
            // TODO: build size
            // TODO: pre-existing conditions
            mage.Log.Add("Built laboratory");
            if (_aura == null)
            {
                mage.BuildLaboratory();
            }
            else
            {
                mage.BuildLaboratory(_aura);
            }
        }

        public override bool Matches(IAction action)
        {
            return action.Action == Activity.BuildLaboratory;
        }

        public override string Log()
        {
            return "Building lab worth " + Desire.ToString("0.00");
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

        public override string Log()
        {
            return "Refining lab worth " + Desire.ToString("0.00");
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

        public override string Log()
        {
            return "Inventing " + Spell.BaseArts.Technique.AbilityName + " " + Spell.BaseArts.Form.AbilityName + " spell worth " + Desire.ToString("0.00");
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

        public override string Log()
        {
            throw new NotImplementedException();
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

        public override string Log()
        {
            throw new NotImplementedException();
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

        public override string Log()
        {
            return "Longevity Ritual worth " + Desire.ToString("0.00");
        }
    }
    #endregion
}
