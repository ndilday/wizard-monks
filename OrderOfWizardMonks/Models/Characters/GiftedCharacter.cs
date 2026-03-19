using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Activities;
using WizardMonks.Instances;
using WizardMonks.Models.Books;
using WizardMonks.Models.Spells;
using WizardMonks.Models.Traditions;

namespace WizardMonks.Models.Characters
{
    /// <summary>
    /// A character who possesses the Gift — the innate magical potential that
    /// allows a person to be trained in a magical tradition.
    ///
    /// A GiftedCharacter may exist in two states:
    ///   - Gift unopened: Tradition is null. The character has the Gift but has
    ///     not yet been initiated into any tradition. A gifted child before their
    ///     Opening of the Arts is in this state.
    ///   - Gift opened: Tradition is non-null. The character has been initiated
    ///     into a magical tradition and can practice magic.
    ///
    /// Art scores, spell lists, and vis stock are all initialized at Opening
    /// time (when Tradition is set), not at construction. This reflects that
    /// these capabilities depend on having a tradition to define them.
    ///
    /// The Gift imposes a social penalty on all interactions with non-Gifted
    /// characters. This is a property of being Gifted, not of having a tradition,
    /// and applies even before the Gift is Opened.
    /// </summary>
    [Serializable]
    public class GiftedCharacter : Character
    {
        #region Private Fields
        // Tradition-defined ability scores (hedge arts, Hermetic arts for non-
        // HermeticMagus subclasses). Keyed by AbilityId for O(1) lookup.
        // For HermeticMagus, Hermetic art lookups bypass this dictionary and
        // go directly to a fixed array in that subclass.
        private readonly Dictionary<int, CharacterAbilityBase> _traditionAbilities;
        #endregion

        #region Public Properties

        /// <summary>
        /// This character's personal magical tradition. Null until the Gift
        /// is Opened by a master. All magical capabilities derive from this
        /// object once set.
        /// </summary>
        public MagicalTradition Tradition { get; set; }

        /// <summary>
        /// True if this character's Gift has been Opened and they are an active
        /// practitioner of a magical tradition.
        /// </summary>
        public bool IsOpened => Tradition != null;

        /// <summary>
        /// Spells this character has learned. Only meaningful once the Gift
        /// is Opened. Populated as the character invents or learns spells.
        /// </summary>
        public List<Spell> SpellList { get; private set; }

        /// <summary>
        /// This character's personal vis stockpile, keyed by magical Art.
        /// Initialized at Opening time with zero quantities for all Arts
        /// defined by the tradition.
        /// </summary>
        public Dictionary<Ability, double> VisStock { get; private set; }

        #endregion

        #region Constructors

        public GiftedCharacter(
            Ability writingLanguage,
            Ability writingAbility,
            Ability areaAbility,
            uint baseAge = 20,
            Personality personality = null,
            Dictionary<string, double> reputationFocuses = null)
            : base(writingLanguage, writingAbility, areaAbility, baseAge, personality, reputationFocuses)
        {
            _traditionAbilities = [];
            SpellList = [];
            VisStock = [];
            Tradition = null;
        }

        #endregion

        #region Gift Opening

        /// <summary>
        /// Called by GiftOpeningService when an Opening succeeds. Sets the
        /// tradition and initializes all tradition-dependent state.
        ///
        /// This method initializes tradition ability scores at zero for all
        /// MagicalAbilityPrinciple concepts in the tradition. Actual experience
        /// values are populated by GiftOpeningService after calling this method,
        /// via GetAbility().AddExperience().
        ///
        /// Should only be called once per Opening event. Re-opening replaces
        /// the tradition entirely via GiftOpeningService.
        /// </summary>
        public virtual void OpenGift(MagicalTradition tradition)
        {
            Tradition = tradition ?? throw new ArgumentNullException(nameof(tradition));

            // Initialize ability slots for all magical abilities in the tradition.
            // Art-type abilities use AcceleratedAbility; others use CharacterAbility.
            foreach (var concept in tradition.GetConceptsOfType<MagicalAbilityPrinciple>())
            {
                var ability = concept.Principle is MagicalAbilityPrinciple map
                    ? map.Ability
                    : null;

                if (ability == null) continue;
                if (_traditionAbilities.ContainsKey(ability.AbilityId)) continue;

                CharacterAbilityBase charAbility = ability.AbilityType == AbilityType.Art
                    ? new AcceleratedAbility(ability)
                    : new CharacterAbility(ability);

                _traditionAbilities[ability.AbilityId] = charAbility;
            }

            // Initialize vis stock at zero for all arts in the tradition.
            foreach (var concept in tradition.GetConceptsOfType<MagicalAbilityPrinciple>())
            {
                if (concept.Principle is MagicalAbilityPrinciple map &&
                    MagicArts.IsArt(map.Ability))
                {
                    if (!VisStock.ContainsKey(map.Ability))
                        VisStock[map.Ability] = 0;
                }
            }

            Log.Add($"Gift opened into tradition '{tradition.Name}'.");
        }

        #endregion

        #region Ability Functions

        /// <summary>
        /// Returns the CharacterAbilityBase for the given ability. Routes
        /// tradition-defined abilities to the tradition ability dictionary.
        /// General abilities (non-magical) fall through to the base Character
        /// dictionary. HermeticMagus overrides this to route the fifteen
        /// canonical Hermetic arts to a fast fixed array.
        /// </summary>
        public override CharacterAbilityBase GetAbility(Ability ability)
        {
            // Check tradition abilities first if the Gift is opened.
            if (IsOpened && _traditionAbilities.TryGetValue(ability.AbilityId, out var traditionAbility))
                return traditionAbility;

            // Fall through to base Character dictionary for non-magical abilities.
            return base.GetAbility(ability);
        }

        /// <summary>
        /// Returns all CharacterAbilityBase instances for this character,
        /// including both general abilities and tradition-defined abilities.
        /// </summary>
        public override IEnumerable<CharacterAbilityBase> GetAbilities()
        {
            return base.GetAbilities().Concat(_traditionAbilities.Values);
        }

        /// <summary>
        /// Returns only the tradition-defined ability scores. Used by
        /// GiftOpeningService when computing the Ease Factor for re-opening.
        /// </summary>
        public IEnumerable<CharacterAbilityBase> GetTraditionAbilities()
        {
            return _traditionAbilities.Values;
        }

        #endregion

        #region Spell Functions

        public Spell GetBestSpell(SpellBase spellBase)
        {
            return SpellList
                .Where(s => s.Base == spellBase)
                .OrderByDescending(s => s.Level)
                .FirstOrDefault();
        }

        #endregion
    }
}