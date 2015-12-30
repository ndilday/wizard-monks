using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks.Decisions.Conditions.Helpers
{
    class LabTotalIncreaseHelper : AHelper
    {
        private ArtPair _arts;
        private double _currentDesire;
        public LabTotalIncreaseHelper(Magus mage, uint ageToCompleteBy, double desirePerPoint, ushort conditionDepth, ArtPair arts, double currentDesire) :
            base(mage, ageToCompleteBy, desirePerPoint, conditionDepth)
        {
            _arts = arts;
            _currentDesire = currentDesire;
        }
    }
}
