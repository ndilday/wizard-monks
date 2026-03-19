using System;
using System.Linq;
using WizardMonks.Activities;
using WizardMonks.Instances;
using WizardMonks.Models.Covenants;
using WizardMonks.Models.Traditions;
using WizardMonks.Services.Characters;
using WizardMonks.Services.Traditions;

namespace WizardMonks.Models.Characters
{
    /// <summary>
    /// A GiftedCharacter whose Gift has been Opened by a non-Hermetic magical
    /// tradition. Carries the HedgeTradition social/recruitment metadata
    /// alongside the MagicalTradition capability model inherited from
    /// GiftedCharacter.
    ///
    /// A HedgeMagus can be converted to a HermeticMagus via BecomeHermeticMagus,
    /// which constructs a new HermeticMagus object. The original HedgeMagus
    /// instance should be replaced in all registries at that point. The
    /// permanent magical lineage is preserved on the new mage's
    /// MagicalTradition.Opener, which records the pre-Hermetic opening,
    /// and then the Hermetic opening is appended via RecordOpening.
    /// </summary>
    public class HedgeMagus : GiftedCharacter
    {
        /// <summary>
        /// Social and recruitment metadata for this mage's tradition.
        /// Carries the narrative identity and non-magical conversion bonuses
        /// applied if this mage is gauntleted into the Order.
        /// </summary>
        public HedgeTradition HedgeTradition { get; private set; }

        public HedgeMagus(HedgeTradition hedgeTradition, MagicalTradition magicalTradition)
            : this(hedgeTradition, magicalTradition, Abilities.Latin, Abilities.ArtesLiberales) { }

        public HedgeMagus(
            HedgeTradition hedgeTradition,
            MagicalTradition magicalTradition,
            Ability writingLanguage,
            Ability writingAbility)
            : base(writingLanguage, writingAbility, Abilities.AreaLore)
        {
            HedgeTradition = hedgeTradition
                ?? throw new ArgumentNullException(nameof(hedgeTradition));

            // Open the gift immediately with the provided tradition.
            // A HedgeMagus is always constructed in the opened state.
            OpenGift(magicalTradition
                ?? throw new ArgumentNullException(nameof(magicalTradition)));
        }

        /// <summary>
        /// Converts this HedgeMagus into a HermeticMagus by having their
        /// Gift re-opened by a Hermetic master. This is the founding-era
        /// operation: Bonisagus opening each of the pre-Hermetic Founders.
        ///
        /// Constructs a new HermeticMagus object. The caller is responsible
        /// for replacing all references to this HedgeMagus with the returned
        /// HermeticMagus in the world simulation's character registry.
        ///
        /// Returns null if the master's Opening Total is insufficient to
        /// meet the Ease Factor.
        /// </summary>
        public HermeticMagus BecomeHermeticMagus(
            HermeticMagus master,
            HousesEnum joiningHouse,
            GiftOpeningService openingService)
        {
            if (master == null) throw new ArgumentNullException(nameof(master));
            if (openingService == null) throw new ArgumentNullException(nameof(openingService));

            double openingTotal = master.GetLabTotal(
                MagicArtPairs.InVi,
                Activity.OpenArts);

            var result = openingService.ReOpenArtsHermetic(master, this, openingTotal);

            if (!result.Succeeded)
            {
                master.Log.Add(
                    $"[Gift Opening] {result.Describe()} " +
                    $"{Name}'s Gift cannot be opened for Hermetic Arts.");
                return null;
            }

            // Construct the new HermeticMagus from this character's base state.
            var newMagus = new HermeticMagus(
                joiningHouse,
                this.SeasonalAge,
                this.Personality,
                this.ReputationFocuses)
            {
                Name = this.Name
            };

            // Assign the translated MagicalTradition from the Opening result.
            newMagus.OpenGift(result.Tradition);

            // Copy non-magical ability experience.
            CopyNonMagicalAbilities(newMagus);

            // Apply conversion bonuses for directly-translatable skills
            // (language fluency, area lore, etc.).
            foreach (var (ability, experience) in HedgeTradition.ConversionBonuses)
            {
                newMagus.GetAbility(ability).AddExperience(experience);
            }

            // Add the new mage to the master's covenant as a visitor.
            if (master.Covenant != null)
            {
                newMagus.JoinCovenant(master.Covenant, CovenantRole.Visitor);
            }

            master.Log.Add(
                $"[Gift Opening] {result.Describe()} " +
                $"{Name} has joined the Order as {newMagus}.");

            return newMagus;
        }

        /// <summary>
        /// Copies all non-magical ability experience from this HedgeMagus to
        /// the target HermeticMagus. Tradition abilities (magical arts) are
        /// excluded — those are handled by GiftOpeningService's Art translation.
        /// Attributes are also copied.
        /// </summary>
        private void CopyNonMagicalAbilities(HermeticMagus target)
        {
            foreach (var charAbility in GetAbilities())
            {
                // Skip magical arts and tradition abilities — handled by Opening.
                if (MagicArts.IsArt(charAbility.Ability)) continue;
                if (Tradition != null &&
                    Tradition.GetConceptsOfType<MagicalAbilityPrinciple>()
                             .Any(c => ((MagicalAbilityPrinciple)c.Principle).Ability.AbilityId
                                       == charAbility.Ability.AbilityId)) continue;
                if (charAbility.Experience <= 0) continue;

                target.GetAbility(charAbility.Ability).AddExperience(charAbility.Experience);
            }

            // Copy attributes.
            foreach (AttributeType attrType in Enum.GetValues(typeof(AttributeType)))
            {
                target.GetAttribute(attrType).BaseValue = this.GetAttribute(attrType).BaseValue;
            }
        }
    }
}