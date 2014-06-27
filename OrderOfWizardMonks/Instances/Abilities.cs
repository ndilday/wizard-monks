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
        public static Ability FolkLore { get; private set; }
        public static Ability MagicLore { get; private set; }
        public static Ability Concentration { get; private set; }
        public static Ability Teaching { get; private set; }
        public static Ability Scribing { get; private set; }
        public static Ability Warping { get; private set; }
        public static Ability EnigmaticWisdom { get; private set; }
        public static Ability CriamonLore { get; private set; }
        public static Ability Heartbeast { get; private set; }
        public static Ability BjornaerLore { get; private set; }
        public static Ability MerinitaLore { get; private set; }
        public static Ability VerditiusLore { get; private set; }
        public static Ability CodeOfHermes { get; private set; }
        public static Ability Philosophae { get; private set; }
        public static Ability Craft { get; private set; }

        static Abilities()
        {
            English = new Ability(100, AbilityType.Language, "English");
            Latin = new Ability(101, AbilityType.Language, "Latin");
            
            ArtesLiberales = new Ability(200, AbilityType.Academic, "Artes Liberales");
            Philosophae = new Ability(201, AbilityType.Academic, "Philosophae");
            
            MagicTheory = new Ability(250, AbilityType.Arcane, "Magic Theory");
            ParmaMagica = new Ability(251, AbilityType.Arcane, "Parma Magica");
            Penetration = new Ability(252, AbilityType.Arcane, "Penetration");
            Finesse = new Ability(253, AbilityType.Arcane, "Finesse");
            CodeOfHermes = new Ability(254, AbilityType.Arcane, "Code of Hermes");

            Etiquette = new Ability(0, AbilityType.General, "Etiquette");
            AreaLore = new Ability(1, AbilityType.General, "Area Lore");
            Concentration = new Ability(2, AbilityType.General, "Concentration");
            Teaching = new Ability(3, AbilityType.General, "Teaching");
            Scribing = new Ability(4, AbilityType.General, "Scribing");
            FolkLore = new Ability(5, AbilityType.General, "Folk Lore");
            MagicLore = new Ability(6, AbilityType.General, "Magic Lore");
            Craft = new Ability(7, AbilityType.General, "Craft");

            Warping = new Ability(-1, AbilityType.Arcane, "Warping");

            EnigmaticWisdom = new Ability(300, AbilityType.Mystery, "Enigmatic Wisdom");
            CriamonLore = new Ability(150, AbilityType.General, "Criamon Lore");
            Heartbeast = new Ability(301, AbilityType.Mystery, "Heartbeast");
            BjornaerLore = new Ability(151, AbilityType.General, "Bjornaer Lore");
            MerinitaLore = new Ability(152, AbilityType.General, "Merinita Lore");
            VerditiusLore = new Ability(153, AbilityType.General, "Verditius Lore");
        }

        public static IEnumerable<Ability> GetEnumerator()
        {
            yield return English;
            yield return Latin;
            yield return ArtesLiberales;
            yield return Philosophae;
            yield return MagicTheory;
            yield return ParmaMagica;
            yield return Penetration;
            yield return Finesse;
            yield return CodeOfHermes;
            yield return Etiquette;
            yield return AreaLore;
            yield return FolkLore;
            yield return MagicLore;
            yield return Craft;
            yield return Concentration;
            yield return Teaching;
            yield return EnigmaticWisdom;
            yield return Heartbeast;
            yield return CriamonLore;
            yield return BjornaerLore;
            yield return MerinitaLore;
            yield return VerditiusLore;
        }
    }
}
