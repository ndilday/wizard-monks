﻿using System;
using System.Collections.Generic;
using System.Linq;

using WizardMonks.Instances;

namespace WizardMonks.Decisions.Conditions.Helpers
{
    public class FindNewAuraHelper : AHelper
    {
        private bool _allowVimVisUse;
        private int _auraCount;
        private double _currentAura;
        private double _currentScore;
        //private double _currentDesire;

        public FindNewAuraHelper(Magus mage, uint ageToCompleteBy, double desirePerPoint, ushort conditionDepth, bool allowVimVisUse, CalculateDesireFunc desireFunc) :
            base(mage, ageToCompleteBy, desirePerPoint, conditionDepth, desireFunc)
        {
            _allowVimVisUse = allowVimVisUse;
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
            double desire = _desireFunc(averageGain, ConditionDepth);

            if (desire > 0.00001)
            {
                
                log.Add("Finding a better aura to build a lab in worth " + desire.ToString("0.000"));
                alreadyConsidered.Add(new FindAura(Abilities.AreaLore, desire));

                // consider the value of increasing find aura related scores
                //practice area lore
                PracticeHelper areaLorePracticeHelper = new PracticeHelper(Abilities.AreaLore, Mage, AgeToCompleteBy - 1, Desire, (ushort)(ConditionDepth + 1), CalculateScoreGainDesire);
                areaLorePracticeHelper.AddActionPreferencesToList(alreadyConsidered, log);

                // read area lore
                ReadingHelper readAreaLoreHelper = new ReadingHelper(Abilities.AreaLore, Mage, AgeToCompleteBy - 1, Desire, (ushort)(ConditionDepth + 1), CalculateScoreGainDesire);

                // consider value of increasing InVi casting total
                CastingTotalIncreaseHelper inViHelper = new CastingTotalIncreaseHelper(Mage, AgeToCompleteBy - 1, Desire / 10, (ushort)(ConditionDepth + 1), MagicArtPairs.InVi, _allowVimVisUse, _desireFunc);

            }
        }

        private double CalculateFindAuraScore()
        {
            double areaLore = Mage.GetAbility(Abilities.AreaLore).Value;
            areaLore += Mage.GetCastingTotal(MagicArtPairs.InVi) / 10;
            areaLore += Mage.GetAttribute(AttributeType.Perception).Value;
            return areaLore;
        }

        private double CalculateScoreGainDesire(double gain, ushort conditionDepth)
        {
            double newScore = _currentScore + gain;
            double probOfBetterNow = 1 - (_currentAura * _currentAura * (_auraCount + 1) / (5 * _currentScore));
            double probOfBetterWithGain = 1 - (_currentAura * _currentAura * (_auraCount + 1) / (5 * newScore));
            double maxAuraNow = Math.Sqrt(5.0 * _currentScore / (_auraCount + 1));
            double maxAuraWithGain = Math.Sqrt(5.0 * newScore / (_auraCount + 1));
            double averageGainNow = maxAuraNow * probOfBetterNow / 2.0;
            double averageGainWithGain = maxAuraWithGain * probOfBetterWithGain / 2.0;
            return Desire * (averageGainWithGain - averageGainNow) / conditionDepth;
            
        }
    }
}
