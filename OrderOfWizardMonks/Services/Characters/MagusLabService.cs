using System;
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

        public static void InventSpell(this Magus mage, Spell spell)
        {
            // TODO: multiple spells in a season
            // TODO: foci
            double labTotal = mage.GetSpellLabTotal(spell);
            if (labTotal <= spell.Level)
            {
                throw new ArgumentException("This mage cannot invent this spell!");
            }

            if (spell == mage.PartialSpell)
            {
                // continue previous spell work
                mage.PartialSpellProgress += labTotal - spell.Level;
                if (mage.PartialSpellProgress >= mage.PartialSpell.Level)
                {
                    mage.LearnSpell(mage.PartialSpell);
                }
            }
            else if (labTotal >= spell.Level * 2)
            {
                mage.LearnSpell(spell);
            }
            else
            {
                mage.PartialSpell = spell;
                mage.PartialSpellProgress = labTotal - spell.Level;
            }
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

        private static void LearnSpell(this Magus mage, Spell spell)
        {
            mage.SpellList.Add(spell);
            mage.PartialSpell = null;
            mage.PartialSpellProgress = 0;
            LabText newLabText = new LabText
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
    }
}
