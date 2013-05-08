using System;
using System.Collections.Generic;

using WizardMonks;
using WizardMonks.Instances;


namespace WizardMonks.Instances
{
    public class CharacterFactory
    {
        public static Character GenerateNewCharacter(Ability langAbility, Ability writingAbility, Ability areaAbility)
        {
            Character character = new Character(langAbility, writingAbility, areaAbility, null);
            NormalizeAttributes(character);

            return character;
        }

        private static void NormalizeAttributes(Character character)
        {
            character.Stamina.BaseValue = Die.Instance.RollNormal();
            character.Strength.BaseValue = Die.Instance.RollNormal();
            character.Dexterity.BaseValue = Die.Instance.RollNormal();
            character.Quickness.BaseValue = Die.Instance.RollNormal();
            character.Intelligence.BaseValue = Die.Instance.RollNormal();
            character.Perception.BaseValue = Die.Instance.RollNormal();
            character.Presence.BaseValue = Die.Instance.RollNormal();
            character.Communication.BaseValue = Die.Instance.RollNormal();
        }

        public static Magus GenerateNewMagus(Ability magicAbility, Ability langAbility, Ability writingAbility, Ability areaAbility)
        {
            Dictionary<Preference, double> preferences = PreferenceFactory.CreateMagusPreferenceList(magicAbility, langAbility, writingAbility, areaAbility);
            Magus magus = new Magus(magicAbility, langAbility, writingAbility, areaAbility, preferences);
            NormalizeAttributes(magus);
            return magus;
        }

        public static Magus GenerateNewMagus()
        {
            Dictionary<Preference, double> preferences = 
                PreferenceFactory.CreateMagusPreferenceList(Abilities.MagicTheory, Abilities.Latin, Abilities.ArtesLiberales, Abilities.AreaLore);
            Magus magus = new Magus(Abilities.MagicTheory, Abilities.Latin, Abilities.ArtesLiberales, Abilities.AreaLore, preferences);
            NormalizeAttributes(magus);
            return magus;
        }
    }

    public static class PreferenceFactory
    {
        public static Dictionary<Preference, double> CreateMagusPreferenceList(Ability magicAbility, Ability langAbility, Ability writingAbility, Ability areaAbility)
        {
            Dictionary<Preference, double> dictionary = new Dictionary<Preference, double>();
            dictionary[new Preference(PreferenceType.AgeToApprentice, null)] = Die.Instance.RollDouble() * 150;
            double writingDesire = Die.Instance.RollDouble();
            foreach (Ability art in MagicArts.GetEnumerator())
            {
                double artDesire = Die.Instance.RollDouble();
                dictionary[new Preference(PreferenceType.Ability, art)] = artDesire;
                dictionary[new Preference(PreferenceType.Writing, art)] = (artDesire + writingDesire) / 2;
            }
            dictionary[Preferences.VisDesire] = Die.Instance.RollNormal(6.5, 0.1);
            dictionary[new Preference(PreferenceType.Ability, magicAbility)] = Die.Instance.RollDouble();
            dictionary[new Preference(PreferenceType.Ability, writingAbility)] = Die.Instance.RollDouble();
            dictionary[new Preference(PreferenceType.Ability, areaAbility)] = Die.Instance.RollDouble();

            return dictionary;
        }
    }
}
