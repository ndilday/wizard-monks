using System.Collections.Generic;
using WizardMonks.Models.Traditions;

namespace WizardMonks.Models.Characters
{
    /// <summary>
    /// Describes a pre-Hermetic magical tradition as a social and recruitment
    /// entity. Carries the narrative identity of the tradition and the conversion
    /// bonuses applied to non-magical abilities when a HedgeMagus is gauntleted
    /// into the Order of Hermes.
    ///
    /// The magical capabilities of the tradition are described by the associated
    /// MagicalTradition instance, which lives on each individual HedgeMagus.
    /// PotentialBreakthroughs have been removed: a Hermetic mage who studies a
    /// hedge tradition's MagicalTradition discovers researchable concepts by
    /// examining which concepts in that tradition are absent from their own and
    /// reading their BreakthroughPointsRequired. No separate list is needed.
    /// </summary>
    public class HedgeTradition
    {
        public string Name { get; private set; }
        public string Description { get; private set; }

        /// <summary>
        /// Non-magical ability bonuses applied to the new HermeticMagus upon
        /// successful gauntlet into the Order. These represent skills accumulated
        /// during life in the hedge tradition that translate directly — language
        /// fluency, area lore, organisation lore, etc.
        /// Magical capability translation is handled separately by GiftOpeningService.
        /// </summary>
        public IReadOnlyList<(Ability Ability, double Experience)> ConversionBonuses { get; private set; }

        public HedgeTradition(string name, string description)
        {
            Name = name;
            Description = description;
            ConversionBonuses = new List<(Ability, double)>();
        }

        public HedgeTradition(
            string name,
            string description,
            IEnumerable<(Ability Ability, double Experience)> conversionBonuses)
        {
            Name = name;
            Description = description;
            ConversionBonuses = new List<(Ability, double)>(conversionBonuses);
        }
    }
}