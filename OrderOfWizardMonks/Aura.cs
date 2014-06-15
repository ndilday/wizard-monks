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
    }
}
