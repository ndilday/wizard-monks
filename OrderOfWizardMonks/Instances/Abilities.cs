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
        public static Ability Finesse { get; private set; }
        public static Ability ArtesLiberales { get; private set; }
        public static Ability ParmaMagica { get; private set; }
        public static Ability Penetration { get; private set; }
        public static Ability Etiquette { get; private set; }
        public static Ability AreaLore { get; private set; }
        public static Ability Concentration { get; private set; }
        public static Ability Teaching { get; private set; }
        public static Ability Scribing { get; private set; }
        public static Ability Warping { get; private set; }

        static Abilities()
        {
            English = new Ability(100, AbilityType.Language, "English");
            Latin = new Ability(101, AbilityType.Language, "Latin");
            
            ArtesLiberales = new Ability(200, AbilityType.Academic, "Artes Liberales");
            
            MagicTheory = new Ability(250, AbilityType.Arcane, "Magic Theory");
            ParmaMagica = new Ability(251, AbilityType.Arcane, "Parma Magica");
            Penetration = new Ability(252, AbilityType.Arcane, "Penetration");
            Finesse = new Ability(253, AbilityType.Arcane, "Finesse");

            Etiquette = new Ability(0, AbilityType.General, "Etiquette");
            AreaLore = new Ability(1, AbilityType.General, "Area Lore");
            Concentration = new Ability(2, AbilityType.General, "Concentration");
            Teaching = new Ability(3, AbilityType.General, "Teaching");
            Scribing = new Ability(4, AbilityType.General, "Scribing");

            Warping = new Ability(-1, AbilityType.Arcane, "Warping");
        }

        public static IEnumerable<Ability> GetEnumerator()
        {
            yield return English;
            yield return Latin;
            yield return MagicTheory;
            yield return ArtesLiberales;
            yield return ParmaMagica;
            yield return Penetration;
            yield return Finesse;
            yield return Etiquette;
            yield return AreaLore;
            yield return Concentration;
            yield return Teaching;
        }
    }
}
