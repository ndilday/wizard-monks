using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Decisions;
using WizardMonks.Decisions.Goals;
using WizardMonks.Instances;
using WizardMonks.Models.Books;
using WizardMonks.Models.Covenants;
using WizardMonks.Models.Ideas;
using WizardMonks.Models.Laboratories;
using WizardMonks.Models.Traditions;
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

    /// <summary>
    /// A GiftedCharacter who has been opened for Hermetic magic and sworn
    /// the Oath of Hermes. The defining member of the Order of Hermes.
    ///
    /// HermeticMagus carries the fifteen canonical Hermetic Art scores in a
    /// fixed-structure Arts object (the hot path for the decision engine),
    /// plus the Order-specific social and physical infrastructure: Covenant,
    /// Laboratory, House, and apprentice relationship.
    ///
    /// The Apprentice field tracks the current social/legal apprenticeship
    /// relationship, which is mutable (another Bonisagus can claim the
    /// apprentice). The permanent magical lineage record lives on
    /// MagicalTradition.Opener and is set at Opening time by GiftOpeningService.
    ///
    /// For backward compatibility during migration, the type alias
    /// "HermeticMagus" is provided at the bottom of this file.
    /// </summary>
    [Serializable]
    public partial class HermeticMagus : GiftedCharacter
    {
        #region Private Fields
        private HousesEnum _house;
        private List<AIdea> _ideas;
        #endregion

        #region Public Properties

        /// <summary>
        /// The fifteen Hermetic Arts, stored in a fixed-structure object
        /// with a switch-based GetAbility for O(1) lookup on the hot path.
        /// Initialized in the constructor; populated with experience at
        /// OpenGift time.
        /// </summary>
        public Arts Arts { get; private set; }

        /// <summary>The Ability that drives magical lab work and research.</summary>
        public Ability MagicAbility { get; private set; }

        public Laboratory Laboratory { get; set; }
        public List<LabText> LabTextsOwned { get; private set; }

        /// <summary>
        /// The current social/legal apprentice relationship. Mutable —
        /// another HermeticMagus may claim this mage's apprentice under
        /// the Code of Hermes. See MagicalTradition.Opener for the immutable
        /// magical lineage record.
        /// </summary>
        public HermeticMagus Apprentice { get; private set; }
        public uint ApprenticeTrainingStartSeason { get; set; }
        public uint LastSeasonTrainedApprentice { get; set; }

        public Dictionary<Character, ushort> DecipheredShorthandLevels { get; private set; }
        public Dictionary<LabText, double> ShorthandTranslationProgress { get; private set; }

        public HousesEnum House
        {
            get => _house;
            set
            {
                _house = value;
                WantsToFollow = false;
            }
        }

        public Covenant Covenant { get; set; }

        #endregion

        #region Constructors

        public HermeticMagus()
            : this(Abilities.MagicTheory, Abilities.Latin, Abilities.ArtesLiberales,
                   Abilities.AreaLore, HousesEnum.Apprentice, 80, null, null)
        { }

        public HermeticMagus(HousesEnum house, uint age, Personality personality,
            Dictionary<string, double> reputationFocuses)
            : this(Abilities.MagicTheory, Abilities.Latin, Abilities.ArtesLiberales,
                   Abilities.AreaLore, house, age, personality, reputationFocuses)
        { }

        public HermeticMagus(
            Ability magicAbility,
            Ability writingLanguage,
            Ability writingAbility,
            Ability areaAbility,
            HousesEnum house = HousesEnum.Apprentice,
            uint baseAge = 20,
            Personality personality = null,
            Dictionary<string, double> reputationFocuses = null)
            : base(writingLanguage, writingAbility, areaAbility, baseAge, personality, reputationFocuses)
        {
            MagicAbility = magicAbility;

            // Initialize Arts structure — all start at zero experience.
            // The change listener wires art-score changes to the writable
            // topics cache invalidation on Character.
            Arts = new Arts(this.InvalidateWritableTopicsCache);

            Covenant = null;
            Laboratory = null;
            LabTextsOwned = [];
            DecipheredShorthandLevels = [];
            ShorthandTranslationProgress = [];
            _ideas = [];
            House = house;

            // VisStock is initialized for Hermetic arts specifically here
            // because HermeticMagus may be constructed before OpenGift is called
            // (e.g., the Founders during simulation setup). GiftedCharacter.OpenGift
            // will also initialize VisStock from the tradition, so this is a safe
            // default that ensures no null-reference issues before Opening.
            foreach (Ability art in MagicArts.GetEnumerator())
            {
                VisStock[art] = 0;
            }

            InitializeGoals();
        }

        private void InitializeGoals()
        {
            // Bootstrap intention — seeded directly at construction before any WorldEvents
            // exist to drive organic goal generation. The GoalGenerator's dedup check will
            // find this in ActiveIntentions and skip creating a duplicate.
            ActiveIntentions.Add(new Intention(
                new AvoidDecrepitudeGoal(this, 1.0),
                commitmentStrength: 0.8f,
                initialDesireScore: 0.8f,
                formationTick: (int)SeasonalAge,
                maxStagnationTicks: 24));

            SeedInitialCognitiveState();
        }

        /// <summary>
        /// Seeds ambient emotional state and self-beliefs at construction, representing
        /// the emotional residue of prior experience. This gives the GoalGenerator enough
        /// signal to fire on the first tick for experienced characters.
        ///
        /// Personality facets above 1.0 (above average) produce proportional ambient state.
        /// The seeded intensities are intentionally modest — enough to clear thresholds
        /// for high-facet characters but not so large as to dominate the simulation.
        /// </summary>
        private void SeedInitialCognitiveState()
        {
            int tick = (int)SeasonalAge;
            float inquisitiveness = (float)Personality.GetFacet(HexacoFacet.Inquisitiveness);
            float prudence = (float)Personality.GetFacet(HexacoFacet.Prudence);
            float fearfulness = (float)Personality.GetFacet(HexacoFacet.Fearfulness);

            // High Inquisitiveness → ambient Pride from years of magical work.
            if (inquisitiveness > 1.0f)
            {
                float prideIntensity = (inquisitiveness - 1.0f) * 0.4f;
                Emotions.Add(new EmotionToken(EmotionType.Pride, prideIntensity, 0.05f, tick));
                CognitiveBeliefs.SelfBeliefs.Upsert("MagicalCompetence",
                    (inquisitiveness - 1.0f) * 0.5f, 0.4f, tick);
            }

            // High Prudence + high Fearfulness + older age → ambient Fear of aging.
            // SeasonalAge >= 140 corresponds to roughly age 35, when aging begins.
            if (prudence > 1.0f && fearfulness > 1.0f && SeasonalAge >= 140)
            {
                float fearIntensity = (prudence - 1.0f) * (fearfulness - 1.0f) * 0.3f;
                Emotions.Add(new EmotionToken(EmotionType.Fear, fearIntensity, 0.05f, tick));
                CognitiveBeliefs.SelfBeliefs.Upsert("Vulnerability",
                    (prudence - 1.0f) * 0.3f, 0.3f, tick);
            }
        }

        #endregion

        #region Gift Opening

        /// <summary>
        /// Opens this mage's Gift for Hermetic magic. In addition to the base
        /// GiftedCharacter opening (which initializes tradition abilities), this
        /// sets the arts in the Arts object to zero, ready to receive translated
        /// experience from GiftOpeningService.
        ///
        /// For the Founders, this is called after Bonisagus performs the Opening
        /// ritual. For standard apprentices, this is called at the start of their
        /// apprenticeship.
        /// </summary>
        public override void OpenGift(MagicalTradition tradition)
        {
            base.OpenGift(tradition);
            // Arts are already initialized to zero in the constructor.
            // GiftOpeningService will add translated experience via GetAbility().AddExperience().
        }

        #endregion

        #region Ability Functions

        /// <summary>
        /// Routes Hermetic Art lookups (AbilityId 300-314) to the fixed Arts
        /// object for O(1) performance. All other abilities fall through to
        /// the GiftedCharacter and Character dictionaries.
        /// </summary>
        public override CharacterAbilityBase GetAbility(Ability ability)
        {
            // Hot path: Hermetic Arts via fixed Arts object.
            var art = Arts.GetAbility(ability);
            if (art != null) return art;

            // Fall through to GiftedCharacter (tradition abilities + general abilities).
            return base.GetAbility(ability);
        }

        /// <summary>
        /// Returns all abilities including Hermetic Arts, tradition abilities,
        /// and general abilities.
        /// </summary>
        public override IEnumerable<CharacterAbilityBase> GetAbilities()
        {
            return base.GetAbilities().Concat(Arts);
        }

        #endregion

        #region Idea Functions

        public IEnumerable<AIdea> GetInspirations() => _ideas;

        public void AddIdea(AIdea idea)
        {
            if (_ideas.Any(i => i.Id == idea.Id)) return;

            _ideas.Add(idea);
            Log.Add($"Gained a new idea: {idea.Description}");

            // Immediately form an intention — ideas are an unambiguous catalyst.
            // Commitment and desire are personality-derived; no pressure threshold needed.
            float inquisitiveness = (float)Personality.GetFacet(HexacoFacet.Inquisitiveness);
            float creativity = (float)Personality.GetFacet(HexacoFacet.Creativity);
            float commitment = Math.Clamp((inquisitiveness + creativity) / 2f * 0.5f, 0.1f, 0.9f);
            float desire = Math.Clamp(inquisitiveness * 0.5f, 0.1f, 1.0f);
            int stagnation = (int)Math.Clamp(8 * Personality.GetFacet(HexacoFacet.Prudence), 2, 24);

            ActiveIntentions.Add(new Intention(
                new PursueIdeaGoal(this, idea),
                commitment, desire, (int)SeasonalAge, stagnation));
        }

        #endregion

        #region Apprentice Functions

        public void TakeApprentice(HermeticMagus apprentice)
        {
            if (Apprentice != null) return;

            Apprentice = apprentice;
            ApprenticeTrainingStartSeason = this.SeasonalAge;
            LastSeasonTrainedApprentice = this.SeasonalAge;
        }

        /// <summary>
        /// Claims another mage's apprentice, as permitted under the Code of
        /// Hermes for House Bonisagus. Updates the social apprentice relationship
        /// on both the previous master (if any) and this mage. Does not affect
        /// the apprentice's MagicalTradition.Opener, which is permanent.
        /// </summary>
        public void ClaimApprentice(HermeticMagus apprentice, HermeticMagus previousMaster)
        {
            if (previousMaster?.Apprentice == apprentice)
            {
                previousMaster.Apprentice = null;
                previousMaster.Log.Add(
                    $"{Name} has claimed {apprentice.Name} as their apprentice.");
            }
            TakeApprentice(apprentice);
        }

        public void GauntletApprentice()
        {
            if (Apprentice == null) return;

            Apprentice.House = this.House;
            if (Apprentice.Covenant != null)
            {
                Apprentice.Covenant.RemoveMagus(Apprentice);
                Apprentice.Covenant.AddMagus(Apprentice, CovenantRole.Visitor);
            }
            Apprentice = null;
            ApprenticeTrainingStartSeason = 0;
            LastSeasonTrainedApprentice = 0;
        }

        #endregion

        #region Aura & Vis

        public IEnumerable<Aura> GetOwnedAuras()
        {
            return Beliefs
                .Where(b => b.Key is Aura aura &&
                            b.Value.GetBeliefMagnitude(Models.Beliefs.BeliefTopics.Owner.Name) > 0)
                .Select(b => (Aura)b.Key);
        }

        #endregion

        #region Object Overrides

        public override string ToString() => $"{Name} ex {House}";

        #endregion
    }
}