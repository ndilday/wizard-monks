using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks.Decisions.Conditions.Helpers
{
    class LabTotalIncreaseHelper : AHelper
    {
        private ArtPair _arts;

        public LabTotalIncreaseHelper(Magus mage, uint ageToCompleteBy, double desirePerPoint, ushort conditionDepth, ArtPair arts) :
            base(mage, ageToCompleteBy, desirePerPoint, conditionDepth)
        {
            _arts = arts;
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log)
        {
            if (AgeToCompleteBy - Mage.SeasonalAge > 0)
            {
                // let's say it's worth half the difference in desire from the original

                // increase non-vim through vis study
                AddVisUseToActionList(_arts.Technique, alreadyConsidered, log);
                if(_arts.Form.AbilityName != "Vim")
                {
                    AddVisUseToActionList(_arts.Form, alreadyConsidered, log);
                }

                // increase either art through reading

                // increase Magic Theory through reading or practice

                // improve lab

                // find better aura
                FindNewAuraHelper helper = new FindNewAuraHelper(Mage, AgeToCompleteBy, Desire, (ushort)(ConditionDepth + 1));
                
                // increase int
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
                double effectiveDesire = gain * Desire / ConditionDepth;
                VisStudying visStudy = new VisStudying(magicArt.Ability, effectiveDesire);
                alreadyConsidered.Add(visStudy);
                // TODO: how do we decrement the cost of the vis?
            }
            // searching for a new aura could improve the vis use
        }
    }
}
