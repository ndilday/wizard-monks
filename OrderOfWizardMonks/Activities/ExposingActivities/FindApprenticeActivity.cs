using System;
using WizardMonks.Core;
using WizardMonks.Instances;
using WizardMonks.Models.Spells;

namespace WizardMonks.Activities.ExposingActivities
{
    [Serializable]
    public class FindApprenticeActivity : AExposingActivity
    {
        private const double EASE_FACTOR = 9.0;

        public FindApprenticeActivity(Ability exposureAbility, double desire) : base(exposureAbility, desire)
        {
            Action = Activity.FindApprentice;
        }

        protected override void DoAction(Character character)
        {
            if (character is not Magus mage)
            {
                // Non-magi cannot search for magically Gifted apprentices at this time.
                character.Log.Add("Cannot search for an apprentice without the Gift.");
                return;
            }

            mage.Log.Add("Searching for a potential apprentice...");

            // Step 1: Calculate the Search Total
            double searchTotal = 0;
            searchTotal += mage.GetAbility(Abilities.FolkKen).Value;
            searchTotal += mage.GetAttributeValue(AttributeType.Perception);
            // Area Lore and Etiquette provide a smaller, supporting bonus.
            searchTotal += mage.GetAbility(Abilities.AreaLore).Value / 2.0;
            searchTotal += mage.GetAbility(Abilities.Etiquette).Value / 2.0;

            // Step 2: Add Magic Bonus
            SpellBase giftFindingBase = SpellBases.GetSpellBaseForEffect(TechniqueEffects.Detect, FormEffects.Gift);
            Spell bestGiftFindingSpell = mage.GetBestSpell(giftFindingBase);
            double giftFindingBonus = 0;

            if (bestGiftFindingSpell != null)
            {
                giftFindingBonus = (bestGiftFindingSpell.Level / 5.0) - 5;
                mage.Log.Add($"Using '{bestGiftFindingSpell.Name}' to aid the search.");
            }
            else
            {
                // If no spell is known, use spontaneous magic potential.
                giftFindingBonus = (mage.GetSpontaneousCastingTotal(MagicArtPairs.InVi) / 5.0) - 5;
            }
            searchTotal += giftFindingBonus;

            // Step 3: Make the Roll and Determine Outcome
            double roll = Die.Instance.RollStressDie(0, out _); // Botch has no special effect for now.

            if (roll == 0) // A botch is treated as a simple failure.
            {
                mage.Log.Add("The search for an apprentice was fruitless this season.");
                return;
            }

            if (roll + searchTotal > EASE_FACTOR)
            {
                // Success! Calculate the margin of success for the bonus points.
                double marginOfSuccess = (roll + searchTotal) - EASE_FACTOR;
                int bonusPoints = (int)Math.Floor(marginOfSuccess);

                mage.Log.Add($"Success! An apprentice has been found with {bonusPoints} bonus points for character generation.");

                Magus newApprentice = CharacterFactory.GenerateNewApprentice(bonusPoints);
                newApprentice.Name = "Apprentice filius " + mage.Name;
                mage.TakeApprentice(newApprentice);
            }
            else
            {
                // Failure
                mage.Log.Add("The search for an apprentice was fruitless this season.");
            }
        }

        public override bool Matches(IActivity action)
        {
            return action.Action == Activity.FindApprentice;
        }

        public override string Log()
        {
            return "Finding apprentice worth " + Desire.ToString("0.000");
        }
    }
}
