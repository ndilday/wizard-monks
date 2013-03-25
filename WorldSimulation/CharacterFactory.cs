using System;

using WizardMonks;

namespace WorldSimulation
{
    class CharacterFactory
    {
        public static Character GenerateNewCharacter()
        {
            Character character = new Character();
            character.Stamina.BaseValue = NormalStatRoller.RandomStat();
            character.Strength.BaseValue = NormalStatRoller.RandomStat();
            character.Dexterity.BaseValue = NormalStatRoller.RandomStat();
            character.Quickness.BaseValue = NormalStatRoller.RandomStat();
            character.Intelligence.BaseValue = NormalStatRoller.RandomStat();
            character.Perception.BaseValue = NormalStatRoller.RandomStat();
            character.Presence.BaseValue = NormalStatRoller.RandomStat();
            character.Communication.BaseValue = NormalStatRoller.RandomStat();

            return character;
        }
    }
}
