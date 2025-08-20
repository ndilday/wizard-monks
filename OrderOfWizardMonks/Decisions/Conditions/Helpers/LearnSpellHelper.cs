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
using WizardMonks.Models.Projects;
using WizardMonks.Models.Spells;
using WizardMonks.Services.Characters;

namespace WizardMonks.Decisions.Conditions.Helpers
{
    class LearnSpellHelper : AHelper
    {
        private readonly SpellBase _spellBase;

        public LearnSpellHelper(Magus mage, uint ageToCompleteBy, ushort conditionDepth, SpellBase spellBase, CalculateDesireFunc desireFunc) :
            base(mage, ageToCompleteBy, conditionDepth, desireFunc)
        {
            _spellBase = spellBase;
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, Desires desires, IList<string> log)
        {
            if (_mage.Laboratory == null)
            {
                if (_ageToCompleteBy > _mage.SeasonalAge)
                {
                    HasLabCondition labCondition = new(_mage, _ageToCompleteBy - 1, (ushort)(_conditionDepth + 1));
                    labCondition.AddActionPreferencesToList(alreadyConsidered, desires, log);
                }
                return;
            }

            if (_ageToCompleteBy <= _mage.SeasonalAge) return;

            // Calculate the magus's current capabilities
            Spell bestKnownSpell = _mage.GetBestSpell(_spellBase);
            ushort existingLevel = bestKnownSpell?.Level ?? 0;
            ushort spontLevel = SpellLevelMath.GetLevelFromMagnitude(_mage.GetSpontaneousCastingTotal(_spellBase.ArtPair));
            if (spontLevel > existingLevel)
            {
                existingLevel = spontLevel;
            }

            double labTotal = _mage.GetLabTotal(_spellBase.ArtPair, Activity.InventSpells);
            if (bestKnownSpell != null)
            {
                labTotal += bestKnownSpell.Level / 5.0;
            }

            // Evaluate Primary Actions (Learn or Invent)
            var availableLabTexts = _mage.GetLabTextsFromCollection(_spellBase)
                .Where(t => t.SpellContained.Level < labTotal)
                .ToList();

            bool tookLabTextAction = EvaluateLearningFromLabText(alreadyConsidered, log, existingLevel, availableLabTexts, desires);

            // Only consider inventing from scratch if learning from a text isn't a viable immediate option.
            if (!tookLabTextAction)
            {
                EvaluateInventingNewSpell(alreadyConsidered, log, existingLevel, labTotal);
            }

            // Evaluate Long-Term Improvement Actions
            if (_conditionDepth < 10)
            {
                EvaluateImprovingLabTotal(alreadyConsidered, desires, log);
            }

            GenerateLabTextDesire(desires, existingLevel, availableLabTexts, labTotal);
        }

        /// <summary>
        /// Checks for usable lab texts and adds appropriate actions (Learn or Translate).
        /// </summary>
        /// <returns>True if a learn/translate action was added, otherwise false.</returns>
        private bool EvaluateLearningFromLabText(ConsideredActions alreadyConsidered, IList<string> log, ushort existingLevel, List<LabText> labTexts, Desires desires)
        {
            if (!labTexts.Any()) return false;

            var bestLabText = labTexts.OrderByDescending(t => t.SpellContained.Level).ThenBy(t => t.IsShorthand).First();
            double magnitudeGain = bestLabText.SpellContained.Level - existingLevel;
            if (magnitudeGain <= 0) return false;

            if (_mage.CanUseLabText(bestLabText))
            {
                double desire = _desireFunc(magnitudeGain, _conditionDepth);
                log.Add($"Learning lab text {bestLabText.SpellContained.Name} {bestLabText.SpellContained.Level} worth {desire:0.000}");
                alreadyConsidered.Add(new LearnSpellFromLabTextActivity(bestLabText, Abilities.MagicTheory, desire));
                return true;
            }
            else
            {
                // Cannot use the text, so the action is to translate it.
                double translateLabTotal = _mage.GetLabTotal(bestLabText.SpellContained.Base.ArtPair, Activity.TranslateLabText)
                                           + (_mage.GetDeciperedLabTextLevel(bestLabText.Author) ?? 0);

                double seasonsToTranslate = Math.Ceiling(bestLabText.SpellContained.Level / translateLabTotal);
                double effectiveDesire = _desireFunc(bestLabText.SpellContained.Level, (ushort)(_conditionDepth + seasonsToTranslate));
                effectiveDesire *= _mage.Personality.GetInverseDesireMultiplier(HexacoFacet.Unconventionality);

                alreadyConsidered.Add(new TranslateShorthandActivity(bestLabText, Abilities.MagicTheory, effectiveDesire));

                // If translation will take a while, also consider improving the ability to translate.
                if (seasonsToTranslate > 1)
                {
                    var labTotalHelper = new LabTotalIncreaseHelper(_mage, _ageToCompleteBy - 1, (ushort)(_conditionDepth + 1), bestLabText.SpellContained.Base.ArtPair, Activity.TranslateLabText, CalculateScoreGainDesire);
                    labTotalHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);
                }
                return true;
            }
        }

