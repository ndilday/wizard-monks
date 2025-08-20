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
        public List<LabText> LabTextsOwned { get; private set; }
        public Ability MagicAbility { get; private set; }
        public Magus Apprentice { get; private set; }
        public uint ApprenticeTrainingStartSeason { get; set; }
        public uint LastSeasonTrainedApprentice { get; set; } 
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
        public Covenant Covenant { get; set; }
        public Arts Arts { get; private set; }
        public double VisStudyRate { get; set; }
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
            ActiveGoals.Add(new AvoidDecrepitudeGoal(this, 1.0));
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
                ActiveGoals.Add(new PursueIdeaGoal(this, idea));
            }
        }
        #endregion

        #region Apprentice Functions
        public void TakeApprentice(Magus apprentice)
        {
            if (Apprentice != null)
            {
                // Can't take a new one while you still have one
                return;
            }
            Apprentice = apprentice;

            // The "Opening of the Arts" counts as the first year's training.
            // We record the current season as the start and last-trained timestamp.
            ApprenticeTrainingStartSeason = this.SeasonalAge;
            LastSeasonTrainedApprentice = this.SeasonalAge;
        }

        public void GauntletApprentice()
        {
            if (Apprentice != null)
            {
                Apprentice.House = this.House;
                Apprentice = null;
                Apprentice.Covenant.RemoveMagus(Apprentice);
                Apprentice.Covenant.AddMagus(Apprentice, CovenantRole.Visitor);
                ApprenticeTrainingStartSeason = 0;
                LastSeasonTrainedApprentice = 0;
            }
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
