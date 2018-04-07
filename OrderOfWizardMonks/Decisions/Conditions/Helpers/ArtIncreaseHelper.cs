using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks.Decisions.Conditions.Helpers
{
    public class ArtIncreaseHelper : AHelper
    {
        private ArtPair _arts;
        public bool AllowVimVisUse { get; private set; }

        public ArtIncreaseHelper(Magus mage, uint ageToCompleteBy, double desirePerPoint, ushort conditionDepth, ArtPair arts, bool allowVimVisUse, CalculateDesireFunc desireFunc) :
            base(mage, ageToCompleteBy, desirePerPoint, conditionDepth, desireFunc)
        {
            _arts = arts;
            AllowVimVisUse = allowVimVisUse;
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log)
        {
            if (AgeToCompleteBy - Mage.SeasonalAge > 0)
            {
                // increase non-vim through vis study
                AddVisUseToActionList(_arts.Technique, alreadyConsidered, log);
                if (AllowVimVisUse || _arts.Form.AbilityName != "Vim")
                {
                    AddVisUseToActionList(_arts.Form, alreadyConsidered, log);
                }

                // increase either art through reading
                ReadingHelper techReadingHelper = new ReadingHelper(_arts.Technique, Mage, AgeToCompleteBy - 1, Desire, (ushort)(ConditionDepth + 1), _desireFunc);
                ReadingHelper formReadingHelper = new ReadingHelper(_arts.Form, Mage, AgeToCompleteBy - 1, Desire, (ushort)(ConditionDepth + 1), _desireFunc);
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
                double effectiveDesire = _desireFunc(gain, ConditionDepth, TimeUntilDue);
                VisStudying visStudy = new VisStudying(magicArt.Ability, effectiveDesire);
                alreadyConsidered.Add(visStudy);
                // consider the value of finding a better aura to study vis in
                FindNewAuraHelper auraHelper = new FindNewAuraHelper(Mage, AgeToCompleteBy - 1, Desire, (ushort)(ConditionDepth + 1), AllowVimVisUse, _desireFunc);

                // TODO: how do we decrement the cost of the vis?
            }
        }
    }
}
