using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Core;
using WizardMonks.Models.Characters;

namespace WizardMonks.Instances
{
    public class CharacterFactory
    {
        public static Character GenerateNewCharacter(Ability langAbility, Ability writingAbility, Ability areaAbility)
        {
            Character character = new(langAbility, writingAbility, areaAbility);
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

        private static void NormalizeAttributes(Character character, double value)
        {
            double[] doubles =
            {
                Die.Instance.RollNormal(),
                Die.Instance.RollNormal(),
                Die.Instance.RollNormal(),
                Die.Instance.RollNormal(),
                Die.Instance.RollNormal(),
                Die.Instance.RollNormal(),
                Die.Instance.RollNormal(),
                Die.Instance.RollNormal()
            };
            double sum = doubles.Sum();
            double scale = value / sum;
            character.GetAttribute(AttributeType.Stamina).BaseValue = doubles[0] * scale;
            character.GetAttribute(AttributeType.Strength).BaseValue = doubles[1] * scale;
            character.GetAttribute(AttributeType.Dexterity).BaseValue = doubles[2] * scale;
            character.GetAttribute(AttributeType.Quickness).BaseValue = doubles[3] * scale;
            character.GetAttribute(AttributeType.Intelligence).BaseValue = doubles[4] * scale;
            character.GetAttribute(AttributeType.Perception).BaseValue = doubles[5] * scale;
            character.GetAttribute(AttributeType.Presence).BaseValue = doubles[6] * scale;
            character.GetAttribute(AttributeType.Communication).BaseValue = doubles[7] * scale;
        }

        public static Magus GenerateNewMagus(Ability magicAbility, Ability langAbility, Ability writingAbility, Ability areaAbility)
        {
            Magus magus = new(magicAbility, langAbility, writingAbility, areaAbility);
            NormalizeAttributes(magus);
            return magus;
        }

        public static Magus GenerateNewHedgeMage()
        {
            return null;
        }

        public static Magus GenerateNewApprentice(int bonusPoints)
        {
            Magus magus = new(Abilities.MagicTheory, Abilities.Latin, Abilities.ArtesLiberales, Abilities.AreaLore);
            NormalizeAttributes(magus);
            magus.GetAbility(Abilities.English).AddExperience(75);
            // TODO: Implement a full point-buy system for apprentice generation.
            // For now, as a placeholder, we will convert bonus points directly into
            // bonus Intelligence, as it's the most impactful stat for a future lab assistant.
            // Each 5 bonus points grants +1 Intelligence.
            if (bonusPoints > 0)
            {
                double intelligenceBonus = Math.Floor(bonusPoints / 5.0);
                if (intelligenceBonus > 0)
                {
                    magus.GetAttribute(AttributeType.Intelligence).BaseValue += intelligenceBonus;
                    magus.Log.Add($"Generated with +{intelligenceBonus} Intelligence due to master's skilled search.");
                }
            }
            // randomly assign 45 points to childhood skills in 5 point blocks
            // Area Lore, Athletics, Awareness, Brawl, Charm, Folk Ken, Guile, Stealth, Survival, Swim
            double experienceBlock = 5.0;
            CharacterAbilityBase charAbility = null;
            for (byte i = 0; i < 9; i++)
            {
                switch (Die.Instance.RollSimpleDie())
                {
                    case 1:
                        charAbility = magus.GetAbility(Abilities.AreaLore);
                        break;
                    case 2:
                        charAbility = magus.GetAbility(Abilities.Athletics);
                        break;
                    case 3:
                        charAbility = magus.GetAbility(Abilities.Awareness);
                        break;
                    case 4:
                        charAbility = magus.GetAbility(Abilities.Brawl);
                        break;
                    case 5:
                        charAbility = magus.GetAbility(Abilities.Charm);
                        break;
                    case 6:
                        charAbility = magus.GetAbility(Abilities.FolkKen);
                        break;
                    case 7:
                        charAbility = magus.GetAbility(Abilities.Guile);
                        break;
                    case 8:
                        charAbility = magus.GetAbility(Abilities.Stealth);
                        break;
                    case 9:
                        charAbility = magus.GetAbility(Abilities.Survival);
                        break;
                    case 10:
                        charAbility = magus.GetAbility(Abilities.Swim);
                        break;
                }
                charAbility.AddExperience(experienceBlock);
            }
            // figure out how much older than 5 the child is
            ushort age = (ushort)(20 + Die.Instance.RollDouble() * 80);

            // add experience for the additional time.
            int extraXP = (age - 20) * 4;
            // for now, lets say 10% chance of academics, 20% of martial
            bool isAcademic = Die.Instance.RollSimpleDie() == 1;
            bool isMartial = Die.Instance.RollSimpleDie() <= 2;
            DistributeExperience(magus, extraXP, isAcademic, isMartial);

            // TODO: how do we initialize the goals of this new apprentice?
            InitializeApprenticeGoals(magus);

            return magus;
        }

        private static void DistributeExperience(Magus mage, int extraXP, bool isAcademic, bool isMartial)
        {
            var abilities = Abilities.GetEnumerator()
                                     .Where(a => a.AbilityType == AbilityType.General || 
                                           (isAcademic && a.AbilityType == AbilityType.Academic) ||
                                           (isMartial && a.AbilityType == AbilityType.Martial));
            int abilityCount = abilities.Count();
            while (extraXP > 0)
            {
                int valueAdd = extraXP >= 5 ? 5 : extraXP;
                int roll = (int)(Die.Instance.RollDouble() * abilityCount);
                Ability ability = abilities.ElementAt(roll);
                if (ability.AbilityType == AbilityType.Academic)
                {
                    if (mage.GetAbility(Abilities.ArtesLiberales).Value < 1)
                    {
                        mage.GetAbility(Abilities.ArtesLiberales).AddExperience(valueAdd);
                    }
                    else if (mage.GetAbility(Abilities.Latin).Value < 3)
                    {
                        mage.GetAbility(Abilities.Latin).AddExperience(valueAdd);
                    }
                    else
                    {
                        mage.GetAbility(ability).AddExperience(valueAdd);
                    }
                }
                else
                {
                    mage.GetAbility(ability).AddExperience(valueAdd);
                }
                extraXP -= valueAdd;
            }
        }

        private static void InitializeApprenticeGoals(Magus magus)
        {
        }
    }
}
