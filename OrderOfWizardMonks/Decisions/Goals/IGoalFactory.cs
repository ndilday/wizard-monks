using WizardMonks.Characters;

namespace WizardMonks.Decisions.Goals
{
    public enum GoalType
    {
        StartCovenant,
        FindCovenant,
        FindApprentice,
        FindFamiliar,
        BindFamiliar,
        Ability,
        InventSpell,
        FoundMysteryCult,
        InventMystery,
        GainPower,
        GainPrestige,
        HermeticBreakthrough,
        EnchantItem
    }

    interface IGoalFactory
    {
        IGoal GenerateGoal(Character character, GoalType goalType, double desire);

        IGoal GenerateSpellGoal(Spell spell, double desire);
    }
}
