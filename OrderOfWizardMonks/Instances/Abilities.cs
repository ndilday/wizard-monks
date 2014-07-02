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
        public static Ability Athletics { get; private set; }
        public static Ability Awareness { get; private set; }
        public static Ability Brawl { get; private set; }
        public static Ability Charm { get; private set; }
        public static Ability FolkKen { get; private set; }
        public static Ability Guile { get; private set; }
        public static Ability MagicLore { get; private set; }
        public static Ability Concentration { get; private set; }
        public static Ability Teaching { get; private set; }
        public static Ability Scribing { get; private set; }
        public static Ability Stealth { get; private set; }
        public static Ability Survival { get; private set; }
        public static Ability Swim { get; private set; }
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
            FolkKen = new Ability(5, AbilityType.General, "Folk Lore");
            MagicLore = new Ability(6, AbilityType.General, "Magic Lore");
            Craft = new Ability(7, AbilityType.General, "Craft");
            Athletics = new Ability(8, AbilityType.General, "Athletics");
            Awareness = new Ability(9, AbilityType.General, "Awareness");
            Brawl = new Ability(10, AbilityType.General, "Brawl");
            Charm = new Ability(11, AbilityType.General, "Charm");
            Guile = new Ability(12, AbilityType.General, "Guile");
            Stealth = new Ability(13, AbilityType.General, "Stealth");
            Survival = new Ability(14, AbilityType.General, "Survival");
            Swim = new Ability(15, AbilityType.General, "Swim");

            Warping = new Ability(-1, AbilityType.Arcane, "Warping");

            EnigmaticWisdom = new Ability(150, AbilityType.Supernatural, "Enigmatic Wisdom");
            CriamonLore = new Ability(50, AbilityType.General, "Criamon Lore");
            Heartbeast = new Ability(151, AbilityType.Supernatural, "Heartbeast");
            BjornaerLore = new Ability(51, AbilityType.General, "Bjornaer Lore");
            MerinitaLore = new Ability(52, AbilityType.General, "Merinita Lore");
            VerditiusLore = new Ability(53, AbilityType.General, "Verditius Lore");
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
            yield return Athletics;
            yield return Awareness;
            yield return Brawl;
            yield return Charm;
            yield return Craft;
            yield return FolkKen;
            yield return Guile;
            yield return MagicLore;
            yield return Stealth;
            yield return Survival;
            yield return Swim;
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
