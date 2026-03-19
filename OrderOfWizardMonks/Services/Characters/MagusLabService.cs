using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Activities;
using WizardMonks.Instances;
using WizardMonks.Models;
using WizardMonks.Models.Beliefs;
using WizardMonks.Models.Books;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Laboratories;
using WizardMonks.Models.Spells;

namespace WizardMonks.Services.Characters
{
    public static class MagusLabService
    {
        /// <summary>
        /// Computes the Lab Total for a given art pair and activity.
        ///
        /// If the mage's tradition defines a TraditionActivityFormula for this
        /// activity, that formula is evaluated. Otherwise the standard Hermetic
        /// formula is used: Technique + Form + Intelligence + MagicTheory +
        /// aura + lab modifier + apprentice contribution.
        ///
        /// The formula-driven path allows non-standard traditions to have
        /// structurally different lab totals without requiring separate service
        /// classes.
        /// </summary>
        public static double GetLabTotal(this HermeticMagus mage, ArtPair artPair, Activity activity)
        {
            double auraStrength = mage.Covenant?.Aura.Strength ?? 0;
            double labBonus = mage.Laboratory?.GetModifier(artPair, activity) ?? 0;

            // If the tradition defines a formula for this activity, use it.
            if (mage.IsOpened)
            {
                var formula = mage.Tradition.GetFormula(activity);
                if (formula != null)
                {
                    double formulaTotal = formula.Evaluate(mage, auraStrength, labBonus);

                    // Apprentice contribution is always additive on top of formula total,
                    // as it represents a lab helper bonus independent of tradition structure.
                    if (mage.Apprentice != null)
                    {
                        formulaTotal += mage.Apprentice.GetAbility(mage.MagicAbility).Value
                                      + mage.Apprentice.GetAttributeValue(AttributeType.Intelligence);
                    }

                    //TODO: foci
                    //TODO: lab assistant (non-apprentice)
                    //TODO: familiar
                    return formulaTotal;
                }
            }

            // Standard Hermetic formula fallback.
            double magicTheory = mage.GetAbility(mage.MagicAbility).Value;
            double techValue = mage.Arts.GetAbility(artPair.Technique).Value;
            double formValue = mage.Arts.GetAbility(artPair.Form).Value;
            double labTotal = magicTheory + techValue + formValue
                            + mage.GetAttribute(AttributeType.Intelligence).Value
                            + auraStrength
                            + labBonus;

            if (mage.Apprentice != null)
            {
                labTotal += mage.Apprentice.GetAbility(mage.MagicAbility).Value
                          + mage.Apprentice.GetAttributeValue(AttributeType.Intelligence);
            }

            //TODO: foci
            //TODO: lab assistant
            //TODO: familiar
            return labTotal;
        }

        public static double GetSpellLabTotal(this HermeticMagus mage, Spell spell)
        {
            double total = mage.GetLabTotal(spell.Base.ArtPair, Activity.InventSpells);
            Spell similarSpell = mage.GetBestSpell(spell.Base);
            if (similarSpell != null)
            {
                total += similarSpell.Level / 5.0;
            }
            return total;
        }

        /// <summary>
        /// Returns the vis distillation rate for this mage — the number of pawns
        /// of Vim vis they can extract from their aura in a single season.
        ///
        /// Uses the tradition's formula for DistillVis if defined, otherwise
        /// falls back to the standard Hermetic formula: CrVi Lab Total / 10.
        /// </summary>
        public static double GetVisDistillationRate(this HermeticMagus mage)
        {
            if (mage.IsOpened)
            {
                var formula = mage.Tradition.GetFormula(Activity.DistillVis);
                if (formula != null)
                {
                    double auraStrength = mage.Covenant?.Aura.Strength ?? 0;
                    double labBonus = mage.Laboratory?.GetModifier(MagicArtPairs.CrVi, Activity.DistillVis) ?? 0;
                    return formula.Evaluate(mage, auraStrength, labBonus);
                }
            }

            // Standard Hermetic fallback.
            return mage.GetLabTotal(MagicArtPairs.CrVi, Activity.DistillVis) / 10.0;
        }

