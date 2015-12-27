﻿using System.Collections.Generic;

namespace WizardMonks.Decisions.Goals
{
    public interface IGoal
    {
        uint? DueDate { get; }
        double Desire { get; set; }
        List<ACondition> Conditions { get; }

        void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log);
        bool IsComplete();
        // TODO: add a boolean to Goals to cache completeness
        // probably should not cache in reversable cases, i.e. labs
    }
}
