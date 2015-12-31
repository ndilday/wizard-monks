using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks.Decisions.Conditions.Helpers
{
    public abstract class AHelper
    {
        public Magus Mage { get; private set; }
        public uint AgeToCompleteBy { get; set; }
        public double Desire { get; set; }
        public ushort ConditionDepth { get; protected set; }

        public AHelper(Magus mage, uint ageToCompleteBy, double desire, ushort conditionDepth)
        {
            Mage = mage;
            AgeToCompleteBy = ageToCompleteBy;
            Desire = desire;
            ConditionDepth = conditionDepth;
        }

        public abstract void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log);
    }
}
