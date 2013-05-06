using System;
using System.Collections.Generic;

using WizardMonks;
using WizardMonks.Instances;


namespace WorldSimulation
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
            character.Stamina.BaseValue = NormalStatRoller.RandomStat();
            character.Strength.BaseValue = NormalStatRoller.RandomStat();
            character.Dexterity.BaseValue = NormalStatRoller.RandomStat();
            character.Quickness.BaseValue = NormalStatRoller.RandomStat();
            character.Intelligence.BaseValue = NormalStatRoller.RandomStat();
            character.Perception.BaseValue = NormalStatRoller.RandomStat();
            character.Presence.BaseValue = NormalStatRoller.RandomStat();
            character.Communication.BaseValue = NormalStatRoller.RandomStat();
        }

        public static Magus GenerateNewMagus(Ability magicAbility, Ability langAbility, Ability writingAbility, Ability areaAbility)
        {
            Dictionary<Preference, double> preferences = PreferenceFactory.CreateMagusPreferenceList(magicAbility, langAbility, writingAbility, areaAbility);
            Magus magus = new Magus(magicAbility, langAbility, writingAbility, areaAbility, preferences);
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
                dictionary[new Preference(PreferenceType.Art, art)] = artDesire;
                dictionary[new Preference(PreferenceType.Writing, art)] = (artDesire + writingDesire) / 2;
            }
            dictionary[new Preference(PreferenceType.Vis, null)] = Die.Instance.RollNormal(6.5, 0.1);
            dictionary[new Preference(PreferenceType.Ability, magicAbility)] = Die.Instance.RollDouble();
            dictionary[new Preference(PreferenceType.Ability, writingAbility)] = Die.Instance.RollDouble();
            dictionary[new Preference(PreferenceType.Ability, areaAbility)] = Die.Instance.RollDouble();

            return dictionary;
        }
    }
}