        /// <summary>
        /// Returns the vis study quality for this mage — the effective source
        /// quality when studying a magical Art from raw vis.
        ///
        /// Studying vis is not a lab activity (it can be done anywhere), but its
        /// quality is tradition-gated: only traditions that support StudyVis can
        /// use this method. Throws if the mage's tradition does not support vis
        /// study.
        ///
        /// The base quality is a stress die result + aura strength. The formula
        /// on the tradition captures the aura contribution; the die roll is
        /// handled at the activity layer. This method returns the non-random
        /// component (the aura contribution) for use in AI planning.
        ///
        /// Standard Hermetic formula: aura strength (die roll handled separately).
        /// </summary>
        public static double GetVisStudyAuraBonus(this HermeticMagus mage)
        {
            if (!mage.IsOpened || !mage.Tradition.SupportsActivity(Activity.StudyVis))
                throw new InvalidOperationException(
                    $"{mage.Name}'s tradition does not support studying vis.");

            var formula = mage.Tradition.GetFormula(Activity.StudyVis);
            if (formula != null)
            {
                double auraStrength = mage.Covenant?.Aura.Strength ?? 0;
                return formula.Evaluate(mage, auraStrength, 0);
            }

            // Standard Hermetic: aura bonus only (no fixed ability components).
            return mage.Covenant?.Aura.Strength ?? 0;
        }

        public static void BuildLaboratory(this HermeticMagus mage)
        {
            mage.Laboratory = new Laboratory(mage, mage.Covenant.Aura, 0);
        }

        public static void BuildLaboratory(this HermeticMagus mage, Aura aura)
        {
            mage.Laboratory = new Laboratory(mage, aura, 0);
        }

        public static void RefineLaboratory(this HermeticMagus mage)
        {
            if (mage.Laboratory == null)
                throw new NullReferenceException("The mage has no laboratory!");
            if (mage.GetAbility(mage.MagicAbility).Value - 4 < mage.Laboratory.Refinement)
                throw new ArgumentOutOfRangeException(
                    "The mage's magical understanding is not high enough to refine this laboratory any further.");
            mage.Laboratory.Refine();
        }

        public static void AddFeatureToLaboratory(this HermeticMagus mage, LabFeature feature)
        {
            if (mage.Laboratory == null)
                throw new NullReferenceException("The mage has no laboratory!");
            // TODO: Implement
        }

        public static void ExtractVis(this HermeticMagus mage, Ability exposureAbility)
        {
            mage.VisStock[MagicArts.Vim] += mage.GetVisDistillationRate();
            mage.GetAbility(exposureAbility).AddExperience(2);
        }

        public static void LearnSpellFromLabText(this HermeticMagus mage, LabText text)
        {
            // TODO: multiple spells in a season
            // TODO: foci
            Spell spell = text.SpellContained;
            double labTotal = mage.GetSpellLabTotal(spell);
            if (labTotal < spell.Level)
                throw new ArgumentException("This mage cannot invent this spell!");

            mage.LearnSpell(spell);
            foreach (var belief in text.BeliefPayload)
            {
                mage.GetBeliefProfile(text.Author).AddOrUpdateBelief(
                    new Belief(belief.Topic, belief.Magnitude));

                if (text.Author is HermeticMagus authorMagus)
                {
                    var houseSubject = Houses.GetSubject(authorMagus.House);
                    mage.GetBeliefProfile(houseSubject).AddOrUpdateBelief(
                        new Belief(belief.Topic, belief.Magnitude * 0.20));
                }
            }
        }

        public static void LearnSpell(this HermeticMagus mage, Spell spell)
        {
            mage.SpellList.Add(spell);
            LabText newLabText = new()
            {
                Author = mage,
                IsShorthand = true,
                SpellContained = spell
            };
            double magnitude = spell.Level / 5.0;
            newLabText.BeliefPayload.Add(
                new Belief(spell.Base.ArtPair.Technique.AbilityName, magnitude));
            newLabText.BeliefPayload.Add(
                new Belief(spell.Base.ArtPair.Form.AbilityName, magnitude));
            mage.LabTextsOwned.Add(newLabText);
        }

