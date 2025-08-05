using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Activities.ExposingActivities;
using WizardMonks.Instances;
using WizardMonks.Models;

namespace WizardMonks.Decisions.Conditions.Helpers
{
    public class FindNewAuraHelper : AHelper
    {
        private int _auraCount;
        private double _currentAura;
        private double _currentScore;
        private double _averageGain;
        private SpellBase _findAuraSpellBase;

        public FindNewAuraHelper(Magus mage, uint ageToCompleteBy, ushort conditionDepth, CalculateDesireFunc desireFunc) :
            base(mage, ageToCompleteBy, conditionDepth, desireFunc)
        {
            _findAuraSpellBase = SpellBases.GetSpellBaseForEffect(TechniqueEffects.Detect, FormEffects.Aura);
            _currentAura = _mage.KnownAuras.Any() ? _mage.KnownAuras.Max(a => a.Strength) : 0;
            _auraCount = _mage.KnownAuras.Count;
            _currentScore = CalculateFindAuraScore();
            _averageGain = GetAverageNewAura(_currentScore);
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, Desires desires, IList<string> log)
        {
            if (_averageGain > 0)
            {
                double desire = _desireFunc(_averageGain, _conditionDepth);
                desire *= _mage.Personality.GetDesireMultiplier(HexacoFacet.Liveliness);

                log.Add("Finding a better aura to build a lab in worth " + desire.ToString("0.000"));
                alreadyConsidered.Add(new FindAuraActivity(Abilities.AreaLore, desire));
            }

            if (_conditionDepth < 10 && _ageToCompleteBy >  _mage.SeasonalAge)
            {
                // consider the value of increasing find aura related scores
                // practice area lore
                PracticeHelper areaLorePracticeHelper =
                    new(Abilities.AreaLore, _mage, _ageToCompleteBy - 1, (ushort)(_conditionDepth + 1), CalculateFindAuraScoreGainDesire);
                areaLorePracticeHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);

                // read area lore, once we have reasonable ways of defining different areas
                //ReadingHelper readAreaLoreHelper = new ReadingHelper(Abilities.AreaLore, Mage, AgeToCompleteBy - 1, Desire, (ushort)(ConditionDepth + 1), CalculateScoreGainDesire);

                // consider value of increasing InVi casting total
                CastingTotalIncreaseHelper inViHelper =
                    new(_mage, _ageToCompleteBy - 1, (ushort)(_conditionDepth + 1), MagicArtPairs.InVi, CalculateCastingTotalGainDesire);
                inViHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);

                // consider value of learning a new spell to detect auras
                LearnSpellHelper spellHelper =
                    new(_mage, _ageToCompleteBy - 1, (ushort)(_conditionDepth + 1), _findAuraSpellBase, CalculateSpellGainDesire);
                spellHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);
            }
        }

        private double CalculateFindAuraScore()
        {
            double areaLore = _mage.GetAbility(Abilities.AreaLore).Value;
            areaLore += _mage.GetAttribute(AttributeType.Perception).Value;
            Spell bestAuraSearchSpell = _mage.GetBestSpell(_findAuraSpellBase);
            // add 1 per magnitude of detection spell to the total
            if (bestAuraSearchSpell != null)
            {
                areaLore += bestAuraSearchSpell.Level / 5.0;
            }
            else
            {
                areaLore += _mage.GetSpontaneousCastingTotal(MagicArtPairs.InVi) / 5.0;
            }
            return areaLore;
        }

        private double GetAverageNewAura(double newScore)
        {
            double probOfBetterWithGain = 1 - (_currentAura * _currentAura * (_auraCount + 1) / (5 * newScore));
            double maxAuraWithGain = Math.Sqrt(5.0 * newScore / (_auraCount + 1));
            return maxAuraWithGain * probOfBetterWithGain / 2.0;
        }

        private double CalculateFindAuraScoreGainDesire(double gain, ushort conditionDepth)
        {
            double newScore = _currentScore + gain;
            // if the current score is so bad that the gain doesn't bring it over zero,
            // just pretend the current score is zero
            double averageGainWithGain = newScore < 0 ? GetAverageNewAura(gain) : GetAverageNewAura(newScore);
            return _desireFunc(averageGainWithGain - _averageGain, conditionDepth);

        }

        private double CalculateCastingTotalGainDesire(double gain, ushort conditionDepth)
        {
            return CalculateFindAuraScoreGainDesire(gain/25.0, conditionDepth);
        }

        private double CalculateSpellGainDesire(double gain, ushort conditionDepth)
        {
            return CalculateFindAuraScoreGainDesire(gain / 5.0, conditionDepth);
        }
    }
}
