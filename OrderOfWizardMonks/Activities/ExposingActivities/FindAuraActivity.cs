using System;
using System.Linq;
using WizardMonks.Core;
using WizardMonks.Instances;
using WizardMonks.Models;
using WizardMonks.Models.Beliefs;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Spells;
using WizardMonks.Services.Characters;

namespace WizardMonks.Activities.ExposingActivities
{
    [Serializable]
    public class FindAuraActivity : AExposingActivity
    {
        public FindAuraActivity(Ability exposureAbility, double desire)
            : base(exposureAbility, desire)
        {
            Action = Activity.FindAura;
        }

        protected override void DoAction(Character character)
        {
            character.Log.Add("Searched for an aura");
            // see if the character can safely spont aura-finding spells
            if (typeof(Magus) == character.GetType())
            {
                MageAuraSearch((Magus)character);
            }
            else
            {
                CharacterAuraSearch(character);
            }
            // TODO: store knowledge of locations
            // TODO: as we go, eventually, do we want locations to be set, rather than generated upon finding?
        }

        private void CharacterAuraSearch(Character character)
        {
            // TODO: eventually characters will be able to use magical items to do the search
            // making them work similar to the mage
        }

        private void MageAuraSearch(Magus mage)
        {
            double areaLore = CalculateSearchScore(mage);

            double roll = Die.Instance.RollDouble() * 5;

            // die roll will be 0-5; area lore will be between 0 and 15, giving auras between 0 and 9
            double auraFound = Math.Sqrt(roll * areaLore / (mage.GetOwnedAuras().Count() + 1));
            if (auraFound >= 1)
            {
                Aura aura = new(Domain.Magic, auraFound);
                mage.Log.Add("Found an aura of strength " + auraFound.ToString("0.000"));
                BeliefProfile auraBelief = new(SubjectType.Aura, 1.0);
                auraBelief.AddOrUpdateBelief(new(BeliefTopics.Strength, aura.Strength));
                mage.AddOrUpdateKnowledge(aura, auraBelief);
                if (mage.Covenant == null || mage.Laboratory == null && mage.Covenant.Aura.Strength < aura.Strength)
                {
                    mage.FoundCovenant(aura);
                }
            }
        }

        private static double CalculateSearchScore(Magus mage)
        {
            // add bonus to area lore equal to casting total div 10?
            double areaLore = mage.GetAbility(Abilities.AreaLore).Value;
            areaLore += mage.GetAttribute(AttributeType.Perception).Value;

            Spell bestAuraSearchSpell =
                mage.GetBestSpell(SpellBases.GetSpellBaseForEffect(TechniqueEffects.Detect, FormEffects.Aura));
            // add 1 per magnitude of detection spell to the total
            if (bestAuraSearchSpell != null)
            {
                areaLore += bestAuraSearchSpell.Level / 5.0;
            }
            else
            {
                areaLore += mage.GetSpontaneousCastingTotal(MagicArtPairs.InVi) / 5.0;
            }

            return areaLore;
        }

        public override bool Matches(IActivity action)
        {
            return action.Action == Activity.FindAura;
        }

        public override string Log()
        {
            return "Finding a new aura worth " + Desire.ToString("0.000");
        }
    }

}
