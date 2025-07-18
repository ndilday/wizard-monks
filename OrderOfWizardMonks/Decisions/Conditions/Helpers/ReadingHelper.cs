using System.Collections.Generic;
using System.Linq;
using WizardMonks.Activities;
using WizardMonks.Instances;

namespace WizardMonks.Decisions.Conditions.Helpers
{
    class ReadingHelper : AHelper
    {
        private Ability _ability;
        public ReadingHelper(Ability ability, Magus mage, uint ageToCompleteBy, ushort conditionDepth, CalculateDesireFunc desireFunc = null) :
            base(mage, ageToCompleteBy, conditionDepth, desireFunc)
        {
            _ability = ability;
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log)
        {
            var bestBook = Mage.GetBestBookToRead(_ability);
            if (bestBook != null)
            {
                double gain = Mage.GetBookLevelGain(bestBook);
                double effectiveDesire = _desireFunc(gain, ConditionDepth);
                log.Add("Reading " + bestBook.Title + " worth " + (effectiveDesire).ToString("0.000"));
                ReadActivity readingAction = new(bestBook, effectiveDesire);
                alreadyConsidered.Add(readingAction);
            }
            else if(ConditionDepth < 10 && AgeToCompleteBy > Mage.SeasonalAge)
            {
                // consider both writing and vis to provide the capital to trade for a book?
                WritingHelper writingHelper = new(Mage, AgeToCompleteBy - 1, (ushort)(ConditionDepth + 1), _desireFunc);
                writingHelper.AddActionPreferencesToList(alreadyConsidered, log);

                FindVisSourceHelper findVisSourceHelper = new(Mage, MagicArts.GetEnumerator().ToList() , AgeToCompleteBy - 1, (ushort)(ConditionDepth + 1), _desireFunc);
                findVisSourceHelper.AddActionPreferencesToList(alreadyConsidered, log);
            }
        }
    }
}
