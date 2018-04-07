using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks.Decisions.Goals
{
    class TeachApprenticeGoal : AGoal
    {
        private Magus _mage;
        public TeachApprenticeGoal(Magus magus, uint ageToCompleteBy, double desire) :
            base(magus, desire, ageToCompleteBy)
        {
            _mage = magus;
        }
    }
}
