using System;
using System.Collections.Generic;
using System.Linq;

namespace WizardMonks
{
    public enum Domain
    {
        Divine,
        Infernal,
        Faerie,
        Magic
    }

    [Flags]
    public enum Season
    {
        None = 0x0,
        Spring = 0x1,
        Summer = 0x2,
        Autumn = 0x4,
        Winter = 0x8
    }

    public class VisSource
    {
        public Aura Aura {get; private set;}
        public Ability Art {get; private set;}
        public Season Seasons { get; private set;}
        public double Amount {get; private set;}
        public double AnnualAmount
        {
            get
            {
                double total = 0;
                if ((Seasons & Season.Spring) == Season.Spring)
                {
                    total += Amount;
                }
                if ((Seasons & Season.Summer) == Season.Summer)
                {
                    total += Amount;
                }
                if ((Seasons & Season.Autumn) == Season.Autumn)
                {
                    total += Amount;
                }
                if ((Seasons & Season.Winter) == Season.Winter)
                {
                    total += Amount;
                }
                return total;
            }
        }

        public VisSource(Aura aura, Ability art, Season seasons, double amount)
        {
            Aura = aura;
            Art = art;
            Seasons = seasons;
            Amount = amount;
        }
    }

    public class Aura
    {
        public Domain Domain {get; private set;}
        public double Strength { get; private set; }
        public List<VisSource> VisSources { get; private set; }
        // TODO: handle regiones
        public Aura(Domain type, double strength)
        {
            Domain = type;
            Strength = strength;
            VisSources = new List<VisSource>();
        }

        public double GetAverageVisSourceSize(double magicLoreRoll)
        {
            // roll that would give current vis sources = sqrt(ml * aura) * 2/3 * 0-5 ^ 1.5
            // divide the area between that point and the max (5) by 5 to get the average gain
            // the 0-5 for the current vis count is:
            // current ^ 2 / aura * ml
            // we're adding 1 to the current vis sum because exceeding the total by < 1 will not return a vis source
            double currentVis = VisSources.Select(v => v.AnnualAmount).Sum() + 1.0;
            double currentRoll = Math.Pow(currentVis, 2) / (magicLoreRoll * Strength);
            double multiplier = Math.Sqrt(magicLoreRoll * Strength) * 2 / 3;
            double areaUnder = (11.180339887498948482045868343656 - Math.Pow(currentRoll, 1.5)) * multiplier;
            return areaUnder / 5;
        }
    }
}
