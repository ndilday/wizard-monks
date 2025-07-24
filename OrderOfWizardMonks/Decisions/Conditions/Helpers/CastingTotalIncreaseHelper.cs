using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks.Decisions.Conditions.Helpers
{
    class CastingTotalIncreaseHelper : ArtIncreaseHelper
    {
        public CastingTotalIncreaseHelper(Magus mage, uint ageToCompleteBy, ushort conditionDepth, ArtPair arts, CalculateDesireFunc desireFunc) :
            base(mage, ageToCompleteBy, conditionDepth, arts, desireFunc)
        { }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, Desires desires, IList<string> log)
        {
            base.AddActionPreferencesToList(alreadyConsidered, desires, log);
            // increase Sta
        }
    }
}
