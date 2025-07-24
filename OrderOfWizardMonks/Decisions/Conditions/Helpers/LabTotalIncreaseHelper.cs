using System.Collections.Generic;

using WizardMonks.Instances;

namespace WizardMonks.Decisions.Conditions.Helpers
{
    public class LabTotalIncreaseHelper : ArtIncreaseHelper
    {
        public LabTotalIncreaseHelper(Magus mage, uint ageToCompleteBy, ushort conditionDepth, ArtPair arts, CalculateDesireFunc desireFunc) :
            base(mage, ageToCompleteBy, conditionDepth, arts, desireFunc)
        { }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, Desires desires, IList<string> log)
        {
            base.AddActionPreferencesToList(alreadyConsidered, desires, log);
            // increase Magic Theory via practice
            PracticeHelper practiceHelper = new(Abilities.MagicTheory, Mage, AgeToCompleteBy - 1, (ushort)(ConditionDepth + 1), _desireFunc);
            practiceHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);
            // increase Magic Theory via reading
            ReadingHelper readingHelper = new(Abilities.MagicTheory, Mage, AgeToCompleteBy - 1, (ushort)(ConditionDepth + 1), _desireFunc);
            readingHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);
            // increase Int
            // improve lab
            // find better aura.
            if (AgeToCompleteBy - 1 > Mage.SeasonalAge)
            {
                // a season to find the aura, and a season to build a lab in it. Doesn't take into account lab specialization
                FindNewAuraHelper auraHelper = new(Mage, AgeToCompleteBy - 2, (ushort)(ConditionDepth + 2), _desireFunc);
            }
        }
    }
}
