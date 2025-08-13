using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WizardMonks.Models.Characters;

namespace WizardMonks.Decisions.Goals
{
    class TeachApprenticeGoal : AGoal
    {
        private Magus _mage;
        public TeachApprenticeGoal(Magus magus, uint ageToCompleteBy, double desire) :
            base(magus, ageToCompleteBy, desire)
        {
            _mage = magus;
        }
    }
}
