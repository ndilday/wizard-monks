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
        private double _currentScore;
        private double _currentDesire;
        private List<Ability> _visTypes;
        private bool _allowVimVis;

        public FindVisSourceHelper(Magus mage, List<Ability> visTypes, uint ageToCompleteBy, double desirePerPoint, ushort conditionDepth, bool allowVimVis, CalculateDesireFunc desireFunc) :
            base(mage, ageToCompleteBy, desirePerPoint, conditionDepth, desireFunc)
        {
            _visTypes = visTypes;
            _allowVimVis = allowVimVis;
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
            double magicLore = Mage.GetAbility(Abilities.MagicLore).Value;
            magicLore += Mage.GetAttribute(AttributeType.Perception).Value;
            magicLore += Mage.GetCastingTotal(MagicArtPairs.InVi) / 5;
            if (magicLore > 0 && Mage.KnownAuras.Any())
            {
                Aura aura = Mage.KnownAuras.OrderByDescending(a => a.GetAverageVisSourceSize(magicLore)).First();
                double averageFind = aura.GetAverageVisSourceSize(magicLore);
                if (averageFind > 0)
                {
                    // modify by chance vis will be of the proper type
                    double gain = (averageFind * _visTypes.Count() / 15);
                    double desire = _desireFunc(gain, ConditionDepth);

                    // TODO: modify by lifelong value of source?
                    log.Add("Looking for vis source worth " + (desire).ToString("0.00"));
                    alreadyConsidered.Add(new FindVisSource(aura, Abilities.MagicLore, desire));
                }

                // consider the value of increasing the casting total first
                CastingTotalIncreaseHelper castingHelper = 
                    new CastingTotalIncreaseHelper(Mage, AgeToCompleteBy, Desire, (ushort)(ConditionDepth + 1), MagicArtPairs.InVi, _allowVimVis, CalculateScoreGainDesire);
                castingHelper.AddActionPreferencesToList(alreadyConsidered, log);
                // consider the value of increasing Magic Lore
                PracticeHelper practiceHelper = new PracticeHelper(Abilities.MagicLore, Mage, AgeToCompleteBy, Desire, (ushort)(ConditionDepth + 1), CalculateScoreGainDesire);
                practiceHelper.AddActionPreferencesToList(alreadyConsidered, log);
                ReadingHelper readingHelper = new ReadingHelper(Abilities.MagicLore, Mage, AgeToCompleteBy, Desire, (ushort)(ConditionDepth + 1), CalculateScoreGainDesire);
                readingHelper.AddActionPreferencesToList(alreadyConsidered, log);
                // TODO: consider increasing Perception
            }

            // consider finding a whole new aura
            FindNewAuraHelper auraHelper = new FindNewAuraHelper(Mage, AgeToCompleteBy, Desire, (ushort)(ConditionDepth + 1), !_visTypes.Contains(MagicArts.Vim), CalculateAuraGainDesire);
        }

        private double CalculateScoreGainDesire(double gain, ushort conditionDepth)
        {
            double newScore = _currentScore + gain;
            double probOfBetter = 1 - (_currentAura * _currentAura * _auraCount / (5 * newScore));
            double maxAura = Math.Sqrt(5.0 * newScore / _auraCount);
            double averageGain = maxAura * probOfBetter / 2.0;
            return Desire * averageGain / conditionDepth;

        }

        private double CalculateAuraGainDesire(double gain, ushort conditionDepth)
        {
            double currentRoll = Math.Pow(currentVis, 2) / (magicLoreRoll * Strength);
            double multiplier = Math.Sqrt(magicLoreRoll * Strength) * 2 / 3;
            double areaUnder = (11.180339887498948482045868343656 - Math.Pow(currentRoll, 1.5)) * multiplier;
            return areaUnder / 5;
        }
    }
}
