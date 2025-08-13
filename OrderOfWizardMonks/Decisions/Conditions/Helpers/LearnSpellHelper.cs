using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Activities;
using WizardMonks.Activities.MageActivities;
using WizardMonks.Core;
using WizardMonks.Economy;
using WizardMonks.Instances;
using WizardMonks.Models.Books;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Spells;
using WizardMonks.Services.Characters;

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
                ushort existingLevel = 0;
                Spell bestSpell = _mage.GetBestSpell(_spellBase);
                if (bestSpell != null)
                {
                    existingLevel = bestSpell.Level;
                }

                ushort spontLevel = SpellLevelMath.GetLevelFromMagnitude(SpellLevelMath.GetMagnitudesFromLevel(_mage.GetSpontaneousCastingTotal(_spellBase.ArtPair)));
                if (spontLevel > existingLevel)
                {
                    existingLevel = spontLevel;
                }

                // determine the level of a spell the mage can invent in a single season
                double labTotal = _mage.GetLabTotal(_spellBase.ArtPair, Activity.InventSpells);
                if(bestSpell != null)
                {
                    // account for already knowing a similar spell in the lab total
                    labTotal += bestSpell.Level / 5.0;
                }
                double singleSeasonPowerLevel = labTotal / 2.0;
                ushort singleSeasonSpellLevel = SpellLevelMath.GetLevelFromMagnitude(SpellLevelMath.GetMagnitudesFromLevel(singleSeasonPowerLevel));

                // if the mage has a lab text with this effect of level between singleSeasonSpellLevel and labTotal, invent that instead
                double minimumLabTextLevel = Math.Max(singleSeasonSpellLevel, existingLevel);
                var labTexts = _mage.GetLabTextsFromCollection(_spellBase).Where(t => t.SpellContained.Level < labTotal && t.SpellContained.Level >= minimumLabTextLevel);
                if(labTexts.Any())
                {
                    ConsiderLearningSpell(alreadyConsidered, log, existingLevel, labTexts, desires);
                }
                // TODO: we're going to have to put a lot of design thought into making this flexible
                else if(singleSeasonSpellLevel > existingLevel && singleSeasonSpellLevel >= SpellLevelMath.GetLevelFromMagnitude(_spellBase.Magnitude))
                {
                    ConsiderInventingSpell(alreadyConsidered, log, existingLevel, singleSeasonSpellLevel);
                }

                // increase Lab Total
                if (_ageToCompleteBy > _mage.SeasonalAge && _conditionDepth < 10)
                {
                    LabTotalIncreaseHelper labTotalIncreaseHelper = new(
                        _mage, 
                        _ageToCompleteBy - 1, 
                        (ushort)(_conditionDepth + 1), 
                        _spellBase.ArtPair, 
                        Activity.InventSpells, 
                        CalculateScoreGainDesire);
                    labTotalIncreaseHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);
                }

                // add a desire for a lab text better than the best one the mage knows
                double desiredLevelBaseline = existingLevel;
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
                desires.AddLabTextDesire(new LabTextDesire(_mage, _spellBase, desiredLevelBaseline, labTotal));
            }
        }

        private void ConsiderLearningSpell(ConsideredActions alreadyConsidered, IList<string> log, ushort existingLevel, IEnumerable<LabText> labTexts, Desires desires)
        {
            // use the highest level lab text
            var labText = labTexts.OrderByDescending(t => t.SpellContained.Level).ThenBy(t => t.IsShorthand).First();
            double magnitudeGain = labText.SpellContained.Level - existingLevel;
            if (_mage.CanUseLabText(labText))
            {
                double desire = _desireFunc(magnitudeGain, _conditionDepth);
                log.Add($"Learning lab text {labText.SpellContained.Name} {labText.SpellContained.Level} worth {desire:0.000}");
                alreadyConsidered.Add(new LearnSpellFromLabTextActivity(labText, Abilities.MagicTheory, desire));
            }
            else
            {
                double labTotal = _mage.GetLabTotal(labText.SpellContained.Base.ArtPair, Activity.TranslateLabText);
                ushort? shorthandLearned = _mage.GetDeciperedLabTextLevel(labText.Author);
                if(shorthandLearned.HasValue)
                {
                    labTotal += (ushort)shorthandLearned;
                }
                // We cannot use this text. The prerequisite action is to translate it.
                log.Add($"Cannot use lab text for '{labText.SpellContained.Name}', must decipher shorthand first.");

                // Calculate desire for translating. The value is tied to the ultimate goal of learning the spell.
                double seasonsToTranslate = Math.Ceiling(labText.SpellContained.Level / labTotal);

                // The desire is inherited from the parent desire, but deferred by the time it takes to translate.
                double effectiveDesire = _desireFunc(labText.SpellContained.Level, (ushort)(_conditionDepth + seasonsToTranslate));
                // unconventional magi will prefer to make something new over using existing spells
                effectiveDesire *= _mage.Personality.GetInverseDesireMultiplier(HexacoFacet.Unconventionality);

                var translateActivity = new TranslateShorthandActivity(labText, Abilities.MagicTheory, effectiveDesire);
                alreadyConsidered.Add(translateActivity);

                // Also, consider actions to speed up translation (i.e., increase Lab Total).
                if (seasonsToTranslate > 1)
                {
                    var labTotalHelper = new LabTotalIncreaseHelper(
                        _mage, 
                        _ageToCompleteBy - 1, 
                        (ushort)(_conditionDepth + 1), 
                        labText.SpellContained.Base.ArtPair, 
                        Activity.InventSpells,
                        CalculateScoreGainDesire);
                    labTotalHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);
                }
            }
        }

        private void ConsiderInventingSpell(ConsideredActions alreadyConsidered, IList<string> log, ushort existingLevel, ushort singleSeasonSpellLevel)
        {
            Spell newSpell;
            ushort availableMagnitudes = (ushort)(SpellLevelMath.GetMagnitudesFromLevel(singleSeasonSpellLevel) - _spellBase.Magnitude);
            string spellName = $"{_mage.Name}'s {_spellBase.Name} {availableMagnitudes}";
            switch (availableMagnitudes)
            {
                case 0:
                    newSpell =
                        new Spell(EffectRanges.Personal, EffectDurations.Instant, EffectTargets.Taste, _spellBase, 0, false, spellName);
                    break;
                case 1:
                    newSpell =
                        new Spell(EffectRanges.Personal, EffectDurations.Instant, EffectTargets.Touch, _spellBase, 0, false, spellName);
                    break;
                case 2:
                    newSpell =
                        new Spell(EffectRanges.Personal, EffectDurations.Instant, EffectTargets.Smell, _spellBase, 0, false, spellName);
                    break;
                case 3:
                    newSpell =
                        new Spell(EffectRanges.Personal, EffectDurations.Instant, EffectTargets.Hearing, _spellBase, 0, false, spellName);
                    break;
                case 4:
                    newSpell =
                        new Spell(EffectRanges.Personal, EffectDurations.Instant, EffectTargets.Sight, _spellBase, 0, false, spellName);
                    break;
                case 5:
                    newSpell =
                        new Spell(EffectRanges.Personal, EffectDurations.Diameter, EffectTargets.Sight, _spellBase, 0, false, spellName);
                    break;
                case 6:
                    newSpell =
                        new Spell(EffectRanges.Personal, EffectDurations.Sun, EffectTargets.Sight, _spellBase, 0, false, spellName);
                    break;
                default:
                    newSpell =
                        new Spell(EffectRanges.Personal, EffectDurations.Moon, EffectTargets.Sight, _spellBase, 0, false, spellName);
                    break;
            }
            ushort magnitudeGain = SpellLevelMath.GetMagnitudeDifferenceBetweenLevels(singleSeasonSpellLevel, existingLevel);
            double desire = _desireFunc(magnitudeGain, _conditionDepth);
            // creative magi like inventing new spells
            desire *= _mage.Personality.GetDesireMultiplier(HexacoFacet.Creativity);
            log.Add($"Inventing {newSpell.Name} {newSpell.Level} worth {desire:0.000}");
            alreadyConsidered.Add(new InventSpellActivity(newSpell, Abilities.MagicTheory, desire));
        }

        private double CalculateScoreGainDesire(double gain, ushort conditionDepth)
        {
            return _desireFunc(gain / 2, conditionDepth);
        }
    }
}
