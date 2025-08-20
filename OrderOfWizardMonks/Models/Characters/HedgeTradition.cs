using System.Collections.Generic;
using WizardMonks.Models.Research;

namespace WizardMonks.Models.Characters
{
    public class HedgeTradition
    {
        public string Name { get; private set; }
        public string Description { get; private set; }

        // A list of research projects Bonisagus can undertake by studying this tradition.
        public List<BreakthroughDefinition> PotentialBreakthroughs { get; private set; }

        // Bonuses the HedgeMagus receives upon successfully being gauntleted into the Order.
        public List<(Ability Ability, double Experience)> ConversionBonuses { get; private set; }

        public HedgeTradition(string name, string description)
        {
            Name = name;
            Description = description;
            PotentialBreakthroughs = new List<BreakthroughDefinition>();
            ConversionBonuses = new List<(Ability, double)>();
        }
    }
}