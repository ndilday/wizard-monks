using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks.Decisions.Goals
{
    public class LongevityRitualGoal : AGoal
    {
        public LongevityRitualGoal(Magus magus, uint? ageToCompleteBy, double desire) : base(magus, ageToCompleteBy, desire){ }
    }
}
