
using System;
using WizardMonks.Core;
using WizardMonks.Models.Characters;

namespace WizardMonks.Services.Characters
{
    public static class CharacterAgingService
    {
        public static void ApplyLongevityRitual(this Character character, ushort strength)
        {
            character.LongevityRitual = strength;
        }

        public static void Age(this Character character, ushort modifiers)
        {
            // roll exploding die for aging
            if (character.LongevityRitual > 0)
            {
                character.Warping.AddExperience(0.25);
            }
            bool apparent = true;
            bool crisis = false;
            bool died = false;
            ushort agingRoll = Die.Instance.RollExplodingDie();
            agingRoll -= modifiers;
            ushort ageModifier = (ushort)Math.Ceiling(character.SeasonalAge / 40.0m);
            agingRoll += ageModifier;

            if (agingRoll < 3)
            {
                character.NoAgingSeasons++;
                apparent = false;
            }

            if (agingRoll == 13 || agingRoll > 21)
            {
                crisis = true;
                character.LongevityRitual = 0;
                character.IncreaseDecrepitudeToNextLevel();
                int crisisRoll = Die.Instance.RollSimpleDie();
                crisisRoll = crisisRoll + ageModifier + character.GetDecrepitudeScore();
                if (crisisRoll > 14)
                {
                    int staDiff = 3 * (crisisRoll - 14);
                    if (character.GetAttribute(AttributeType.Stamina).Value + Die.Instance.RollSimpleDie() < staDiff)
                    {
                        died = true;
                        character.Decrepitude = 75;
                    }
                }
            }
            else if (agingRoll > 9)
            {
                character.Decrepitude++;
            }

            if (character.Decrepitude > 74)
            {
                died = true;
            }

            AgingEventArgs args = new(character, crisis, apparent, died);
            character.OnAged(args);
        }

        private static void IncreaseDecrepitudeToNextLevel(this Character character)
        {
            // TODO: decrepitude points need to go to attributes
            // TODO: add configuration option to choose between different methods of distributing decrepitude points
            if (character.Decrepitude < 5)
            {
                character.Decrepitude = 5;
            }
            else if (character.Decrepitude < 15)
            {
                character.Decrepitude = 15;
            }
            else if (character.Decrepitude < 30)
            {
                character.Decrepitude = 30;
            }
            else if (character.Decrepitude < 50)
            {
                character.Decrepitude = 50;
            }
            else if (character.Decrepitude < 75)
            {
                character.Decrepitude = 75;
            }
        }

        private static byte GetDecrepitudeScore(this Character character)
        {
            if (character.Decrepitude < 5) return 0;
            if (character.Decrepitude < 15) return 1;
            if (character.Decrepitude < 30) return 2;
            if (character.Decrepitude < 50) return 3;
            if (character.Decrepitude < 75) return 4;
            return 5;
        }
    }
}
