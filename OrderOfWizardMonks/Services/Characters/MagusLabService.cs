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
        public static double GetLabTotal(this Magus mage, ArtPair artPair, Activity activity)
        {
            double magicTheory = mage.GetAbility(mage.MagicAbility).Value;
            double techValue = mage.Arts.GetAbility(artPair.Technique).Value;
            double formValue = mage.Arts.GetAbility(artPair.Form).Value;
            double labTotal = magicTheory + techValue + formValue + mage.GetAttribute(AttributeType.Intelligence).Value;
            if (mage.Covenant != null)
            {
                labTotal += mage.Covenant.Aura.Strength;

                if (mage.Laboratory != null)
                {
                    labTotal += mage.Laboratory.GetModifier(artPair, activity);
                }
                if (mage.Apprentice != null)
                {
                    labTotal += mage.Apprentice.GetAbility(mage.MagicAbility).Value + mage.Apprentice.GetAttributeValue(AttributeType.Intelligence);
                }
            }

            //TODO: foci
            //TODO: lab assistant
            //TODO: familiar
            return labTotal;
        }

        public static double GetSpellLabTotal(this Magus mage, Spell spell)
        {
            double total = mage.GetLabTotal(spell.Base.ArtPair, Activity.InventSpells);
            // see if the mage knows a sell with the same base effect
            Spell similarSpell = mage.GetBestSpell(spell.Base);
            if (similarSpell != null)
            {
                // if so, add the level of that spell to the lab total
                total += similarSpell.Level / 5.0;
            }
            return total;
        }

        public static void BuildLaboratory(this Magus mage)
        {
            // TODO: flesh out laboratory specialization
            mage.Laboratory = new Laboratory(mage, mage.Covenant.Aura, 0);
        }

        public static void BuildLaboratory(this Magus mage, Aura aura)
        {
            mage.Laboratory = new Laboratory(mage, aura, 0);
        }

        public static void RefineLaboratory(this Magus mage)
        {
            if (mage.Laboratory == null)
            {
                throw new NullReferenceException("The mage has no laboratory!");
            }
            if (mage.GetAbility(mage.MagicAbility).Value - 4 < mage.Laboratory.Refinement)
            {
                throw new ArgumentOutOfRangeException("The mage's magical understanding is not high enough to refine this laboratory any further.");
            }
            mage.Laboratory.Refine();
        }

        public static void AddFeatureToLaboratory(this Magus mage, LabFeature feature)
        {
            if (mage.Laboratory == null)
            {
                throw new NullReferenceException("The mage has no laboratory!");
            }
            // TODO: Implement
        }

        public static void ExtractVis(this Magus mage, Ability exposureAbility)
        {
            // add vis to personal inventory or covenant inventory
            mage.VisStock[MagicArts.Vim] += mage.GetVisDistillationRate();

            // grant exposure experience
            mage.GetAbility(exposureAbility).AddExperience(2);
        }

        public static void LearnSpellFromLabText(this Magus mage, LabText text)
        {
            // TODO: multiple spells in a season
            // TODO: foci
            Spell spell = text.SpellContained;
            double labTotal = mage.GetSpellLabTotal(spell);
            if (labTotal < spell.Level)
            {
                throw new ArgumentException("This mage cannot invent this spell!");
            }
            else
            {
                mage.LearnSpell(spell);
                foreach (var belief in text.BeliefPayload)
                {
                    // Update belief about the author
                    mage.GetBeliefProfile(text.Author).AddOrUpdateBelief(new Belief(belief.Topic, belief.Magnitude));

                    // Update stereotype about the author's house
                    // TODO: should sterotypes really apply to arts or attributes?
                    if (text.Author is Magus magus)
                    {
                        var houseSubject = Houses.GetSubject(magus.House);
                        mage.GetBeliefProfile(houseSubject).AddOrUpdateBelief(new Belief(belief.Topic, belief.Magnitude * 0.20)); // Stereotype is 20% strength
                    }
                }
            }
        }

        public static void LearnSpell(this Magus mage, Spell spell)
        {
            mage.SpellList.Add(spell);
            LabText newLabText = new()
            {
                Author = mage,
                IsShorthand = true,
                SpellContained = spell
            };
            // Generate Belief Payload for the shorthand lab text
            double magnitude = spell.Level / 5.0;
            newLabText.BeliefPayload.Add(new Belief(spell.Base.ArtPair.Technique.AbilityName, magnitude));
            newLabText.BeliefPayload.Add(new Belief(spell.Base.ArtPair.Form.AbilityName, magnitude));

            // Generate personality beliefs based on spell tags
            mage.LabTextsOwned.Add(newLabText);

        }

        public static IEnumerable<LabText> GetLabTextsFromCollection(this Magus mage, SpellBase spellBase)
        {
            return mage.LabTextsOwned.Where(t => t.SpellContained.Base == spellBase);
        }

        

        public static IEnumerable<LabText> GetUnneededLabTextsFromCollection(this Magus mage)
        {
            List<LabText> unneededLabTexts = [];
            foreach (LabText labText in mage.LabTextsOwned)
            {
                bool unneeded = false;
                foreach (Spell spell in mage.SpellList)
                {
                    // if the mage already knows the spell, the lab text is unneeded
                    if (spell == labText.SpellContained)
                    {
                        unneeded = true;
                        break;
                    }
                    // if the mage already knows a better version of the spell, the lab text is unneeded
                    else if (spell.Base == labText.SpellContained.Base && spell.Level > labText.SpellContained.Level)
                    {
                        unneeded = true;
                    }
                }
                if (unneeded)
                {
                    unneededLabTexts.Add(labText);
                }

            }
            return unneededLabTexts;
        }

        /// <summary>
        /// Determines the value of a given lab text to this magus in an equivalent pawn-value of Vim vis.
        /// The value is based on the number of seasons the magus would save by learning from the text
        /// instead of inventing the spell from scratch.
        /// </summary>
        /// <param name="labText">The lab text to evaluate.</param>
        /// <returns>The value of the lab text in pawns of Vim vis. Returns 0 if the text is unusable or not beneficial.</returns>
        public static double RateLifetimeLabTextValue(this Magus mage, LabText labText)
        {
            // If we already know this spell or a better version of it, the text has no value.
            if (mage.SpellList.Any(s => s.Base == labText.SpellContained.Base && s.Level >= labText.SpellContained.Level))
            {
                return 0;
            }

            // To learn from a lab text, the magus's Lab Total must be greater than the spell's level.
            double labTotal = mage.GetSpellLabTotal(labText.SpellContained);
            if (labTotal < labText.SpellContained.Level)
            {
                return 0;
            }

            // --- Step 2: Calculate the seasons required to invent the spell from scratch ---

            // Invention requires accumulating 'Level' points of progress.
            double inventionPointsNeeded = labText.SpellContained.Level;

            // Progress per season is the amount the Lab Total exceeds the spell's level.
            double inventionProgressPerSeason = labTotal - labText.SpellContained.Level;

            // This case should be caught by the labTotal check above, but as a safeguard against division by zero.
            if (inventionProgressPerSeason == 0)
            {
                inventionProgressPerSeason = 1;
            }

            // Calculate how many full seasons it would take to gain the required points.
            // A fraction of a season's work still consumes the entire season.
            double seasonsToInvent = Math.Ceiling(inventionPointsNeeded / inventionProgressPerSeason);

            // --- Step 3: Calculate seasons saved and convert to vis value ---

            // Learning from a lab text takes a single season.
            int seasonsToLearnFromText = labText.IsShorthand ? 2 : 1;
            double seasonsSaved = seasonsToInvent - seasonsToLearnFromText;

            // If it takes 1 or fewer seasons to invent, the lab text provides no time savings and has no value.
            if (seasonsSaved <= 0)
            {
                return 0;
            }
            if (seasonsSaved > 2)
            {
                // decrement by one to account for how the inventing mage's Magic Theory will increase along the way
                seasonsSaved -= 1;
            }

            // The value of a saved season is equivalent to the amount of Vim vis that could be distilled in that time.
            // This serves as our universal "opportunity cost" currency for the AI.
            double visDistilledPerSeason = mage.GetVisDistillationRate();
            double visValue = seasonsSaved * visDistilledPerSeason;

            return visValue;
        }

        public static bool CanUseLabText(this Magus mage, LabText text)
        {
            if (!text.IsShorthand)
            {
                return true; // Not shorthand, so it's usable by anyone with Magic Theory.
            }

            // If it's our own shorthand, we can always use it.
            if (text.Author == mage)
            {
                return true;
            }

            // Check if we've deciphered this author's shorthand to a sufficient level.
            if (mage.DecipheredShorthandLevels.TryGetValue(text.Author, out ushort decipheredLevel))
            {
                return text.SpellContained.Level <= decipheredLevel;
            }

            return false; // We have no understanding of this author's shorthand.
        }

        public static ushort? GetDeciperedLabTextLevel(this Magus mage, Character author)
        {
            if (mage.DecipheredShorthandLevels.TryGetValue(author, out ushort currentProgress)) return currentProgress;
            return null;
        }

        public static double? GetLabTextTranslationProgress(this Magus mage, LabText text)
        {
            if (mage.ShorthandTranslationProgress.TryGetValue(text, out double currentProgress)) return currentProgress;
            return null;
        }

        public static void SetLabTextTranslationProgress(this Magus mage, LabText text, double progress)
        {
            mage.ShorthandTranslationProgress[text] = progress;
        }

        public static void AddDecipheredLabTextLevel(this Magus mage, Character author, ushort level)
        {
            // remove any partial translations of this author of this level or below
            foreach (var kvp in mage.ShorthandTranslationProgress)
            {
                if (kvp.Key.Author == author && kvp.Key.SpellContained.Level <= level)
                {
                    mage.ShorthandTranslationProgress.Remove(kvp.Key);
                }
            }
            if (!mage.DecipheredShorthandLevels.ContainsKey(author) || mage.DecipheredShorthandLevels[author] < level)
            {
                mage.DecipheredShorthandLevels[author] = level;
            }
        }

        public static double GetLabTextWritingRate(this Magus mage)
        {
            // Latin skill * 20 levels per season
            return mage.GetAbility(mage.WritingLanguage).Value * 20;
        }

        public static double GetLabTextCopyingRate(this Magus mage)
        {
            // Profession: Scribe skill * 60 levels per season
            return mage.GetAbility(Abilities.Scribing).Value * 60;
        }
    }
}
