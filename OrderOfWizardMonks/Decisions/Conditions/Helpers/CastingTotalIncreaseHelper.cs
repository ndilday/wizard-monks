using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks.Decisions.Conditions.Helpers
{
    class CastingTotalIncreaseHelper :ArtIncreaseHelper
    {
        public CastingTotalIncreaseHelper(Magus mage, uint? ageToCompleteBy, double desirePerPoint, ushort conditionDepth, ArtPair arts, bool allowVimVisUse, CalculateDesireFunc desireFunc) :
            base(mage, ageToCompleteBy, desirePerPoint, conditionDepth, arts, allowVimVisUse, desireFunc)
        { }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log)
        {
            base.AddActionPreferencesToList(alreadyConsidered, log);
            // increase Sta
        }
    }
}
