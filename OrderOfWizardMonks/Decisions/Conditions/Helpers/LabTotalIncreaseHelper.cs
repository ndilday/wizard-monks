using System.Collections.Generic;

using WizardMonks.Instances;

namespace WizardMonks.Decisions.Conditions.Helpers
{
    public class LabTotalIncreaseHelper : ArtIncreaseHelper
    {
        public LabTotalIncreaseHelper(Magus mage, uint ageToCompleteBy, double desirePerPoint, ushort conditionDepth, ArtPair arts, bool allowVimVisUse, CalculateDesireFunc desireFunc) :
            base(mage, ageToCompleteBy, desirePerPoint, conditionDepth, arts, allowVimVisUse, desireFunc)
        { }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log)
        {
            base.AddActionPreferencesToList(alreadyConsidered, log);
            // increase Magic Theory via practice
            PracticeHelper practiceHelper = new PracticeHelper(Abilities.MagicTheory, Mage, AgeToCompleteBy, Desire, (ushort)(ConditionDepth + 1), _desireFunc);
            practiceHelper.AddActionPreferencesToList(alreadyConsidered, log);
            // increase Magic Theory via reading
            ReadingHelper readingHelper = new ReadingHelper(Abilities.MagicTheory, Mage, AgeToCompleteBy, Desire, (ushort)(ConditionDepth + 1), _desireFunc);
            readingHelper.AddActionPreferencesToList(alreadyConsidered, log);
            // increase Int
            // improve lab
            // find better aura
            if (AgeToCompleteBy - Mage.SeasonalAge > 2)
            {
                // a season to find the aura, and a season to build a lab in it. Doesn't take into account lab specialization
                FindNewAuraHelper auraHelper = new FindNewAuraHelper(Mage, AgeToCompleteBy - 1, Desire, (ushort)(ConditionDepth + 2), AllowVimVisUse, _desireFunc);
            }
        }
    }
}
