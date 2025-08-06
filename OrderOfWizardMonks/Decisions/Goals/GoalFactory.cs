using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Models.Spells;

namespace WizardMonks.Decisions.Goals
{
    public enum GoalType
    {
        FoundCovenant,
        FindApprentice,
        BindFamiliar,
        BuildLab,
        Ability,
        InventSpell
    }

    class GoalFactory
    {
        public static IGoal GenerateGoal(Character character, GoalType goalType, double desire)
        {
            switch (goalType)
            {
                default:
                    return null;
            }
        }

        public static IGoal GenerateSpellGoal(Spell spell, double desire)
        {
            //return new InventSpellGoal(spell, desire, 0);
            return null;
        }
    }
}
