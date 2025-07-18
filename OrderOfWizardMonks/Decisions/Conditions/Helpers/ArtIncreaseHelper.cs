using System.Collections.Generic;
using WizardMonks.Activities.MageActivities;

namespace WizardMonks.Decisions.Conditions.Helpers
{
    public class ArtIncreaseHelper : AHelper
    {
        private ArtPair _arts;

        public ArtIncreaseHelper(Magus mage, uint ageToCompleteBy, ushort conditionDepth, ArtPair arts, CalculateDesireFunc desireFunc) :
            base(mage, ageToCompleteBy, conditionDepth, desireFunc)
        {
            _arts = arts;
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log)
        {
            if (AgeToCompleteBy > Mage.SeasonalAge)
            {
                // increase non-vim through vis study
                AddVisUseToActionList(_arts.Technique, alreadyConsidered, log);
                AddVisUseToActionList(_arts.Form, alreadyConsidered, log);

                // increase either art through reading
                ReadingHelper techReadingHelper = new(_arts.Technique, Mage, AgeToCompleteBy - 1, (ushort)(ConditionDepth + 1), _desireFunc);
                ReadingHelper formReadingHelper = new(_arts.Form, Mage, AgeToCompleteBy - 1, (ushort)(ConditionDepth + 1), _desireFunc);
                techReadingHelper.AddActionPreferencesToList(alreadyConsidered, log);
                formReadingHelper.AddActionPreferencesToList(alreadyConsidered, log);
            }
        }

        private void AddVisUseToActionList(Ability art, ConsideredActions alreadyConsidered, IList<string> log)
        {
            CharacterAbilityBase magicArt = Mage.GetAbility(art);
            double stockpile = Mage.GetVisCount(art);
            double visNeed = 0.5 + (magicArt.Value / 10.0);

            // if so, assume vis will return an average of 6XP + aura
            if (stockpile > visNeed)
            {
                double gain = magicArt.GetValueGain(Mage.VisStudyRate);
                double effectiveDesire = _desireFunc(gain, ConditionDepth);
                StudyVisActivity visStudy = new(magicArt.Ability, effectiveDesire);
                alreadyConsidered.Add(visStudy);
                // consider the value of finding a better aura to study vis in
                FindNewAuraHelper auraHelper = new(Mage, AgeToCompleteBy - 1, (ushort)(ConditionDepth + 1), _desireFunc);

                // TODO: how do we decrement the cost of the vis?
            }
        }
    }
}
