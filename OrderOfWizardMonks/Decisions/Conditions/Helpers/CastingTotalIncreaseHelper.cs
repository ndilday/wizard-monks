using System.Collections.Generic;

using WizardMonks.Characters;

namespace WizardMonks.Decisions.Conditions.Helpers
{
    class CastingTotalIncreaseHelper : ArtIncreaseHelper
    {
        public CastingTotalIncreaseHelper(Magus mage, uint ageToCompleteBy, ushort conditionDepth, ArtPair arts, CalculateDesireFunc desireFunc) :
            base(mage, ageToCompleteBy, conditionDepth, arts, desireFunc)
        { }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log)
        {
            base.AddActionPreferencesToList(alreadyConsidered, log);
            // increase Sta
        }
    }
}
