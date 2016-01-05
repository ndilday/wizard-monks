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

        public Magus Mage { get; private set; }
        public uint AgeToCompleteBy { get; set; }
        public double Desire { get; set; }
        public ushort ConditionDepth { get; protected set; }

        public AHelper(Magus mage, uint ageToCompleteBy, double desire, ushort conditionDepth, CalculateDesireFunc desireFunc = null)
        {
            Mage = mage;
            AgeToCompleteBy = ageToCompleteBy;
            Desire = desire;
            ConditionDepth = conditionDepth;
            _desireFunc = desireFunc;
        }

        public abstract void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log);
    }
}
