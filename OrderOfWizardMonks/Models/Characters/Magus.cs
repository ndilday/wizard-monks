using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Decisions.Goals;
using WizardMonks.Instances;
using WizardMonks.Models.Books;
using WizardMonks.Models.Covenants;
using WizardMonks.Models.Ideas;
using WizardMonks.Models.Laboratories;
using WizardMonks.Models.Spells;
using WizardMonks.Services.Characters;

namespace WizardMonks.Models.Characters
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
	public partial class Magus : Character
    {
        #region Private Fields
        private HousesEnum _house;
        private List<AIdea> _ideas;
        #endregion

        #region Public Properties
        public Spell PartialSpell { get; set; }
        public double PartialSpellProgress { get; set; }
        public List<LabText> LabTextsOwned { get; private set; }
        public Ability MagicAbility { get; private set; }
        public Magus Apprentice { get; private set; }
        public Laboratory Laboratory { get; set; }
        public List<Spell> SpellList { get; private set; }
        public Dictionary<Ability, double> VisStock { get; private set; }
        public Dictionary<Character, ushort> DecipheredShorthandLevels { get; private set; }
        public Dictionary<LabText, double> ShorthandTranslationProgress { get; private set; }
        public HousesEnum House
        {
            get
            {
                return _house;
            }
            set
            {
                _house = value;
                WantsToFollow = false;
            }
        }
        public Covenant Covenant { get; private set; }
        public Arts Arts { get; private set; }
        public double VisStudyRate { get; private set; }
        #endregion

        #region Initialization Functions
        public Magus() : this(Abilities.MagicTheory, Abilities.Latin, Abilities.ArtesLiberales, Abilities.AreaLore, HousesEnum.Apprentice, 80, null, null) { }
        public Magus(HousesEnum house, uint age, Personality personality, Dictionary<string, double> reputationFocuses) : this(Abilities.MagicTheory, Abilities.Latin, Abilities.ArtesLiberales, Abilities.AreaLore, house, age, personality, reputationFocuses) { }
        public Magus(Ability magicAbility, Ability writingLanguage, Ability writingAbility, Ability areaAbility, HousesEnum house = HousesEnum.Apprentice, uint baseAge = 20, Personality personality = null, Dictionary<string, double> reputationFocuses = null)
            : base(writingLanguage, writingAbility, areaAbility, baseAge, personality, reputationFocuses)
        {
            MagicAbility = magicAbility;
            Arts = new Arts(this.InvalidateWritableTopicsCache);
            Covenant = null;
            Laboratory = null;
            VisStock = [];
            SpellList = [];
            LabTextsOwned = [];
            DecipheredShorthandLevels = [];
            ShorthandTranslationProgress = [];
            PartialSpell = null;
            PartialSpellProgress = 0;
            _ideas = [];
            VisStudyRate = 6.75;
            House = house;
            foreach (Ability art in MagicArts.GetEnumerator())
            {
                VisStock[art] = 0;
            }

            InitializeGoals();
        }

        private void InitializeGoals()
        {
            Goals.Add(new AvoidDecrepitudeGoal(this, 1.0));
        }
        #endregion

        #region Ability Functions
        public override CharacterAbilityBase GetAbility(Ability ability)
        {
            if (MagicArts.IsArt(ability))
            {
                return Arts.GetAbility(ability);
            }
            else
            {
                return base.GetAbility(ability);
            }
        }
        #endregion

        #region Covenant Functions
        public void Join(Covenant covenant)
        {
            if (Covenant != null)
            {
                Covenant.RemoveMagus(this);
            }
            Covenant = covenant;
            covenant.AddMagus(this);
            VisStudyRate = 6.75 + covenant.Aura.Strength;
        }

        public Covenant FoundCovenant(Aura aura)
        {
            Covenant coventant = new(aura);
            Join(coventant);
            return coventant;
        }
        #endregion

        #region Idea Functions
        public IEnumerable<AIdea> GetInspirations()
        {
            return _ideas;
        }

        public void AddIdea(AIdea idea)
        {
            // Prevent adding duplicate ideas, if we later decide ideas can be shared
            if (!_ideas.Any(i => i.Id == idea.Id))
            {
                _ideas.Add(idea);
                Log.Add($"Gained a new idea: {idea.Description}");

                // Add a new goal to pursue this inspiration
                Goals.Add(new PursueIdeaGoal(this, idea));
            }
        }
        #endregion

        #region Apprentice Functions
        public void TakeApprentice(Magus apprentice)
        {
            // TODO: what sort of error checking should go here?
            Apprentice = apprentice;
            // add a teaching goal for each year
            for (byte i = 2; i < 16; i++)
            {
                uint dueDate = (uint)(i * 4);
                IGoal teachingGoal = new TeachApprenticeGoal(this, SeasonalAge + i - 1, 1);
                Goals.Add(teachingGoal);
            }
            IGoal gauntletGoal = new GauntletApprenticeGoal(this, SeasonalAge + 60, 1);
            Goals.Add(gauntletGoal);
        }

        public void GauntletApprentice()
        {
            Apprentice.House = House;
            Apprentice = null;
        }
        #endregion

        #region object Overrides
        public override string ToString()
        {
            return Name + " ex " + House.ToString();
        }
        #endregion
    }
}
