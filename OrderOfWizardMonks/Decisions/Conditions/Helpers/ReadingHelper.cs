using System.Collections.Generic;
using System.Linq;
using WizardMonks.Activities;
using WizardMonks.Economy;
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

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, Desires desires, IList<string> log)
        {
            var bestBook = _mage.GetBestBookToRead(_ability);
            if (bestBook != null)
            {
                double gain = _mage.GetBookLevelGain(bestBook);
                double effectiveDesire = _desireFunc(gain, _conditionDepth);
                log.Add("Reading " + bestBook.Title + " worth " + (effectiveDesire).ToString("0.000"));
                ReadActivity readingAction = new(bestBook, effectiveDesire);
                alreadyConsidered.Add(readingAction);
            }
            else if(_conditionDepth < 10 && _ageToCompleteBy > _mage.SeasonalAge)
            {
                // add a book in this topic to the desired list
                // for consistency, we will assume a Quality of 7
                double newBookDesire = _desireFunc(7, _conditionDepth);
                desires.AddBookDesire(new BookDesire(_mage, _ability, newBookDesire, _mage.GetAbility(_ability).Value));

                // consider both writing and vis to provide the capital to trade for a book?
                WritingHelper writingHelper = new(_mage, _ageToCompleteBy - 1, (ushort)(_conditionDepth + 1), _desireFunc);
                writingHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);

                FindVisSourceHelper findVisSourceHelper = new(_mage, MagicArts.GetEnumerator().ToList() , _ageToCompleteBy - 1, (ushort)(_conditionDepth + 1), _desireFunc);
                findVisSourceHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);
            }
        }
    }
}
