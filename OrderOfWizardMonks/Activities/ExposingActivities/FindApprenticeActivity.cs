using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WizardMonks.Core;
using WizardMonks.Instances;

namespace WizardMonks.Activities.ExposingActivities
{
    [Serializable]
    public class FindApprentice : AExposingActivity
    {
        public FindApprentice(Ability exposureAbility, double desire) : base(exposureAbility, desire)
        {
            Action = Activity.FindApprentice;
        }

        protected override void DoAction(Character character)
        {
            // TODO: see if the character can safely spont gift-finding spells
            if (typeof(Magus) == character.GetType())
            {
                MageApprenticeSearch((Magus)character);
            }
            else
            {
                CharacterApprenticeSearch(character);
            }
        }

        private void CharacterApprenticeSearch(Character character)
        {
            // TODO: eventually characters will be able to use magical items to do the search
            // making them work similar to the mage
        }

        private void MageApprenticeSearch(Magus mage)
        {
            // add bonus to area lore equal to casting total div 5?
            double folkKen = mage.GetAbility(Abilities.FolkKen).Value;
            folkKen += mage.GetAttribute(AttributeType.Perception).Value;
            Spell bestApprenticeSearchSpell =
                mage.GetBestSpell(SpellBases.GetSpellBaseForEffect(TechniqueEffects.Detect, FormEffects.Gift));
            // add 1 per magnitude of detection spell to the total
            if (bestApprenticeSearchSpell != null)
            {
                folkKen += bestApprenticeSearchSpell.Level / 5.0;
            }
            else
            {
                folkKen += mage.GetSpontaneousCastingTotal(MagicArtPairs.InVi) / 5.0;
            }
            double roll = Die.Instance.RollExplodingDie() + folkKen;
            if (roll > 12)
            {
                mage.Log.Add("Apprentice found");
                mage.TakeApprentice(CharacterFactory.GenerateNewApprentice());
                mage.Apprentice.Name = "Apprentice filius " + mage.Name;
            }
            // TODO: gradual reduction in chance?
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
