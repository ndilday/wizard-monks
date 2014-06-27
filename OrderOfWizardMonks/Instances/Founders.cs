using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks.Instances
{
    public static class Founders
    {
        public static Magus Bjornaer { get; private set; }
        public static Magus Bonisgaus { get; private set; }
        public static Magus Criamon { get; private set; }
        public static Magus Diedne { get; private set; }
        public static Magus Flambeau { get; private set; }
        public static Magus Guernicus { get; private set; }
        public static Magus Jerbiton { get; private set; }
        public static Magus Mercere { get; private set; }
        public static Magus Merinita { get; private set; }
        public static Magus Tremere { get; private set; }
        public static Magus Tytalus { get; private set; }
        public static Magus Verditius { get; private set; }

        public static IEnumerable<Magus> GetEnumerator()
        {
            yield return Bjornaer;
            yield return Bonisgaus;
            yield return Criamon;
            yield return Diedne;
            yield return Flambeau;
            yield return Guernicus;
            yield return Jerbiton;
            yield return Mercere;
            yield return Merinita;
            yield return Tremere;
            yield return Tytalus;
            yield return Verditius;
        }

        static Founders()
        {
            BuildBjornaer();
            BuildBonisagus();
            BuildCriamon();
            BuildDiedne();
            BuildFlambeau();
            BuildGuernicus();
            BuildJerbiton();
            BuildMercere();
            BuildMerinita();
            BuildTremere();
            BuildTytalus();
            BuildVerditius();
        }

        public static void BuildBjornaer()
        {
            AbilityScoreCondition goal = new AbilityScoreCondition(MagicArts.Animal, 20, 1);
            List<IGoal> goals = new List<IGoal>();
            goals.Add(goal);

            HasApprenticeCondition apprentice = new HasApprenticeCondition();
            goals.Add(apprentice);

            goal = new AbilityScoreCondition(Abilities.BjornaerLore, 5, 1.01);
            goals.Add(goal);

            goal = new AbilityScoreCondition(Abilities.Heartbeast, 5, 1.02);

            Bjornaer = new Magus(Abilities.MagicTheory, Abilities.Latin, Abilities.ArtesLiberales, Abilities.AreaLore, goals, 80);
            Bjornaer.Name = "Bjornaer";

            Bjornaer.GetAttribute(AttributeType.Stamina).BaseValue = 3;
            Bjornaer.GetAttribute(AttributeType.Strength).BaseValue = 1;
            Bjornaer.GetAttribute(AttributeType.Dexterity).BaseValue = -2;
            Bjornaer.GetAttribute(AttributeType.Quickness).BaseValue = -2;
            Bjornaer.GetAttribute(AttributeType.Intelligence).BaseValue = 2;
            Bjornaer.GetAttribute(AttributeType.Communication).BaseValue = 1;
            Bjornaer.GetAttribute(AttributeType.Presence).BaseValue = 2;
            Bjornaer.GetAttribute(AttributeType.Perception).BaseValue = -1;

            Bjornaer.GetAbility(MagicArts.Creo).AddExperience(0);
            Bjornaer.GetAbility(MagicArts.Intellego).AddExperience(0);
            Bjornaer.GetAbility(MagicArts.Muto).AddExperience(15);
            Bjornaer.GetAbility(MagicArts.Perdo).AddExperience(0);
            Bjornaer.GetAbility(MagicArts.Rego).AddExperience(0);
            Bjornaer.GetAbility(MagicArts.Animal).AddExperience(15);
            Bjornaer.GetAbility(MagicArts.Aquam).AddExperience(0);
            Bjornaer.GetAbility(MagicArts.Auram).AddExperience(0);
            Bjornaer.GetAbility(MagicArts.Corpus).AddExperience(15);
            Bjornaer.GetAbility(MagicArts.Herbam).AddExperience(0);
            Bjornaer.GetAbility(MagicArts.Ignem).AddExperience(0);
            Bjornaer.GetAbility(MagicArts.Imaginem).AddExperience(0);
            Bjornaer.GetAbility(MagicArts.Mentem).AddExperience(0);
            Bjornaer.GetAbility(MagicArts.Terram).AddExperience(0);
            Bjornaer.GetAbility(MagicArts.Vim).AddExperience(0);
            Bjornaer.GetAbility(Abilities.AreaLore).AddExperience(45);
            Bjornaer.GetAbility(Abilities.ArtesLiberales).AddExperience(5);
            Bjornaer.GetAbility(Abilities.English).AddExperience(75);
            Bjornaer.GetAbility(Abilities.Etiquette).AddExperience(0);
            Bjornaer.GetAbility(Abilities.Latin).AddExperience(50);
            Bjornaer.GetAbility(Abilities.MagicTheory).AddExperience(30);
            Bjornaer.GetAbility(Abilities.ParmaMagica).AddExperience(5);
            Bjornaer.GetAbility(Abilities.Penetration).AddExperience(0);
            Bjornaer.GetAbility(Abilities.Concentration).AddExperience(0);
            Bjornaer.GetAbility(Abilities.Heartbeast).AddExperience(30);
            Bjornaer.GetAbility(Abilities.BjornaerLore).AddExperience(75);
        }

        public static void BuildBonisagus()
        {
            AbilityScoreCondition goal = new AbilityScoreCondition(Abilities.MagicTheory, 10, 1);
            List<IGoal> goals = new List<IGoal>();
            goals.Add(goal);

            HasApprenticeCondition apprentice = new HasApprenticeCondition();
            goals.Add(goal);

            Bonisgaus = new Magus(Abilities.MagicTheory, Abilities.Latin, Abilities.ArtesLiberales, Abilities.AreaLore, goals, 80);
            Bonisgaus.Name = "Bonisagus";

            Bonisgaus.GetAttribute(AttributeType.Stamina).BaseValue = 1;
            Bonisgaus.GetAttribute(AttributeType.Strength).BaseValue = -2;
            Bonisgaus.GetAttribute(AttributeType.Dexterity).BaseValue = -2;
            Bonisgaus.GetAttribute(AttributeType.Quickness).BaseValue = -2;
            Bonisgaus.GetAttribute(AttributeType.Intelligence).BaseValue = 5;
            Bonisgaus.GetAttribute(AttributeType.Communication).BaseValue = 2;
            Bonisgaus.GetAttribute(AttributeType.Presence).BaseValue = -2;
            Bonisgaus.GetAttribute(AttributeType.Perception).BaseValue = 0;

            Bonisgaus.GetAbility(MagicArts.Creo).AddExperience(1);
            Bonisgaus.GetAbility(MagicArts.Intellego).AddExperience(1);
            Bonisgaus.GetAbility(MagicArts.Muto).AddExperience(1);
            Bonisgaus.GetAbility(MagicArts.Perdo).AddExperience(1);
            Bonisgaus.GetAbility(MagicArts.Rego).AddExperience(1);
            Bonisgaus.GetAbility(MagicArts.Animal).AddExperience(0);
            Bonisgaus.GetAbility(MagicArts.Aquam).AddExperience(0);
            Bonisgaus.GetAbility(MagicArts.Auram).AddExperience(0);
            Bonisgaus.GetAbility(MagicArts.Corpus).AddExperience(0);
            Bonisgaus.GetAbility(MagicArts.Herbam).AddExperience(0);
            Bonisgaus.GetAbility(MagicArts.Ignem).AddExperience(0);
            Bonisgaus.GetAbility(MagicArts.Imaginem).AddExperience(0);
            Bonisgaus.GetAbility(MagicArts.Mentem).AddExperience(0);
            Bonisgaus.GetAbility(MagicArts.Terram).AddExperience(0);
            Bonisgaus.GetAbility(MagicArts.Vim).AddExperience(55);
            Bonisgaus.GetAbility(Abilities.AreaLore).AddExperience(0);
            Bonisgaus.GetAbility(Abilities.ArtesLiberales).AddExperience(15);
            Bonisgaus.GetAbility(Abilities.English).AddExperience(75);
            Bonisgaus.GetAbility(Abilities.Etiquette).AddExperience(45);
            Bonisgaus.GetAbility(Abilities.Latin).AddExperience(75);
            Bonisgaus.GetAbility(Abilities.MagicTheory).AddExperience(75);
            Bonisgaus.GetAbility(Abilities.ParmaMagica).AddExperience(5);
            Bonisgaus.GetAbility(Abilities.Penetration).AddExperience(5);
            Bonisgaus.GetAbility(Abilities.Concentration).AddExperience(5);
        }

        public static void BuildCriamon()
        {
            AbilityScoreCondition goal = new AbilityScoreCondition(Abilities.MagicTheory, 5, 1);
            List<IGoal> goals = new List<IGoal>();
            goals.Add(goal);
            
            goal = new AbilityScoreCondition(Abilities.EnigmaticWisdom, 5, 1);
            goals.Add(goal);

            goal = new AbilityScoreCondition(Abilities.CriamonLore, 5, 1);
            goals.Add(goal);

            HasApprenticeCondition app = new HasApprenticeCondition();
            goals.Add(app);

            Criamon = new Magus(Abilities.MagicTheory, Abilities.Latin, Abilities.ArtesLiberales, Abilities.AreaLore, goals, 80);
            Criamon.Name = "Criamon";

            Criamon.GetAttribute(AttributeType.Stamina).BaseValue = 1;
            Criamon.GetAttribute(AttributeType.Strength).BaseValue = -2;
            Criamon.GetAttribute(AttributeType.Dexterity).BaseValue = -2;
            Criamon.GetAttribute(AttributeType.Quickness).BaseValue = 1;
            Criamon.GetAttribute(AttributeType.Intelligence).BaseValue = 2;
            Criamon.GetAttribute(AttributeType.Communication).BaseValue = -1;
            Criamon.GetAttribute(AttributeType.Presence).BaseValue = 3;
            Criamon.GetAttribute(AttributeType.Perception).BaseValue = 2;

            Criamon.GetAbility(MagicArts.Creo).AddExperience(0);
            Criamon.GetAbility(MagicArts.Intellego).AddExperience(15);
            Criamon.GetAbility(MagicArts.Muto).AddExperience(0);
            Criamon.GetAbility(MagicArts.Perdo).AddExperience(0);
            Criamon.GetAbility(MagicArts.Rego).AddExperience(0);
            Criamon.GetAbility(MagicArts.Animal).AddExperience(0);
            Criamon.GetAbility(MagicArts.Aquam).AddExperience(0);
            Criamon.GetAbility(MagicArts.Auram).AddExperience(0);
            Criamon.GetAbility(MagicArts.Corpus).AddExperience(0);
            Criamon.GetAbility(MagicArts.Herbam).AddExperience(0);
            Criamon.GetAbility(MagicArts.Ignem).AddExperience(0);
            Criamon.GetAbility(MagicArts.Imaginem).AddExperience(0);
            Criamon.GetAbility(MagicArts.Mentem).AddExperience(0);
            Criamon.GetAbility(MagicArts.Terram).AddExperience(0);
            Criamon.GetAbility(MagicArts.Vim).AddExperience(15);
            Criamon.GetAbility(Abilities.AreaLore).AddExperience(30);
            Criamon.GetAbility(Abilities.ArtesLiberales).AddExperience(30);
            Criamon.GetAbility(Abilities.English).AddExperience(75);
            Criamon.GetAbility(Abilities.Etiquette).AddExperience(15);
            Criamon.GetAbility(Abilities.Latin).AddExperience(50);
            Criamon.GetAbility(Abilities.MagicTheory).AddExperience(50);
            Criamon.GetAbility(Abilities.ParmaMagica).AddExperience(5);
            Criamon.GetAbility(Abilities.Penetration).AddExperience(0);
            Criamon.GetAbility(Abilities.Concentration).AddExperience(30);
            Criamon.GetAbility(Abilities.EnigmaticWisdom).AddExperience(15);
            Criamon.GetAbility(Abilities.CriamonLore).AddExperience(30);
        }

        public static void BuildDiedne()
        {
            AbilityScoreCondition goal = new AbilityScoreCondition(MagicArts.Creo, 5, 5);
            List<IGoal> goals = new List<IGoal>();
            goals.Add(goal);

            goal = new AbilityScoreCondition(MagicArts.Intellego, 5, 1.05);
            goals.Add(goal);

            goal = new AbilityScoreCondition(MagicArts.Muto, 5, 1.05);
            goals.Add(goal);

            goal = new AbilityScoreCondition(MagicArts.Perdo, 5, 1.05);
            goals.Add(goal);

            goal = new AbilityScoreCondition(MagicArts.Rego, 5, 1.05);
            goals.Add(goal);

            goal = new AbilityScoreCondition(MagicArts.Animal, 5, 1.05);
            goals.Add(goal);

            goal = new AbilityScoreCondition(MagicArts.Aquam, 5, 1);
            goals.Add(goal);

            goal = new AbilityScoreCondition(MagicArts.Corpus, 5, 1);
            goals.Add(goal);

            goal = new AbilityScoreCondition(MagicArts.Herbam, 5, 1);
            goals.Add(goal);

            goal = new AbilityScoreCondition(MagicArts.Ignem, 5, 1);
            goals.Add(goal);

            goal = new AbilityScoreCondition(MagicArts.Imaginem, 5, 1);
            goals.Add(goal);

            goal = new AbilityScoreCondition(MagicArts.Mentem, 5, 1);
            goals.Add(goal);

            goal = new AbilityScoreCondition(MagicArts.Terram, 5, 1);
            goals.Add(goal);

            goal = new AbilityScoreCondition(MagicArts.Vim, 5, 1);
            goals.Add(goal);

            HasApprenticeCondition app = new HasApprenticeCondition();
            goals.Add(app);

            Diedne = new Magus(Abilities.MagicTheory, Abilities.Latin, Abilities.ArtesLiberales, Abilities.AreaLore, goals, 80);
            Diedne.Name = "Diedne";

            Diedne.GetAttribute(AttributeType.Stamina).BaseValue = 1;
            Diedne.GetAttribute(AttributeType.Strength).BaseValue = -2;
            Diedne.GetAttribute(AttributeType.Dexterity).BaseValue = 0;
            Diedne.GetAttribute(AttributeType.Quickness).BaseValue = 0;
            Diedne.GetAttribute(AttributeType.Intelligence).BaseValue = 1;
            Diedne.GetAttribute(AttributeType.Communication).BaseValue = 1;
            Diedne.GetAttribute(AttributeType.Presence).BaseValue = 3;
            Diedne.GetAttribute(AttributeType.Perception).BaseValue = 1;

            Diedne.GetAbility(MagicArts.Creo).AddExperience(12);
            Diedne.GetAbility(MagicArts.Intellego).AddExperience(12);
            Diedne.GetAbility(MagicArts.Muto).AddExperience(12);
            Diedne.GetAbility(MagicArts.Perdo).AddExperience(12);
            Diedne.GetAbility(MagicArts.Rego).AddExperience(12);
            Diedne.GetAbility(MagicArts.Animal).AddExperience(6);
            Diedne.GetAbility(MagicArts.Aquam).AddExperience(6);
            Diedne.GetAbility(MagicArts.Auram).AddExperience(6);
            Diedne.GetAbility(MagicArts.Corpus).AddExperience(6);
            Diedne.GetAbility(MagicArts.Herbam).AddExperience(6);
            Diedne.GetAbility(MagicArts.Ignem).AddExperience(6);
            Diedne.GetAbility(MagicArts.Imaginem).AddExperience(6);
            Diedne.GetAbility(MagicArts.Mentem).AddExperience(6);
            Diedne.GetAbility(MagicArts.Terram).AddExperience(6);
            Diedne.GetAbility(MagicArts.Vim).AddExperience(6);
            Diedne.GetAbility(Abilities.AreaLore).AddExperience(30);
            Diedne.GetAbility(Abilities.ArtesLiberales).AddExperience(0);
            Diedne.GetAbility(Abilities.English).AddExperience(75);
            Diedne.GetAbility(Abilities.Etiquette).AddExperience(15);
            Diedne.GetAbility(Abilities.Latin).AddExperience(0);
            Diedne.GetAbility(Abilities.MagicTheory).AddExperience(30);
            Diedne.GetAbility(Abilities.ParmaMagica).AddExperience(30);
            Diedne.GetAbility(Abilities.Penetration).AddExperience(30);
            Diedne.GetAbility(Abilities.Concentration).AddExperience(30);
        }

        public static void BuildFlambeau()
        {
            AbilityScoreCondition goal = new AbilityScoreCondition(MagicArts.Ignem, 20, 1);
            List<IGoal> goals = new List<IGoal>();
            goals.Add(goal);

            goal = new AbilityScoreCondition(Abilities.Penetration, 5, 1);
            goals.Add(goal);

            goal = new AbilityScoreCondition(Abilities.ParmaMagica, 5, 1);
            goals.Add(goal);

            HasApprenticeCondition app = new HasApprenticeCondition();
            goals.Add(app);

            Flambeau = new Magus(Abilities.MagicTheory, Abilities.Latin, Abilities.ArtesLiberales, Abilities.AreaLore, goals, 80);
            Flambeau.Name = "Flambeau";

            Flambeau.GetAttribute(AttributeType.Stamina).BaseValue = 2;
            Flambeau.GetAttribute(AttributeType.Strength).BaseValue = 2;
            Flambeau.GetAttribute(AttributeType.Dexterity).BaseValue = 2;
            Flambeau.GetAttribute(AttributeType.Quickness).BaseValue = 0;
            Flambeau.GetAttribute(AttributeType.Intelligence).BaseValue = 1;
            Flambeau.GetAttribute(AttributeType.Communication).BaseValue = -2;
            Flambeau.GetAttribute(AttributeType.Presence).BaseValue = 2;
            Flambeau.GetAttribute(AttributeType.Perception).BaseValue = -2;

            Flambeau.GetAbility(MagicArts.Creo).AddExperience(3);
            Flambeau.GetAbility(MagicArts.Intellego).AddExperience(0);
            Flambeau.GetAbility(MagicArts.Muto).AddExperience(0);
            Flambeau.GetAbility(MagicArts.Perdo).AddExperience(3);
            Flambeau.GetAbility(MagicArts.Rego).AddExperience(3);
            Flambeau.GetAbility(MagicArts.Animal).AddExperience(0);
            Flambeau.GetAbility(MagicArts.Aquam).AddExperience(0);
            Flambeau.GetAbility(MagicArts.Auram).AddExperience(0);
            Flambeau.GetAbility(MagicArts.Corpus).AddExperience(0);
            Flambeau.GetAbility(MagicArts.Herbam).AddExperience(0);
            Flambeau.GetAbility(MagicArts.Ignem).AddExperience(21);
            Flambeau.GetAbility(MagicArts.Imaginem).AddExperience(0);
            Flambeau.GetAbility(MagicArts.Mentem).AddExperience(0);
            Flambeau.GetAbility(MagicArts.Terram).AddExperience(0);
            Flambeau.GetAbility(MagicArts.Vim).AddExperience(0);
            Flambeau.GetAbility(Abilities.AreaLore).AddExperience(15);
            Flambeau.GetAbility(Abilities.ArtesLiberales).AddExperience(15);
            Flambeau.GetAbility(Abilities.English).AddExperience(75);
            Flambeau.GetAbility(Abilities.Etiquette).AddExperience(30);
            Flambeau.GetAbility(Abilities.Latin).AddExperience(75);
            Flambeau.GetAbility(Abilities.MagicTheory).AddExperience(30);
            Flambeau.GetAbility(Abilities.ParmaMagica).AddExperience(30);
            Flambeau.GetAbility(Abilities.Penetration).AddExperience(30);
            Flambeau.GetAbility(Abilities.Concentration).AddExperience(30);
        }

        public static void BuildGuernicus()
        {
            AbilityScoreCondition goal = new AbilityScoreCondition(MagicArts.Terram, 20, 1);
            List<IGoal> goals = new List<IGoal>();
            goals.Add(goal);

            goal = new AbilityScoreCondition(Abilities.CodeOfHermes, 5, 1.5);
            goals.Add(goal);

            goal = new AbilityScoreCondition(Abilities.MagicTheory, 5, 1);
            goals.Add(goal);

            HasApprenticeCondition app = new HasApprenticeCondition();
            goals.Add(app);

            Guernicus = new Magus(Abilities.MagicTheory, Abilities.Latin, Abilities.ArtesLiberales, Abilities.AreaLore, goals, 80);
            Guernicus.Name = "Guernicus";

            Guernicus.GetAttribute(AttributeType.Stamina).BaseValue = 2;
            Guernicus.GetAttribute(AttributeType.Strength).BaseValue = 0;
            Guernicus.GetAttribute(AttributeType.Dexterity).BaseValue = -1;
            Guernicus.GetAttribute(AttributeType.Quickness).BaseValue = -2;
            Guernicus.GetAttribute(AttributeType.Intelligence).BaseValue = 2;
            Guernicus.GetAttribute(AttributeType.Communication).BaseValue = 2;
            Guernicus.GetAttribute(AttributeType.Presence).BaseValue = 1;
            Guernicus.GetAttribute(AttributeType.Perception).BaseValue = 1;

            Guernicus.GetAbility(MagicArts.Creo).AddExperience(0);
            Guernicus.GetAbility(MagicArts.Intellego).AddExperience(15);
            Guernicus.GetAbility(MagicArts.Muto).AddExperience(0);
            Guernicus.GetAbility(MagicArts.Perdo).AddExperience(0);
            Guernicus.GetAbility(MagicArts.Rego).AddExperience(0);
            Guernicus.GetAbility(MagicArts.Animal).AddExperience(0);
            Guernicus.GetAbility(MagicArts.Aquam).AddExperience(0);
            Guernicus.GetAbility(MagicArts.Auram).AddExperience(0);
            Guernicus.GetAbility(MagicArts.Corpus).AddExperience(0);
            Guernicus.GetAbility(MagicArts.Herbam).AddExperience(0);
            Guernicus.GetAbility(MagicArts.Ignem).AddExperience(0);
            Guernicus.GetAbility(MagicArts.Imaginem).AddExperience(0);
            Guernicus.GetAbility(MagicArts.Mentem).AddExperience(0);
            Guernicus.GetAbility(MagicArts.Terram).AddExperience(55);
            Guernicus.GetAbility(MagicArts.Vim).AddExperience(0);
            Guernicus.GetAbility(Abilities.AreaLore).AddExperience(15);
            Guernicus.GetAbility(Abilities.ArtesLiberales).AddExperience(30);
            Guernicus.GetAbility(Abilities.English).AddExperience(75);
            Guernicus.GetAbility(Abilities.Etiquette).AddExperience(30);
            Guernicus.GetAbility(Abilities.Latin).AddExperience(75);
            Guernicus.GetAbility(Abilities.MagicTheory).AddExperience(50);
            Guernicus.GetAbility(Abilities.ParmaMagica).AddExperience(5);
            Guernicus.GetAbility(Abilities.CodeOfHermes).AddExperience(5);
            Guernicus.GetAbility(Abilities.Concentration).AddExperience(5);
        }

        public static void BuildJerbiton()
        {
            AbilityScoreCondition goal = new AbilityScoreCondition(MagicArts.Imaginem, 20, 1);
            List<IGoal> goals = new List<IGoal>();
            goals.Add(goal);

            goal = new AbilityScoreCondition(Abilities.ArtesLiberales, 5, 1);
            goals.Add(goal);

            goal = new AbilityScoreCondition(Abilities.Etiquette, 5, 1);
            goals.Add(goal);

            goal = new AbilityScoreCondition(Abilities.Finesse, 5, 1.01);
            goals.Add(goal);

            HasApprenticeCondition app = new HasApprenticeCondition();
            goals.Add(app);

            Jerbiton = new Magus(Abilities.MagicTheory, Abilities.Latin, Abilities.ArtesLiberales, Abilities.AreaLore, goals, 80);
            Jerbiton.Name = "Jerbiton";

            Jerbiton.GetAttribute(AttributeType.Stamina).BaseValue = -1;
            Jerbiton.GetAttribute(AttributeType.Strength).BaseValue = -2;
            Jerbiton.GetAttribute(AttributeType.Dexterity).BaseValue = 0;
            Jerbiton.GetAttribute(AttributeType.Quickness).BaseValue = -1;
            Jerbiton.GetAttribute(AttributeType.Intelligence).BaseValue = 2;
            Jerbiton.GetAttribute(AttributeType.Communication).BaseValue = 2;
            Jerbiton.GetAttribute(AttributeType.Presence).BaseValue = 2;
            Jerbiton.GetAttribute(AttributeType.Perception).BaseValue = 2;

            Jerbiton.GetAbility(MagicArts.Creo).AddExperience(3);
            Jerbiton.GetAbility(MagicArts.Intellego).AddExperience(1);
            Jerbiton.GetAbility(MagicArts.Muto).AddExperience(3);
            Jerbiton.GetAbility(MagicArts.Perdo).AddExperience(0);
            Jerbiton.GetAbility(MagicArts.Rego).AddExperience(3);
            Jerbiton.GetAbility(MagicArts.Animal).AddExperience(0);
            Jerbiton.GetAbility(MagicArts.Aquam).AddExperience(0);
            Jerbiton.GetAbility(MagicArts.Auram).AddExperience(0);
            Jerbiton.GetAbility(MagicArts.Corpus).AddExperience(0);
            Jerbiton.GetAbility(MagicArts.Herbam).AddExperience(0);
            Jerbiton.GetAbility(MagicArts.Ignem).AddExperience(0);
            Jerbiton.GetAbility(MagicArts.Imaginem).AddExperience(55);
            Jerbiton.GetAbility(MagicArts.Mentem).AddExperience(0);
            Jerbiton.GetAbility(MagicArts.Terram).AddExperience(0);
            Jerbiton.GetAbility(MagicArts.Vim).AddExperience(0);
            Jerbiton.GetAbility(Abilities.AreaLore).AddExperience(15);
            Jerbiton.GetAbility(Abilities.ArtesLiberales).AddExperience(50);
            Jerbiton.GetAbility(Abilities.English).AddExperience(75);
            Jerbiton.GetAbility(Abilities.Etiquette).AddExperience(30);
            Jerbiton.GetAbility(Abilities.Latin).AddExperience(75);
            Jerbiton.GetAbility(Abilities.MagicTheory).AddExperience(30);
            Jerbiton.GetAbility(Abilities.ParmaMagica).AddExperience(5);
            Jerbiton.GetAbility(Abilities.Penetration).AddExperience(0);
            Jerbiton.GetAbility(Abilities.Finesse).AddExperience(15);
        }

        public static void BuildMercere()
        {
            AbilityScoreCondition goal = new AbilityScoreCondition(MagicArts.Mentem, 20, 1);
            List<IGoal> goals = new List<IGoal>();
            goals.Add(goal);

            goal = new AbilityScoreCondition(Abilities.Etiquette, 5, 5);
            goals.Add(goal);

            HasApprenticeCondition app = new HasApprenticeCondition();
            goals.Add(app);

            Mercere = new Magus(Abilities.MagicTheory, Abilities.Latin, Abilities.ArtesLiberales, Abilities.AreaLore, goals, 80);
            Mercere.Name = "Mercere";

            Mercere.GetAttribute(AttributeType.Stamina).BaseValue = 1;
            Mercere.GetAttribute(AttributeType.Strength).BaseValue = 1;
            Mercere.GetAttribute(AttributeType.Dexterity).BaseValue = 1;
            Mercere.GetAttribute(AttributeType.Quickness).BaseValue = 1;
            Mercere.GetAttribute(AttributeType.Intelligence).BaseValue = 2;
            Mercere.GetAttribute(AttributeType.Communication).BaseValue = 2;
            Mercere.GetAttribute(AttributeType.Presence).BaseValue = -3;
            Mercere.GetAttribute(AttributeType.Perception).BaseValue = 2;

            Mercere.GetAbility(MagicArts.Creo).AddExperience(15);
            Mercere.GetAbility(MagicArts.Intellego).AddExperience(6);
            Mercere.GetAbility(MagicArts.Muto).AddExperience(15);
            Mercere.GetAbility(MagicArts.Perdo).AddExperience(3);
            Mercere.GetAbility(MagicArts.Rego).AddExperience(15);
            Mercere.GetAbility(MagicArts.Animal).AddExperience(0);
            Mercere.GetAbility(MagicArts.Aquam).AddExperience(0);
            Mercere.GetAbility(MagicArts.Auram).AddExperience(0);
            Mercere.GetAbility(MagicArts.Corpus).AddExperience(0);
            Mercere.GetAbility(MagicArts.Herbam).AddExperience(0);
            Mercere.GetAbility(MagicArts.Ignem).AddExperience(0);
            Mercere.GetAbility(MagicArts.Imaginem).AddExperience(0);
            Mercere.GetAbility(MagicArts.Mentem).AddExperience(55);
            Mercere.GetAbility(MagicArts.Terram).AddExperience(0);
            Mercere.GetAbility(MagicArts.Vim).AddExperience(1);
            Mercere.GetAbility(Abilities.AreaLore).AddExperience(30);
            Mercere.GetAbility(Abilities.ArtesLiberales).AddExperience(30);
            Mercere.GetAbility(Abilities.English).AddExperience(75);
            Mercere.GetAbility(Abilities.Etiquette).AddExperience(30);
            Mercere.GetAbility(Abilities.Latin).AddExperience(50);
            Mercere.GetAbility(Abilities.MagicTheory).AddExperience(30);
            Mercere.GetAbility(Abilities.ParmaMagica).AddExperience(5);
            Mercere.GetAbility(Abilities.Penetration).AddExperience(0);
            Mercere.GetAbility(Abilities.Concentration).AddExperience(0);
        }

        public static void BuildMerinita()
        {
            AbilityScoreCondition goal = new AbilityScoreCondition(MagicArts.Herbam, 20, 1);
            List<IGoal> goals = new List<IGoal>();
            goals.Add(goal);

            goal = new AbilityScoreCondition(MagicArts.Animal, 15, 1);
            goals.Add(goal);

            goal = new AbilityScoreCondition(MagicArts.Creo, 5, 1);
            goals.Add(goal);

            goal = new AbilityScoreCondition(MagicArts.Intellego, 5, 1);
            goals.Add(goal);

            goal = new AbilityScoreCondition(MagicArts.Muto, 5, 1);
            goals.Add(goal);

            goal = new AbilityScoreCondition(MagicArts.Perdo, 5, 1);
            goals.Add(goal);

            goal = new AbilityScoreCondition(MagicArts.Rego, 5, 1);
            goals.Add(goal);

            Merinita = new Magus(Abilities.MagicTheory, Abilities.Latin, Abilities.ArtesLiberales, Abilities.AreaLore, goals, 80);
            Merinita.Name = "Merinita";

            Merinita.GetAttribute(AttributeType.Stamina).BaseValue = 0;
            Merinita.GetAttribute(AttributeType.Strength).BaseValue = -1;
            Merinita.GetAttribute(AttributeType.Dexterity).BaseValue = 0;
            Merinita.GetAttribute(AttributeType.Quickness).BaseValue = 0;
            Merinita.GetAttribute(AttributeType.Intelligence).BaseValue = 1;
            Merinita.GetAttribute(AttributeType.Communication).BaseValue = 1;
            Merinita.GetAttribute(AttributeType.Presence).BaseValue = 3;
            Merinita.GetAttribute(AttributeType.Perception).BaseValue = 0;

            Merinita.GetAbility(MagicArts.Creo).AddExperience(10);
            Merinita.GetAbility(MagicArts.Intellego).AddExperience(15);
            Merinita.GetAbility(MagicArts.Muto).AddExperience(15);
            Merinita.GetAbility(MagicArts.Perdo).AddExperience(10);
            Merinita.GetAbility(MagicArts.Rego).AddExperience(10);
            Merinita.GetAbility(MagicArts.Animal).AddExperience(0);
            Merinita.GetAbility(MagicArts.Aquam).AddExperience(0);
            Merinita.GetAbility(MagicArts.Auram).AddExperience(0);
            Merinita.GetAbility(MagicArts.Corpus).AddExperience(0);
            Merinita.GetAbility(MagicArts.Herbam).AddExperience(55);
            Merinita.GetAbility(MagicArts.Ignem).AddExperience(0);
            Merinita.GetAbility(MagicArts.Imaginem).AddExperience(0);
            Merinita.GetAbility(MagicArts.Mentem).AddExperience(0);
            Merinita.GetAbility(MagicArts.Terram).AddExperience(15);
            Merinita.GetAbility(MagicArts.Vim).AddExperience(0);
            Merinita.GetAbility(Abilities.AreaLore).AddExperience(15);
            Merinita.GetAbility(Abilities.ArtesLiberales).AddExperience(5);
            Merinita.GetAbility(Abilities.English).AddExperience(75);
            Merinita.GetAbility(Abilities.Etiquette).AddExperience(15);
            Merinita.GetAbility(Abilities.Latin).AddExperience(50);
            Merinita.GetAbility(Abilities.MagicTheory).AddExperience(15);
            Merinita.GetAbility(Abilities.ParmaMagica).AddExperience(5);
            Merinita.GetAbility(Abilities.Penetration).AddExperience(0);
            Merinita.GetAbility(Abilities.MerinitaLore).AddExperience(50);
        }

        public static void BuildTremere()
        {
            IGoal goal = new HasApprenticeCondition(5, 0);
            List<IGoal> goals = new List<IGoal>();
            goals.Add(goal);

            goal = new AbilityScoreCondition(Abilities.Penetration, 5, 1);
            goals.Add(goal);

            goal = new AbilityScoreCondition(Abilities.ParmaMagica, 5, 1);
            goals.Add(goal);

            Tremere = new Magus(Abilities.MagicTheory, Abilities.Latin, Abilities.ArtesLiberales, Abilities.AreaLore, goals, 80);
            Tremere.Name = "Tremere";

            Tremere.GetAttribute(AttributeType.Stamina).BaseValue = 0;
            Tremere.GetAttribute(AttributeType.Strength).BaseValue = 0;
            Tremere.GetAttribute(AttributeType.Dexterity).BaseValue = 0;
            Tremere.GetAttribute(AttributeType.Quickness).BaseValue = 1;
            Tremere.GetAttribute(AttributeType.Intelligence).BaseValue = 2;
            Tremere.GetAttribute(AttributeType.Communication).BaseValue = 1;
            Tremere.GetAttribute(AttributeType.Presence).BaseValue = -1;
            Tremere.GetAttribute(AttributeType.Perception).BaseValue = 2;

            Tremere.GetAbility(MagicArts.Creo).AddExperience(0);
            Tremere.GetAbility(MagicArts.Intellego).AddExperience(0);
            Tremere.GetAbility(MagicArts.Muto).AddExperience(0);
            Tremere.GetAbility(MagicArts.Perdo).AddExperience(0);
            Tremere.GetAbility(MagicArts.Rego).AddExperience(0);
            Tremere.GetAbility(MagicArts.Animal).AddExperience(0);
            Tremere.GetAbility(MagicArts.Aquam).AddExperience(0);
            Tremere.GetAbility(MagicArts.Auram).AddExperience(0);
            Tremere.GetAbility(MagicArts.Corpus).AddExperience(0);
            Tremere.GetAbility(MagicArts.Herbam).AddExperience(0);
            Tremere.GetAbility(MagicArts.Ignem).AddExperience(0);
            Tremere.GetAbility(MagicArts.Imaginem).AddExperience(0);
            Tremere.GetAbility(MagicArts.Mentem).AddExperience(0);
            Tremere.GetAbility(MagicArts.Terram).AddExperience(0);
            Tremere.GetAbility(MagicArts.Vim).AddExperience(0);
            Tremere.GetAbility(Abilities.AreaLore).AddExperience(0);
            Tremere.GetAbility(Abilities.ArtesLiberales).AddExperience(15);
            Tremere.GetAbility(Abilities.English).AddExperience(75);
            Tremere.GetAbility(Abilities.Etiquette).AddExperience(5);
            Tremere.GetAbility(Abilities.Latin).AddExperience(75);
            Tremere.GetAbility(Abilities.MagicTheory).AddExperience(30);
            Tremere.GetAbility(Abilities.ParmaMagica).AddExperience(30);
            Tremere.GetAbility(Abilities.Penetration).AddExperience(30);
            Tremere.GetAbility(Abilities.Concentration).AddExperience(0);
        }

        public static void BuildTytalus()
        {
            AbilityScoreCondition goal = new AbilityScoreCondition(MagicArts.Rego, 20, 1);
            List<IGoal> goals = new List<IGoal>();
            goals.Add(goal);

            goal = new AbilityScoreCondition(MagicArts.Mentem, 10, 1);
            goals.Add(goal);

            goal = new AbilityScoreCondition(Abilities.Penetration, 5, 1);
            goals.Add(goal);

            goal = new AbilityScoreCondition(Abilities.Finesse, 5, 1);
            goals.Add(goal);

            HasApprenticeCondition app = new HasApprenticeCondition();
            goals.Add(app);

            Tytalus = new Magus(Abilities.MagicTheory, Abilities.Latin, Abilities.ArtesLiberales, Abilities.AreaLore, goals, 80);
            Tytalus.Name = "Tytalus";

            Tytalus.GetAttribute(AttributeType.Stamina).BaseValue = 3;
            Tytalus.GetAttribute(AttributeType.Strength).BaseValue = 0;
            Tytalus.GetAttribute(AttributeType.Dexterity).BaseValue = 0;
            Tytalus.GetAttribute(AttributeType.Quickness).BaseValue = -1;
            Tytalus.GetAttribute(AttributeType.Intelligence).BaseValue = 2;
            Tytalus.GetAttribute(AttributeType.Communication).BaseValue = -2;
            Tytalus.GetAttribute(AttributeType.Presence).BaseValue = -1;
            Tytalus.GetAttribute(AttributeType.Perception).BaseValue = 2;

            Tytalus.GetAbility(MagicArts.Creo).AddExperience(0);
            Tytalus.GetAbility(MagicArts.Intellego).AddExperience(0);
            Tytalus.GetAbility(MagicArts.Muto).AddExperience(10);
            Tytalus.GetAbility(MagicArts.Perdo).AddExperience(15);
            Tytalus.GetAbility(MagicArts.Rego).AddExperience(55);
            Tytalus.GetAbility(MagicArts.Animal).AddExperience(0);
            Tytalus.GetAbility(MagicArts.Aquam).AddExperience(0);
            Tytalus.GetAbility(MagicArts.Auram).AddExperience(0);
            Tytalus.GetAbility(MagicArts.Corpus).AddExperience(0);
            Tytalus.GetAbility(MagicArts.Herbam).AddExperience(0);
            Tytalus.GetAbility(MagicArts.Ignem).AddExperience(0);
            Tytalus.GetAbility(MagicArts.Imaginem).AddExperience(0);
            Tytalus.GetAbility(MagicArts.Mentem).AddExperience(0);
            Tytalus.GetAbility(MagicArts.Terram).AddExperience(0);
            Tytalus.GetAbility(MagicArts.Vim).AddExperience(0);
            Tytalus.GetAbility(Abilities.AreaLore).AddExperience(30);
            Tytalus.GetAbility(Abilities.ArtesLiberales).AddExperience(5);
            Tytalus.GetAbility(Abilities.English).AddExperience(75);
            Tytalus.GetAbility(Abilities.Etiquette).AddExperience(15);
            Tytalus.GetAbility(Abilities.Latin).AddExperience(50);
            Tytalus.GetAbility(Abilities.MagicTheory).AddExperience(30);
            Tytalus.GetAbility(Abilities.ParmaMagica).AddExperience(30);
            Tytalus.GetAbility(Abilities.Penetration).AddExperience(30);
            Tytalus.GetAbility(Abilities.Concentration).AddExperience(15);
        }

        public static void BuildVerditius()
        {
            AbilityScoreCondition goal = new AbilityScoreCondition(MagicArts.Intellego, 5, 1);
            List<IGoal> goals = new List<IGoal>();
            goals.Add(goal);

            goal = new AbilityScoreCondition(MagicArts.Muto, 5, 1);
            goals.Add(goal);

            goal = new AbilityScoreCondition(MagicArts.Perdo, 5, 1);
            goals.Add(goal);

            goal = new AbilityScoreCondition(MagicArts.Rego, 5, 1);
            goals.Add(goal);

            goal = new AbilityScoreCondition(MagicArts.Terram, 20, 1);
            goals.Add(goal);

            goal = new AbilityScoreCondition(Abilities.Philosophae, 4, 1);
            goals.Add(goal);

            goal = new AbilityScoreCondition(Abilities.Craft, 5, 1);
            goals.Add(goal);

            HasApprenticeCondition app = new HasApprenticeCondition();
            goals.Add(app);

            Verditius = new Magus(Abilities.MagicTheory, Abilities.Latin, Abilities.ArtesLiberales, Abilities.AreaLore, goals, 80);
            Verditius.Name = "Verditius";

            Verditius.GetAttribute(AttributeType.Stamina).BaseValue = 1;
            Verditius.GetAttribute(AttributeType.Strength).BaseValue = 0;
            Verditius.GetAttribute(AttributeType.Dexterity).BaseValue = 2;
            Verditius.GetAttribute(AttributeType.Quickness).BaseValue = -3;
            Verditius.GetAttribute(AttributeType.Intelligence).BaseValue = 3;
            Verditius.GetAttribute(AttributeType.Communication).BaseValue = -2;
            Verditius.GetAttribute(AttributeType.Presence).BaseValue = 2;
            Verditius.GetAttribute(AttributeType.Perception).BaseValue = 2;

            Verditius.GetAbility(MagicArts.Creo).AddExperience(15);
            Verditius.GetAbility(MagicArts.Intellego).AddExperience(0);
            Verditius.GetAbility(MagicArts.Muto).AddExperience(0);
            Verditius.GetAbility(MagicArts.Perdo).AddExperience(0);
            Verditius.GetAbility(MagicArts.Rego).AddExperience(15);
            Verditius.GetAbility(MagicArts.Animal).AddExperience(0);
            Verditius.GetAbility(MagicArts.Aquam).AddExperience(0);
            Verditius.GetAbility(MagicArts.Auram).AddExperience(0);
            Verditius.GetAbility(MagicArts.Corpus).AddExperience(0);
            Verditius.GetAbility(MagicArts.Herbam).AddExperience(0);
            Verditius.GetAbility(MagicArts.Ignem).AddExperience(0);
            Verditius.GetAbility(MagicArts.Imaginem).AddExperience(0);
            Verditius.GetAbility(MagicArts.Mentem).AddExperience(0);
            Verditius.GetAbility(MagicArts.Terram).AddExperience(15);
            Verditius.GetAbility(MagicArts.Vim).AddExperience(15);
            Verditius.GetAbility(Abilities.AreaLore).AddExperience(15);
            Verditius.GetAbility(Abilities.ArtesLiberales).AddExperience(30);
            Verditius.GetAbility(Abilities.English).AddExperience(75);
            Verditius.GetAbility(Abilities.Etiquette).AddExperience(30);
            Verditius.GetAbility(Abilities.Latin).AddExperience(75);
            Verditius.GetAbility(Abilities.MagicTheory).AddExperience(30);
            Verditius.GetAbility(Abilities.ParmaMagica).AddExperience(5);
            Verditius.GetAbility(Abilities.Penetration).AddExperience(5);
            Verditius.GetAbility(Abilities.Craft).AddExperience(30);
            Verditius.GetAbility(Abilities.Philosophae).AddExperience(5);
        }
    }
}
