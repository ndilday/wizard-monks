using WizardMonks.Instances;

namespace WizardMonks.Models.Characters
{
    public class HedgeMagus : Character
    {
        public HedgeTradition Tradition { get; private set; }

        public HedgeMagus(HedgeTradition tradition)
            : this(tradition, Abilities.Latin, Abilities.ArtesLiberales) { }

        public HedgeMagus(HedgeTradition tradition, Ability writingLanguage, Ability writingAbility)
            : base(writingLanguage, writingAbility, Abilities.AreaLore) // Sensible defaults
        {
            Tradition = tradition;
        }

        // The critical conversion method
        public Magus BecomeHermeticMagus(HousesEnum joiningHouse)
        {
            var newMagus = new Magus(joiningHouse, this.SeasonalAge, this.Personality, this.ReputationFocuses)
            {
                Name = this.Name,
                // Copy over base attributes, non-magical abilities, etc.
                // This would be a good place for a copy constructor or helper method.
            };

            // Apply conversion bonuses
            foreach (var (ability, experience) in Tradition.ConversionBonuses)
            {
                newMagus.GetAbility(ability).AddExperience(experience);
            }

            // The new magus's Hermetic Theory now contains the potential for their old magic
            // This is how Bonisagus will "see" what he can learn from them.
            // newMagus.HermeticTheory.PotentialBreakthroughs.AddRange(Tradition.PotentialBreakthroughs);

            return newMagus;
        }
    }
}