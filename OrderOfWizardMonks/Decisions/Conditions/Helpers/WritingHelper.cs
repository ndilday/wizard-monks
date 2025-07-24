using System.Collections.Generic;
using WizardMonks.Activities.ExposingActivities;
using WizardMonks.Instances;

namespace WizardMonks.Decisions.Conditions.Helpers
{
    /// <summary>
    /// Writing as an activity is generally mwant to gain vis or a book in exchange
    /// The intent is to find a topic the author can write upon that they don't already have a book in the library
    /// available for trade for which there is demand in the market
    /// </summary>
    class WritingHelper : AHelper
    {
        public WritingHelper(Magus mage, uint ageToCompleteBy, ushort conditionDepth, CalculateDesireFunc desireFunc = null) :
            base(mage, ageToCompleteBy, conditionDepth, desireFunc)
        {
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, Desires desires, IList<string> log)
        {
            var bestBook = Mage.GetBestBookToWrite();
            if (bestBook != null)
            {
                double effectiveDesire = _desireFunc(bestBook.Value, ConditionDepth);
                log.Add("Writing " + bestBook.Title + " worth " + (effectiveDesire).ToString("0.000"));
                WriteActivity writingAction = new(bestBook.Topic, bestBook.Title, Abilities.Latin, bestBook.Level, effectiveDesire);
                alreadyConsidered.Add(writingAction);
            }
        }
    }
}
