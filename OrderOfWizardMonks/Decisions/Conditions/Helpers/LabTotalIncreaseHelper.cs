﻿using System.Collections.Generic;

using WizardMonks.Instances;

namespace WizardMonks.Decisions.Conditions.Helpers
{
    public class LabTotalIncreaseHelper : ArtIncreaseHelper
    {
        public LabTotalIncreaseHelper(Magus mage, uint ageToCompleteBy, double desirePerPoint, ushort conditionDepth, ArtPair arts, CalculateDesireFunc desireFunc) :
            base(mage, ageToCompleteBy, desirePerPoint, conditionDepth, arts, desireFunc)
        { }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log)
        {
            base.AddActionPreferencesToList(alreadyConsidered, log);
            // increase Magic Theory via practice
            PracticeHelper practiceHelper = new(Abilities.MagicTheory, Mage, AgeToCompleteBy - 1, Desire, (ushort)(ConditionDepth + 1), _desireFunc);
            practiceHelper.AddActionPreferencesToList(alreadyConsidered, log);
            // increase Magic Theory via reading
            ReadingHelper readingHelper = new(Abilities.MagicTheory, Mage, AgeToCompleteBy - 1, Desire, (ushort)(ConditionDepth + 1), _desireFunc);
            readingHelper.AddActionPreferencesToList(alreadyConsidered, log);
            // increase Int
            // improve lab
            // find better aura.
            if (AgeToCompleteBy - 1 > Mage.SeasonalAge)
            {
                // a season to find the aura, and a season to build a lab in it. Doesn't take into account lab specialization
                FindNewAuraHelper auraHelper = new(Mage, AgeToCompleteBy - 2, Desire, (ushort)(ConditionDepth + 2), _desireFunc);
            }
        }
    }
}
