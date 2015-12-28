using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks.Decisions.Goals
{
    class ApprenticeGoal : AGoal
    {
        public ApprenticeGoal(Character character, uint? dueDate, double desire) : base(character, dueDate, desire) {}
    }
}
