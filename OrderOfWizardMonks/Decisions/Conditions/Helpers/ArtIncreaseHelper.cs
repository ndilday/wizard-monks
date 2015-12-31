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

        public ArtIncreaseHelper(Magus mage, uint ageToCompleteBy, double desirePerPoint, ushort conditionDepth, ArtPair arts, bool allowVimVisUse) :
            base(mage, ageToCompleteBy, desirePerPoint, conditionDepth)
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
                if (_arts.Form.AbilityName != "Vim")
                {
                    AddVisUseToActionList(_arts.Form, alreadyConsidered, log);
                }

                // TODO: increase either art through reading
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
            // TODO: searching for a new aura could improve the vis use
        }
    }
}
