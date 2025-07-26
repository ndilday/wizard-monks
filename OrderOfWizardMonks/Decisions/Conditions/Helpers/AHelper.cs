using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks.Decisions.Conditions.Helpers
{

    public delegate double CalculateDesireFunc(double gain, ushort conditionDepth);
    public abstract class AHelper
    {
        protected CalculateDesireFunc _desireFunc;

        protected Magus _mage;
        protected uint _ageToCompleteBy;
        protected ushort _conditionDepth;

        public AHelper(Magus mage, uint ageToCompleteBy, ushort conditionDepth, CalculateDesireFunc desireFunc = null)
        {
            _mage = mage;
            _ageToCompleteBy = ageToCompleteBy;
            _conditionDepth = conditionDepth;
            _desireFunc = desireFunc;
        }

        public abstract void AddActionPreferencesToList(ConsideredActions alreadyConsidered, Desires desires, IList<string> log);
    }
}
