using System;
using System.Linq;
using WizardMonks.Core;
using WizardMonks.Instances;

namespace WizardMonks.Activities.ExposingActivities
{
    [Serializable]
    public class FindVisSourceActivity : AExposingActivity
    {
        public Aura Aura { get; private set; }

        public FindVisSourceActivity(Aura auraToSearch, Ability exposure, double desire) : base(exposure, desire)
        {
            Aura = auraToSearch;
            Action = Activity.FindVisSource;
        }

        protected override void DoAction(Character character)
        {
            if (character.GetType() == typeof(Magus))
            {
                character.Log.Add("Searching for a vis site in aura " + Aura.Strength.ToString("0.000"));
                Magus mage = (Magus)character;
                // add bonus to area lore equal to casting total div 5?
                // TODO: once spells are implemented, increase finding chances based on aura-detection spells
                double magicLore = mage.GetAbility(Abilities.MagicLore).Value;
                magicLore += mage.GetAttribute(AttributeType.Perception).Value;
                magicLore += mage.GetCastingTotal(MagicArtPairs.InVi) / 5;
                double roll = Die.Instance.RollDouble() * 5;

                // die roll will be 0-5; area lore will be between 0 and 25; aura will be 0-9, giving vis counts of 0-35
                double visSourceFound = Math.Sqrt(roll * magicLore * Aura.Strength);
                visSourceFound -= Aura.VisSources.Select(v => v.AnnualAmount).Sum();
                if (visSourceFound > 1.0)
                {
                    Season seasons = DetermineSeasons(ref visSourceFound);
                    Ability art = DetermineArt();
                    string logMessage = art.AbilityName + " vis source of size " + visSourceFound.ToString("0.000") + " found: ";
                    if ((seasons & Season.Spring) == Season.Spring)
                    {
                        logMessage += "Sp";
                    }
                    if ((seasons & Season.Summer) == Season.Summer)
                    {
                        logMessage += "Su";
                    }
                    if ((seasons & Season.Autumn) == Season.Autumn)
                    {
                        logMessage += "Au";
                    }
                    if ((seasons & Season.Winter) == Season.Winter)
                    {
                        logMessage += "Wi";
                    }
                    mage.Log.Add(logMessage);
                    Aura.VisSources.Add(new VisSource(Aura, art, seasons, visSourceFound));
                }
            }
        }

        private static Ability DetermineArt()
        {
            double artRoll = Die.Instance.RollDouble() * 15;
            return (int)artRoll switch
            {
                0 => MagicArts.Creo,
                1 => MagicArts.Intellego,
                2 => MagicArts.Muto,
                3 => MagicArts.Perdo,
                4 => MagicArts.Rego,
                5 => MagicArts.Animal,
                6 => MagicArts.Aquam,
                7 => MagicArts.Auram,
                8 => MagicArts.Corpus,
                9 => MagicArts.Herbam,
                10 => MagicArts.Ignem,
                11 => MagicArts.Imaginem,
                12 => MagicArts.Mentem,
                13 => MagicArts.Terram,
                _ => MagicArts.Vim,
            };
        }

        private static Season DetermineSeasons(ref double visSourceFound)
        {
            // we always want at least 1 pawn/harvest season
            // the larger the source, the more likely multiple seasons should be
            // let's call a rook of vis/season the max
            // TODO: make this math better
            int seasons;
            if (visSourceFound <= 4)
            {
                seasons = (int)(visSourceFound * Die.Instance.RollDouble());
            }
            else if (visSourceFound > 10)
            {
                seasons = 4;
            }
            else
            {
                seasons = (int)(visSourceFound / (Die.Instance.RollDouble() * 10)) + 1;
            }
            if (seasons < 1)
            {
                seasons = 1;
            }
            else if (seasons > 4)
            {
                seasons = 4;
            }
            visSourceFound /= seasons;
            switch (seasons)
            {
                case 4:
                    return Season.Spring | Season.Summer | Season.Autumn | Season.Winter;
                case 1:
                    double dieRoll = Die.Instance.RollDouble() * 4;
                    if (dieRoll < 1) return Season.Spring;
                    if (dieRoll < 2) return Season.Summer;
                    if (dieRoll < 3) return Season.Autumn;
                    return Season.Winter;
                case 3:
                    double dieRoll3 = Die.Instance.RollDouble() * 4;
                    if (dieRoll3 < 1) return Season.Summer | Season.Autumn | Season.Winter;
                    if (dieRoll3 < 2) return Season.Spring | Season.Autumn | Season.Winter;
                    if (dieRoll3 < 3) return Season.Spring | Season.Summer | Season.Winter;
                    return Season.Spring | Season.Summer | Season.Autumn;
                default:
                    double dieRoll1 = Die.Instance.RollDouble() * 4;
                    double dieRoll2;
                    do
                    {
                        dieRoll2 = Die.Instance.RollDouble() * 4;
                    } while ((int)dieRoll2 == (int)dieRoll1);
                    Season season;
                    if (dieRoll1 < 1) season = Season.Spring;
                    else if (dieRoll1 < 2) season = Season.Summer;
                    else if (dieRoll1 < 3) season = Season.Autumn;
                    else season = Season.Winter;
                    if (dieRoll2 < 1) season |= Season.Spring;
                    else if (dieRoll2 < 1) season |= Season.Summer;
                    else if (dieRoll2 < 1) season |= Season.Autumn;
                    else season |= Season.Winter;
                    return season;
            }
        }

        public override bool Matches(IActivity action)
        {
            return action.Action == Activity.FindVisSource && ((FindVisSourceActivity)action).Aura == Aura;
        }

        public override string Log()
        {
            return "Finding vis source worth " + Desire.ToString("0.000");
        }
    }

}
