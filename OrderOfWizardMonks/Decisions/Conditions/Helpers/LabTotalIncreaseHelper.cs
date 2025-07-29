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
            PracticeHelper practiceHelper = new(Abilities.MagicTheory, _mage, _ageToCompleteBy - 1, (ushort)(_conditionDepth + 1), _desireFunc);
            practiceHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);
            // increase Magic Theory via reading
            ReadingHelper readingHelper = new(Abilities.MagicTheory, _mage, _ageToCompleteBy - 1, (ushort)(_conditionDepth + 1), _desireFunc);
            readingHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);
            // increase Int
            // improve lab
            // find better aura.
            if (_ageToCompleteBy - 1 > _mage.SeasonalAge)
            {
                // a season to find the aura, and a season to build a lab in it. Doesn't take into account lab specialization
                FindNewAuraHelper auraHelper = new(_mage, _ageToCompleteBy - 2, (ushort)(_conditionDepth + 2), _desireFunc);
                auraHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);
            }
            // find apprentice
            if(_ageToCompleteBy - 2 > _mage.SeasonalAge && _mage.Apprentice == null)
            {
                FindApprenticeHelper apprenticeHelper = new(_mage, _ageToCompleteBy - 3, (ushort)(_conditionDepth + 3), _desireFunc);
                apprenticeHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);
            }

        }
    }
}
