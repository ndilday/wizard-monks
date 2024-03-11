using WizardMonks.Characters;

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

    interface IGoalFactory
    {
        IGoal GenerateGoal(Character character, GoalType goalType, double desire);

        IGoal GenerateSpellGoal(Spell spell, double desire);
    }
}