        /// <summary>
        /// Checks if inventing a new spell is viable and adds a project/activity if so.
        /// </summary>
        private void EvaluateInventingNewSpell(ConsideredActions alreadyConsidered, IList<string> log, ushort existingLevel, double labTotal)
        {
            ushort singleSeasonSpellLevel = SpellLevelMath.GetLevelFromMagnitude(labTotal / 2.0);

            if (singleSeasonSpellLevel > existingLevel && singleSeasonSpellLevel >= _spellBase.Magnitude)
            {
                Spell newSpell = CreateSpellFromLevel(_mage, _spellBase, singleSeasonSpellLevel);

                var existingProject = _mage.ActiveProjects.OfType<SpellInventionProject>()
                    .FirstOrDefault(p => p.SpellToInvent.Base == newSpell.Base && p.SpellToInvent.Level >= newSpell.Level);

                if (existingProject == null)
                {
                    existingProject = new SpellInventionProject(_mage, newSpell);
                    _mage.ActiveProjects.Add(existingProject);
                    log.Add($"[Project Created] Started a project to invent '{newSpell.Name}'.");
                }

                ushort magnitudeGain = SpellLevelMath.GetMagnitudeDifferenceBetweenLevels(newSpell.Level, existingLevel);

                // Calculate desire from two sources: the practical value from the goal, and the inherent prestige value.
                double practicalDesire = _desireFunc(magnitudeGain, _conditionDepth);
                double prestigeDesire = _mage.CalculatePrestigeValueForSpell(newSpell) * _mage.Personality.GetPrestigeMotivation();
                double totalDesire = practicalDesire + prestigeDesire;

                totalDesire *= _mage.Personality.GetDesireMultiplier(HexacoFacet.Creativity);
                log.Add($"Considering work on inventing {newSpell.Name} ({newSpell.Level}) worth {totalDesire:0.000} (Practical: {practicalDesire:0.000}, Prestige: {prestigeDesire:0.000})");
                alreadyConsidered.Add(new InventSpellActivity(existingProject.ProjectId, Abilities.MagicTheory, totalDesire));
            }
        }

        /// <summary>
        /// Considers actions to improve the lab total for future spell invention.
        /// </summary>
        private void EvaluateImprovingLabTotal(ConsideredActions alreadyConsidered, Desires desires, IList<string> log)
        {
            var labTotalHelper = new LabTotalIncreaseHelper(_mage, _ageToCompleteBy - 1, (ushort)(_conditionDepth + 1), _spellBase.ArtPair, Activity.InventSpells, CalculateScoreGainDesire);
            labTotalHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);
        }

        /// <summary>
        /// Adds a desire for a lab text to the global economy if no good options are available locally.
        /// </summary>
        private void GenerateLabTextDesire(Desires desires, ushort existingLevel, List<LabText> availableLabTexts, double labTotal)
        {
            double desiredLevelBaseline = existingLevel;
            if (availableLabTexts.Any())
            {
                desiredLevelBaseline = availableLabTexts.Max(lt => lt.SpellContained.Level) + 5;
            }
            desires.AddLabTextDesire(new LabTextDesire(_mage, _spellBase, desiredLevelBaseline, labTotal));
        }

        private Spell CreateSpellFromLevel(Magus magus, SpellBase spellBase, ushort level)
        {
            ushort totalMagnitudes = SpellLevelMath.GetMagnitudesFromLevel(level);
            ushort availableMagnitudes = (ushort)(totalMagnitudes > spellBase.Magnitude ? totalMagnitudes - spellBase.Magnitude : 0);
            string spellName = $"{magus.Name}'s {spellBase.Name} of {availableMagnitudes} Magnitudes";

            return new Spell(EffectRanges.Touch, EffectDurations.Sun, EffectTargets.Individual, spellBase, 0, false, spellName);
        }

        private double CalculateScoreGainDesire(double gain, ushort conditionDepth)
        {
            return _desireFunc(gain / 2.0, conditionDepth);
        }
    }
}