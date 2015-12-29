using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks.Decisions.Goals
{
    class GauntletApprenticeGoal : AGoal
    {
        private Magus _mage;
        public GauntletApprenticeGoal(Magus magus, uint ageToCompleteBy, double desire) :
            base(magus, ageToCompleteBy, desire)
        {
            _mage = magus;
        }
    }
}
