using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks.Instances
{
    public static class Abilities
    {
        public static Ability English { get; private set; }
        public static Ability Latin { get; private set; }
        public static Ability MagicTheory { get; private set; }
        public static Ability ArtesLiberales { get; private set; }
        public static Ability ParmaMagica { get; private set; }
        public static Ability Penetration { get; private set; }
        public static Ability Etiquette { get; private set; }
        public static Ability AreaLore { get; private set; }

        static Abilities()
        {
            English = new Ability(100, AbilityType.Language, "English");
            Latin = new Ability(101, AbilityType.Language, "Latin");
            
            ArtesLiberales = new Ability(200, AbilityType.Academic, "Artes Liberales");
            
            MagicTheory = new Ability(250, AbilityType.Arcane, "Magic Theory");
            ParmaMagica = new Ability(251, AbilityType.Arcane, "Parma Magica");
            Penetration = new Ability(252, AbilityType.Arcane, "Penetration");

            Etiquette = new Ability(0, AbilityType.General, "Etiquette");
            AreaLore = new Ability(1, AbilityType.General, "Area Lore");
        }

        public static IEnumerable<Ability> GetEnumerator()
        {
            yield return English;
            yield return Latin;
            yield return MagicTheory;
            yield return ArtesLiberales;
            yield return ParmaMagica;
            yield return Penetration;
            yield return Etiquette;
            yield return AreaLore;
        }
    }
}
