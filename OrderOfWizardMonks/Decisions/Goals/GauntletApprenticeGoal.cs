using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WizardMonks.Models.Characters;

namespace WizardMonks.Decisions.Goals
{
    class GauntletApprenticeGoal : AGoal
    {
        private HermeticMagus _mage;
        public GauntletApprenticeGoal(HermeticMagus magus, uint ageToCompleteBy, double desire) :
            base(magus, ageToCompleteBy, desire)
        {
            _mage = magus;
        }
    }
}
