using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks.Decisions.Goals
{
    class AbilityScoreGoal : AGoal
    {
        AbilityScoreGoal(Character character, uint? dueDate, double desire, Ability ability, double level) :
            base(character, dueDate, desire)
        {
            Conditions.Add(new AbilityScoreCondition(character, ability, level));
        }


    }
}
