using System;
using System.Collections.Generic;
using System.Linq;

using WizardMonks.Instances;

namespace WizardMonks.Decisions.Conditions.Helpers
{
    public class FindVisSourceHelper : AHelper
    {
        private int _auraCount;
        private double _currentAura;
        private double _currentVis;
        private double _currentScore;
        private List<Ability> _visTypes;
        private double _magicLoreTotal;
        private SpellBase _findVisSpellBase;

        public FindVisSourceHelper(Magus mage, List<Ability> visTypes, uint ageToCompleteBy, double desirePerPoint, ushort conditionDepth, CalculateDesireFunc desireFunc) :
            base(mage, ageToCompleteBy, desirePerPoint, conditionDepth, desireFunc)
        {
            _visTypes = visTypes;
            _findVisSpellBase = SpellBases.GetSpellBaseForEffect(TechniqueEffects.Detect, FormEffects.Vis);
            _auraCount = mage.KnownAuras.Count;
            if (_auraCount == 0)
            {
                _currentAura = 0;
                _currentVis = 0;
            }
            else
            {
                // find the aura with the most vis scource "capacity"
                // this magic lore score builds in an assumed roll on the aura search of 2.5
                Aura aura = Mage.KnownAuras.OrderByDescending(a => a.GetAverageVisSourceSize(_magicLoreTotal)).First();
                _currentAura = aura.Strength;
                _currentVis = aura.VisSources.Sum(vs => vs.Amount);
            }

            _currentScore = mage.GetAbility(Abilities.MagicLore).Value + mage.GetAttribute(AttributeType.Perception).Value + (mage.GetCastingTotal(MagicArtPairs.InVi) / 10);
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log)
        {
            List<Ability> visSearchAbilities = new List<Ability>();
            visSearchAbilities.Add(Abilities.MagicLore);
            visSearchAbilities.Add(MagicArts.Intellego);
            visSearchAbilities.Add(MagicArts.Vim);
            // we're not getting vis fast enough, so we need to find a new source
            // consider the value of searching for new vis sites in current auras
            // determine average vis source found
            _magicLoreTotal = Mage.GetAbility(Abilities.MagicLore).Value;
            _magicLoreTotal += Mage.GetAttribute(AttributeType.Perception).Value;
            Spell bestVisSearchSpell = Mage.GetBestSpell(_findVisSpellBase);
            // add 1 per magnitude of detection spell to the total
            if (bestVisSearchSpell != null)
            {
                _magicLoreTotal += bestVisSearchSpell.Level / 5.0;
            }
            else
            {
                _magicLoreTotal += Mage.GetSpontaneousCastingTotal(MagicArtPairs.InVi) / 5.0;
            }
            if (Mage.KnownAuras.Any())
            {
                Aura aura = Mage.KnownAuras.OrderByDescending(a => a.GetAverageVisSourceSize(_magicLoreTotal)).First();
                double averageFind = aura.GetAverageVisSourceSize(_magicLoreTotal);
                if (averageFind > 1.0)
                {
                    // going to try to go closer to the original logic, where chances the vis type will be acceptable are factored in
                    //averageFind = averageFind * _visTypes.Count / 15;
                    // in this version, unaccecptable vis types get half credit
                    averageFind = averageFind * (_visTypes.Count+15) / 30;
                    double desire = _desireFunc(averageFind, ConditionDepth);

                    // TODO: modify by lifelong value of source?
                    log.Add("Looking for vis source worth " + (desire).ToString("0.000"));
                    alreadyConsidered.Add(new FindVisSource(aura, Abilities.MagicLore, desire));
                }

                // consider the value of increasing the casting total first
                CastingTotalIncreaseHelper castingHelper = 
                    new CastingTotalIncreaseHelper(Mage, AgeToCompleteBy - 1, Desire, (ushort)(ConditionDepth + 1), MagicArtPairs.InVi, CalculateCastingTotalGainDesire);
                castingHelper.AddActionPreferencesToList(alreadyConsidered, log);
                // consider the value of increasing Magic Lore
                PracticeHelper practiceHelper = new PracticeHelper(Abilities.MagicLore, Mage, AgeToCompleteBy - 1, Desire, (ushort)(ConditionDepth + 1), CalculateMagicLoreGainDesire);
                practiceHelper.AddActionPreferencesToList(alreadyConsidered, log);
                ReadingHelper readingHelper = new ReadingHelper(Abilities.MagicLore, Mage, AgeToCompleteBy - 1, Desire, (ushort)(ConditionDepth + 1), CalculateMagicLoreGainDesire);
                readingHelper.AddActionPreferencesToList(alreadyConsidered, log);
                // consider value of learning a new spell to detect auras
                LearnSpellHelper spellHelper =
                    new LearnSpellHelper(Mage, AgeToCompleteBy - 1, Desire, (ushort)(ConditionDepth + 1), _findVisSpellBase, CalculateCastingTotalGainDesire);
                spellHelper.AddActionPreferencesToList(alreadyConsidered, log);
                // TODO: consider increasing Perception
            }

            // consider finding a whole new aura
            FindNewAuraHelper auraHelper = new FindNewAuraHelper(Mage, AgeToCompleteBy - 1, Desire, (ushort)(ConditionDepth + 1), CalculateAuraGainDesire);
            auraHelper.AddActionPreferencesToList(alreadyConsidered, log);
        }

        private double CalculateMagicLoreGainDesire(double gain, ushort conditionDepth)
        {
            double newScore = _currentScore + gain;
            double probOfBetter = 1 - ((_currentVis + 1) * (_currentVis + 1) / (5 * _currentAura * newScore));
            double maxVis = Math.Sqrt(5.0 * newScore * _currentAura);
            double averageGain = maxVis * probOfBetter / 2.0;
            return _desireFunc(averageGain, conditionDepth);

        }

        private double CalculateCastingTotalGainDesire(double gain, ushort conditionDepth)
        {
            double newScore = _currentScore + gain/5.0;
            double probOfBetter = 1 - ((_currentVis + 1) * (_currentVis + 1) / (5 * _currentAura * newScore));
            double maxVis = Math.Sqrt(5.0 * newScore * _currentAura);
            double averageGain = maxVis * probOfBetter / 2.0;
            return _desireFunc(averageGain, conditionDepth);

        }

        private double CalculateAuraGainDesire(double auraGain, ushort conditionDepth)
        {
            double multiplier = Math.Sqrt(_magicLoreTotal * auraGain) * 2 / 3;
            double areaUnder = (11.180339887498948482045868343656) * multiplier;
            double averageFind = areaUnder / 5.0;

            if (averageFind > 1.0)
            {
                double gain = averageFind * (_visTypes.Count + 15) / 30; ;
                return _desireFunc(gain, conditionDepth);
            }
            return 0;
        }
    }
}
