using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Activities;
using WizardMonks.Activities.MageActivities;
using WizardMonks.Economy;
using WizardMonks.Instances;

namespace WizardMonks.Decisions.Conditions.Helpers
{
    class LearnSpellHelper : AHelper
    {
        private SpellBase _spellBase;
        public LearnSpellHelper(Magus mage, uint ageToCompleteBy, ushort conditionDepth, SpellBase spellBase, CalculateDesireFunc desireFunc) :
            base(mage, ageToCompleteBy, conditionDepth, desireFunc)
        {
            _spellBase = spellBase;
        }
        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, Desires desires, IList<string> log)
        {
            if(_mage.Laboratory == null)
            {
                if (_ageToCompleteBy - 1 > _mage.SeasonalAge)
                {
                    HasLabCondition labCondition = 
                        new(_mage, _ageToCompleteBy - 1, (ushort)(_conditionDepth + 1));
                    labCondition.AddActionPreferencesToList(alreadyConsidered, desires, log);
                }
            }
            else if (_ageToCompleteBy > _mage.SeasonalAge)
            {
                double minLevel = 0;
                // see if the mage already knows a spell
                Spell bestSpell = _mage.GetBestSpell(_spellBase);
                if(bestSpell != null)
                {
                    minLevel = bestSpell.Level;
                }
                else
                {
                    minLevel = _mage.GetSpontaneousCastingTotal(_spellBase.ArtPair);
                }

                // determine the level of a spell the mage can invent in a single season
                double labTotal = _mage.GetLabTotal(_spellBase.ArtPair, Activity.InventSpells);
                if(bestSpell != null)
                {
                    // account for already knowing a similar spell in the lab total
                    labTotal += bestSpell.Level / 5.0;
                }
                double singleSeasonSpellLevel = labTotal / 2.0;
                if (singleSeasonSpellLevel > 5)
                {
                    // round off to a multiple of 5
                    singleSeasonSpellLevel = Math.Floor(singleSeasonSpellLevel / 5) * 5;
                }
                else
                {
                    singleSeasonSpellLevel = Math.Floor(singleSeasonSpellLevel);
                }

                // if the mage has a lab text with this effect of level between singleSeasonSpellLevel and labTotal, invent that instead
                double minimumLabTextLevel = Math.Max(singleSeasonSpellLevel, minLevel);
                var labTexts = _mage.GetLabTexts(_spellBase).Where(t => t.SpellContained.Level < labTotal && t.SpellContained.Level >= minimumLabTextLevel);
                if(labTexts.Any())
                {
                    ConsiderLearningSpell(alreadyConsidered, log, minLevel, labTexts);
                }
                // TODO: we're going to have to put a lot of design thought into making this flexible
                else if(singleSeasonSpellLevel > minLevel && singleSeasonSpellLevel >= _spellBase.Level)
                {
                    ConsiderInventingSpell(alreadyConsidered, log, minLevel, singleSeasonSpellLevel);
                }

                // increase Lab Total
                if (_ageToCompleteBy > _mage.SeasonalAge && _conditionDepth < 10)
                {
                    LabTotalIncreaseHelper labTotalIncreaseHelper =
                        new(_mage, _ageToCompleteBy - 1, (ushort)(_conditionDepth + 1), _spellBase.ArtPair, CalculateScoreGainDesire);
                    labTotalIncreaseHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);
                }

                // add a desire for a lab text better than the best one the mage knows
                double desiredLevelBaseline = minLevel;
                if(labTexts.Any())
                {
                    double bestLabTextLevel = labTexts.Max(lt => lt.SpellContained.Level);
                    if(bestLabTextLevel < 5)
                    {
                        bestLabTextLevel++;
                    }
                    else
                    {
                        bestLabTextLevel += 5;
                    }
                    if(bestLabTextLevel > desiredLevelBaseline)
                    {
                        desiredLevelBaseline = bestLabTextLevel;
                    }
                }
                desires.AddLabTextDesire(new LabTextDesire(_mage, _spellBase, desiredLevelBaseline));
            }
        }

        private void ConsiderLearningSpell(ConsideredActions alreadyConsidered, IList<string> log, double minLevel, IEnumerable<LabText> labTexts)
        {
            // use the highest level lab text
            var labText = labTexts.OrderByDescending(t => t.SpellContained.Level).First();
            double desire = _desireFunc((labText.SpellContained.Level - minLevel), _conditionDepth);
            log.Add($"Learning lab text {labText.SpellContained.Name} {labText.SpellContained.Level} worth {desire:0.000}");
            alreadyConsidered.Add(new LearnSpellFromLabTextActivity(labText, Abilities.MagicTheory, desire));
        }

        private void ConsiderInventingSpell(ConsideredActions alreadyConsidered, IList<string> log, double minLevel, double singleSeasonSpellLevel)
        {
            Spell newSpell;
            switch (singleSeasonSpellLevel - _spellBase.Level)
            {
                case 0:
                    newSpell =
                        new Spell(EffectRanges.Personal, EffectDurations.Instant, EffectTargets.Taste, _spellBase, 0, false, _spellBase.Name);
                    break;
                case 1:
                    newSpell =
                        new Spell(EffectRanges.Personal, EffectDurations.Instant, EffectTargets.Touch, _spellBase, 0, false, _spellBase.Name);
                    break;
                case 2:
                    newSpell =
                        new Spell(EffectRanges.Personal, EffectDurations.Instant, EffectTargets.Smell, _spellBase, 0, false, _spellBase.Name);
                    break;
                case 3:
                    newSpell =
                        new Spell(EffectRanges.Personal, EffectDurations.Instant, EffectTargets.Hearing, _spellBase, 0, false, _spellBase.Name);
                    break;
                case 4:
                    newSpell =
                        new Spell(EffectRanges.Personal, EffectDurations.Instant, EffectTargets.Sight, _spellBase, 0, false, _spellBase.Name);
                    break;
                case 5:
                    newSpell =
                        new Spell(EffectRanges.Personal, EffectDurations.Diameter, EffectTargets.Sight, _spellBase, 0, false, _spellBase.Name);
                    break;
                case 10:
                    newSpell =
                        new Spell(EffectRanges.Personal, EffectDurations.Sun, EffectTargets.Sight, _spellBase, 0, false, _spellBase.Name);
                    break;
                default:
                    newSpell =
                        new Spell(EffectRanges.Personal, EffectDurations.Moon, EffectTargets.Sight, _spellBase, 0, false, _spellBase.Name);
                    break;
            }
            double desire = _desireFunc((singleSeasonSpellLevel - minLevel), _conditionDepth);
            log.Add($"Inventing {newSpell.Name} {newSpell.Level} worth {desire:0.000}");
            alreadyConsidered.Add(new InventSpellActivity(newSpell, Abilities.MagicTheory, desire));
        }

        private double CalculateScoreGainDesire(double gain, ushort conditionDepth)
        {
            return _desireFunc(gain / 2, conditionDepth);
        }
    }
}
