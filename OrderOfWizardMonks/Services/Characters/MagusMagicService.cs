using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Activities;
using WizardMonks.Economy;
using WizardMonks.Instances;
using WizardMonks.Models;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Spells;

namespace WizardMonks.Services.Characters
{
    public static class MagusMagicService
    {
        public static double GetCastingTotal(this Magus mage, ArtPair artPair)
        {
            double techValue = mage.Arts.GetAbility(artPair.Technique).Value;
            double formValue = mage.Arts.GetAbility(artPair.Form).Value;
            return techValue + formValue + mage.GetAttribute(AttributeType.Stamina).Value;
        }

        public static Spell GetBestSpell(this Magus mage, SpellBase spellBase)
        {
            return mage.SpellList.Where(s => s.Base == spellBase).OrderByDescending(s => s.Level).FirstOrDefault();
        }

        public static double GetSpontaneousCastingTotal(this Magus mage, ArtPair artPair)
        {
            // TODO: make the Diedne hack better
            double divisor = mage.Name == "Diedne" ? 2.0 : 5.0;
            return mage.GetCastingTotal(artPair) / divisor;
        }

        public static double GetVisDistillationRate(this Magus mage)
        {
            // TODO: One day, we'll make this more complicated
            return mage.GetLabTotal(MagicArtPairs.CrVi, Activity.DistillVis) / 10;
        }

        public static double GetAverageAuraFound(this Magus mage)
        {
            double auraCount = mage.KnownAuras.Count;
            double areaLore = mage.GetAbility(Abilities.AreaLore).Value;
            areaLore += mage.GetCastingTotal(MagicArtPairs.InVi) / 10;
            areaLore += mage.GetAttribute(AttributeType.Perception).Value;

            double minRoll = (auraCount + 1) / areaLore;
            double multiplier = Math.Sqrt(areaLore / (auraCount + 1)) * 2 / 3;
            double areaUnder = (11.180339887498948482045868343656 - Math.Pow(minRoll, 1.5)) * multiplier;
            return areaUnder / 5;
        }

        private static void CheckTwilight()
        {
        }

        public static double GetVisCount(this Magus mage, Ability visArt)
        {
            double total = 0;
            if (mage.Covenant != null)
            {
                total += mage.Covenant.GetVis(visArt);
            }
            if (mage.VisStock.ContainsKey(visArt))
            {
                total += mage.VisStock[visArt];
            }
            return total;
        }

        public static double UseVis(this Magus mage, Ability visType, double amount)
        {
            if (!MagicArts.IsArt(visType))
            {
                throw new ArgumentException("Only magic arts have vis!");
            }
            if (mage.VisStock[visType] + (mage.Covenant == null ? 0 : mage.Covenant.GetVis(visType)) < amount)
            {
                throw new ArgumentException("Insufficient vis available!");
            }
            double covVis = mage.Covenant == null ? 0 : mage.Covenant.GetVis(visType);
            if (covVis >= amount)
            {
                mage.Covenant.RemoveVis(visType, amount);
            }
            else
            {
                if (mage.Covenant != null)
                {
                    amount -= covVis;
                    mage.Covenant.RemoveVis(visType, covVis);
                }
                mage.VisStock[visType] -= amount;
            }
            return mage.VisStock[visType];
        }

        public static void UseVis(this Magus mage, List<VisOffer> visOffers)
        {
            foreach (VisOffer offer in visOffers)
            {
                mage.UseVis(offer.Art, offer.Quantity);
            }
        }

        public static double GainVis(this Magus mage, Ability visType, double amount)
        {
            if (!MagicArts.IsArt(visType))
            {
                throw new ArgumentException("Only magic arts have vis!");
            }
            if (mage.VisStock.ContainsKey(visType))
            {
                mage.VisStock[visType] += amount;
            }
            else
            {
                mage.VisStock[visType] = amount;
            }
            return mage.VisStock[visType];
        }

        public static void GainVis(this Magus mage, List<VisOffer> visOffers)
        {
            foreach (VisOffer offer in visOffers)
            {
                mage.GainVis(offer.Art, offer.Quantity);
            }
        }

        public static bool HasSufficientVis(this Magus mage, List<VisOffer> visOffers)
        {
            foreach (VisOffer offer in visOffers)
            {
                if (mage.GetVisCount(offer.Art) < offer.Quantity)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
