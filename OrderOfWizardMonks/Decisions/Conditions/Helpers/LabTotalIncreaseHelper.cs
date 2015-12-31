using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks.Decisions.Conditions.Helpers
{
    public class LabTotalIncreaseHelper : ArtIncreaseHelper
    {
        public LabTotalIncreaseHelper(Magus mage, uint ageToCompleteBy, double desirePerPoint, ushort conditionDepth, ArtPair arts, bool allowVimVisUse) :
            base(mage, ageToCompleteBy, desirePerPoint, conditionDepth, arts, allowVimVisUse)
        { }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log)
        {
            base.AddActionPreferencesToList(alreadyConsidered, log);
            // increase Magic Theory via practice
            // increase Magic Theory via reading
            // increase Int
            // improve lab
            // find better aura
            if (AgeToCompleteBy - Mage.SeasonalAge > 1)
            {
                FindNewAuraHelper auraHelper = new FindNewAuraHelper(Mage, AgeToCompleteBy - 1, Desire, (ushort)(ConditionDepth + 1), AllowVimVisUse);
            }
        }
    }
}
