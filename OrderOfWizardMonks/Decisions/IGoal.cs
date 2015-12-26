using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks.Decisions
{
    public interface IGoal
    {
        uint? DueDate { get; }
        double Desire { get; set; }
        List<ACondition> Conditions { get; set; }

        void ModifyActionList(Character character, ConsideredActions alreadyConsidered, IList<string> log);
        bool IsComplete(Character character);
        // TODO: add a boolean to Goals to cache completeness
        // probably should not cache in reversable cases, i.e. labs
        IList<BookDesire> GetBookNeeds(Character character);
    }
}