        public static IEnumerable<LabText> GetLabTextsFromCollection(this HermeticMagus mage, SpellBase spellBase)
        {
            return mage.LabTextsOwned.Where(t => t.SpellContained.Base == spellBase);
        }

        public static IEnumerable<LabText> GetUnneededLabTextsFromCollection(this HermeticMagus mage)
        {
            List<LabText> unneededLabTexts = [];
            foreach (LabText labText in mage.LabTextsOwned)
            {
                bool unneeded = false;
                foreach (Spell spell in mage.SpellList)
                {
                    if (spell == labText.SpellContained)
                    {
                        unneeded = true;
                        break;
                    }
                    else if (spell.Base == labText.SpellContained.Base
                             && spell.Level > labText.SpellContained.Level)
                    {
                        unneeded = true;
                    }
                }
                if (unneeded)
                    unneededLabTexts.Add(labText);
            }
            return unneededLabTexts;
        }

        /// <summary>
        /// Determines the lifetime value of a lab text to this mage in
        /// equivalent Vim vis, based on seasons saved versus inventing
        /// the spell from scratch.
        /// </summary>
        public static double RateLifetimeLabTextValue(this HermeticMagus mage, LabText labText)
        {
            if (mage.SpellList.Any(s =>
                    s.Base == labText.SpellContained.Base
                    && s.Level >= labText.SpellContained.Level))
                return 0;

            double labTotal = mage.GetSpellLabTotal(labText.SpellContained);
            if (labTotal < labText.SpellContained.Level)
                return 0;

            double inventionPointsNeeded = labText.SpellContained.Level;
            double inventionProgressPerSeason = labTotal - labText.SpellContained.Level;

            if (inventionProgressPerSeason == 0)
                inventionProgressPerSeason = 1;

            double seasonsToInvent = Math.Ceiling(inventionPointsNeeded / inventionProgressPerSeason);
            int seasonsToLearnFromText = labText.IsShorthand ? 2 : 1;
            double seasonsSaved = seasonsToInvent - seasonsToLearnFromText;

            if (seasonsSaved <= 0) return 0;
            if (seasonsSaved > 2) seasonsSaved -= 1;

            return seasonsSaved * mage.GetVisDistillationRate();
        }

        public static bool CanUseLabText(this HermeticMagus mage, LabText text)
        {
            if (!text.IsShorthand) return true;
            if (text.Author == mage) return true;

            if (mage.DecipheredShorthandLevels.TryGetValue(text.Author, out ushort decipheredLevel))
                return text.SpellContained.Level <= decipheredLevel;

            return false;
        }

        public static ushort? GetDecipheredLabTextLevel(this HermeticMagus mage, Character author)
        {
            if (mage.DecipheredShorthandLevels.TryGetValue(author, out ushort currentProgress))
                return currentProgress;
            return null;
        }

        public static double? GetLabTextTranslationProgress(this HermeticMagus mage, LabText text)
        {
            if (mage.ShorthandTranslationProgress.TryGetValue(text, out double currentProgress))
                return currentProgress;
            return null;
        }

        public static void SetLabTextTranslationProgress(this HermeticMagus mage, LabText text, double progress)
        {
            mage.ShorthandTranslationProgress[text] = progress;
        }

        public static void AddDecipheredLabTextLevel(this HermeticMagus mage, Character author, ushort level)
        {
            foreach (var kvp in mage.ShorthandTranslationProgress.ToList())
            {
                if (kvp.Key.Author == author && kvp.Key.SpellContained.Level <= level)
                    mage.ShorthandTranslationProgress.Remove(kvp.Key);
            }
            if (!mage.DecipheredShorthandLevels.ContainsKey(author)
                || mage.DecipheredShorthandLevels[author] < level)
            {
                mage.DecipheredShorthandLevels[author] = level;
            }
        }

        public static double GetLabTextWritingRate(this HermeticMagus mage)
        {
            return mage.GetAbility(mage.WritingLanguage).Value * 20;
        }

        public static double GetLabTextCopyingRate(this HermeticMagus mage)
        {
            return mage.GetAbility(Abilities.Scribing).Value * 60;
        }
    }
}