using System.Collections.Generic;

namespace WizardMonks.Decisions.Conditions.Helpers
{
    class ReadingHelper : AHelper
    {
        private Ability _ability;
        public ReadingHelper(Ability ability, Magus mage, uint? ageToCompleteBy, double desire, ushort conditionDepth, CalculateDesireFunc desireFunc = null) :
            base(mage, ageToCompleteBy, desire, conditionDepth, desireFunc)
        {
            _ability = ability;
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log)
        {
            var bestBook = Mage.GetBestBookToRead(_ability);
            if (bestBook != null)
            {
                double gain = Mage.GetBookLevelGain(bestBook);
                double effectiveDesire = _desireFunc(gain, ConditionDepth, TimeUntilDue);
                log.Add("Reading " + bestBook.Title + " worth " + (effectiveDesire).ToString("0.000"));
                Read readingAction = new Read(bestBook, effectiveDesire);
                alreadyConsidered.Add(readingAction);
            }
        }
    }
}
