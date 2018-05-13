using System;
using System.Collections.Generic;
using System.Linq;

using WizardMonks.Instances;

namespace WizardMonks.Decisions.Conditions.Helpers
{
    public class FindNewAuraHelper : AHelper
    {
        private Area _location;
        private bool _allowVimVisUse;
        private int _auraCount;
        private double _currentAura;
        private double _currentScore;
        //private double _currentDesire;

        public FindNewAuraHelper(Magus mage, Area location, uint? ageToCompleteBy, double desirePerPoint, ushort conditionDepth, bool allowVimVisUse, CalculateDesireFunc desireFunc) :
            base(mage, ageToCompleteBy, desirePerPoint, conditionDepth, desireFunc)
        {
            _allowVimVisUse = allowVimVisUse;
            _location = location;
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log)
        {
            _currentAura = Mage.Covenant == null ? 0 : Mage.Covenant.Aura.Strength;
            _auraCount = Mage.KnownAuras.Count;

            // for now
            _currentScore= CalculateFindAuraScore();
            double probOfBetter = 1 - (_currentAura * _currentAura * (_auraCount + 1) / (5 * _currentScore));
            double maxAura = Math.Sqrt(5.0 * _currentScore / (_auraCount + 1));
            double averageGain = maxAura * probOfBetter / 2.0;
            double desire = _desireFunc(averageGain, ConditionDepth, TimeUntilDue);
            log.Add("Finding an aura worth " + desire.ToString("0.00000"));

            if (desire > 0.00001)
            {
                alreadyConsidered.Add(new FindAura(_location, _location.AreaLore, desire));

                // consider the value of increasing find aura related scores
                //practice area lore
                PracticeHelper areaLorePracticeHelper = new PracticeHelper(_location.AreaLore, Mage, GetLowerOrderAgeToCompleteBy(-1), Desire, (ushort)(ConditionDepth + 1), CalculateScoreGainDesire);
                areaLorePracticeHelper.AddActionPreferencesToList(alreadyConsidered, log);

                // read area lore
                ReadingHelper readAreaLoreHelper = new ReadingHelper(_location.AreaLore, Mage, GetLowerOrderAgeToCompleteBy(-1), Desire, (ushort)(ConditionDepth + 1), CalculateScoreGainDesire);

                // consider value of increasing InVi casting total
                CastingTotalIncreaseHelper inViHelper = new CastingTotalIncreaseHelper(Mage, GetLowerOrderAgeToCompleteBy(-1), Desire / 10, (ushort)(ConditionDepth + 1), MagicArtPairs.InVi, _allowVimVisUse, _desireFunc);
            }
        }

        private double CalculateFindAuraScore()
        {
            double areaLore = Mage.GetAbility(_location.AreaLore).Value;
            areaLore += Mage.GetCastingTotal(MagicArtPairs.InVi) / 10;
            areaLore += Mage.GetAttribute(AttributeType.Perception).Value;
            return areaLore;
        }

        private double CalculateScoreGainDesire(double gain, ushort conditionDepth, uint timeToComplete)
        {
            double newScore = _currentScore + gain;
            double probOfBetterNow = 1 - (_currentAura * _currentAura * (_auraCount + 1) / (5 * _currentScore));
            double probOfBetterWithGain = 1 - (_currentAura * _currentAura * (_auraCount + 1) / (5 * newScore));
            double maxAuraNow = Math.Sqrt(5.0 * _currentScore / (_auraCount + 1));
            double maxAuraWithGain = Math.Sqrt(5.0 * newScore / (_auraCount + 1));
            double averageGainNow = maxAuraNow * probOfBetterNow / 2.0;
            double averageGainWithGain = maxAuraWithGain * probOfBetterWithGain / 2.0;
            return Desire * (averageGainWithGain - averageGainNow) / (conditionDepth * timeToComplete);
            
        }
    }
}
