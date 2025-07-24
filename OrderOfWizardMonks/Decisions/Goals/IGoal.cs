using System.Collections.Generic;
using WizardMonks.Decisions.Conditions;
using WizardMonks.Economy;

namespace WizardMonks.Decisions.Goals
{
    public interface IGoal
    {
        uint? AgeToCompleteBy { get; }
        double Desire { get; set; }
        List<ACondition> Conditions { get; }

        void AddActionPreferencesToList(ConsideredActions alreadyConsidered, Desires desires, IList<string> log);
        bool IsComplete();
        // TODO: add a boolean to Goals to cache completeness
        // probably should not cache in reversable cases, i.e. labs
    }
}
