using System;
using System.Collections.Generic;

using WizardMonks;

namespace WizardMonks.Instances
{
    public class CharacterFactory
    {
        public static Character GenerateNewCharacter(Ability langAbility, Ability writingAbility, Ability areaAbility)
        {
            Character character = new Character(langAbility, writingAbility, areaAbility);
            NormalizeAttributes(character);

            return character;
        }

        private static void NormalizeAttributes(Character character)
        {
            character.GetAttribute(AttributeType.Stamina).BaseValue = Die.Instance.RollNormal();
            character.GetAttribute(AttributeType.Strength).BaseValue = Die.Instance.RollNormal();
            character.GetAttribute(AttributeType.Dexterity).BaseValue = Die.Instance.RollNormal();
            character.GetAttribute(AttributeType.Quickness).BaseValue = Die.Instance.RollNormal();
            character.GetAttribute(AttributeType.Intelligence).BaseValue = Die.Instance.RollNormal();
            character.GetAttribute(AttributeType.Perception).BaseValue = Die.Instance.RollNormal();
            character.GetAttribute(AttributeType.Presence).BaseValue = Die.Instance.RollNormal();
            character.GetAttribute(AttributeType.Communication).BaseValue = Die.Instance.RollNormal();
        }

        public static Magus GenerateNewMagus(Ability magicAbility, Ability langAbility, Ability writingAbility, Ability areaAbility)
        {
            Magus magus = new Magus(magicAbility, langAbility, writingAbility, areaAbility);
            NormalizeAttributes(magus);
            return magus;
        }

        public static Magus GenerateNewMagus()
        {
            Magus magus = new Magus(Abilities.MagicTheory, Abilities.Latin, Abilities.ArtesLiberales, Abilities.AreaLore);
            NormalizeAttributes(magus);
            return magus;
        }
    }
}
