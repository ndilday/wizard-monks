using System;
using System.Collections.Generic;
using System.Linq;

using WizardMonks;

namespace WizardMonks.Instances
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
        public static IGoal GenerateGoal(GoalType goalType, double desire)
        {
            switch (goalType)
            {
                case GoalType.FoundCovenant:
                    return new HasCovenantCondition(desire);
                case GoalType.BuildLab:
                    return new HasLabCondition(desire);
                default:
                    return null;
            }
        }

        public static IGoal GenerateSpellGoal(Spell spell, double desire)
        {
            return new InventSpellGoal(spell, desire);
        }
    }
}
