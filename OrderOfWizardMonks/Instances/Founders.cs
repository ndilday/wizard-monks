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

        private static Dictionary<Preference, double> BuildBasicPreferences()
        {
            Dictionary<Preference, double> preferences = new Dictionary<Preference, double>();
            preferences[Preferences.VisDesire] = 6.5;
            preferences[new Preference(PreferenceType.Ability, Abilities.MagicTheory)] = .5;

            return preferences;
        }

        public static void BuildBjornaer()
        {
            Bjornaer = new Magus(Abilities.MagicTheory, Abilities.Latin, Abilities.ArtesLiberales, Abilities.AreaLore, BuildBasicPreferences());
            Bjornaer.Name = "Bjornaer";
            Bjornaer.GetAbility(MagicArts.Creo).AddExperience(0);
            Bjornaer.GetAbility(MagicArts.Intellego).AddExperience(0);
            Bjornaer.GetAbility(MagicArts.Muto).AddExperience(0);
            Bjornaer.GetAbility(MagicArts.Perdo).AddExperience(0);
            Bjornaer.GetAbility(MagicArts.Rego).AddExperience(0);
            Bjornaer.GetAbility(MagicArts.Animal).AddExperience(0);
            Bjornaer.GetAbility(MagicArts.Aquam).AddExperience(0);
            Bjornaer.GetAbility(MagicArts.Auram).AddExperience(0);
            Bjornaer.GetAbility(MagicArts.Corpus).AddExperience(0);
            Bjornaer.GetAbility(MagicArts.Herbam).AddExperience(0);
            Bjornaer.GetAbility(MagicArts.Ignem).AddExperience(0);
            Bjornaer.GetAbility(MagicArts.Imaginem).AddExperience(0);
            Bjornaer.GetAbility(MagicArts.Mentem).AddExperience(0);
            Bjornaer.GetAbility(MagicArts.Terram).AddExperience(0);
            Bjornaer.GetAbility(MagicArts.Vim).AddExperience(0);
            Bjornaer.GetAbility(Abilities.AreaLore).AddExperience(0);
            Bjornaer.GetAbility(Abilities.ArtesLiberales).AddExperience(0);
            Bjornaer.GetAbility(Abilities.English).AddExperience(75);
            Bjornaer.GetAbility(Abilities.Etiquette).AddExperience(5);
            Bjornaer.GetAbility(Abilities.Latin).AddExperience(0);
            Bjornaer.GetAbility(Abilities.MagicTheory).AddExperience(0);
            Bjornaer.GetAbility(Abilities.ParmaMagica).AddExperience(0);
            Bjornaer.GetAbility(Abilities.Penetration).AddExperience(0);
            Bjornaer.GetAbility(Abilities.Concentration).AddExperience(0);
        }

        public static void BuildBonisagus()
        {
            Bonisgaus = new Magus(Abilities.MagicTheory, Abilities.Latin, Abilities.ArtesLiberales, Abilities.AreaLore, BuildBasicPreferences());
            Bonisgaus.Name = "Bonisagus";
            Bonisgaus.GetAbility(MagicArts.Creo).AddExperience(0);
            Bonisgaus.GetAbility(MagicArts.Intellego).AddExperience(0);
            Bonisgaus.GetAbility(MagicArts.Muto).AddExperience(0);
            Bonisgaus.GetAbility(MagicArts.Perdo).AddExperience(0);
            Bonisgaus.GetAbility(MagicArts.Rego).AddExperience(0);
            Bonisgaus.GetAbility(MagicArts.Animal).AddExperience(0);
            Bonisgaus.GetAbility(MagicArts.Aquam).AddExperience(0);
            Bonisgaus.GetAbility(MagicArts.Auram).AddExperience(0);
            Bonisgaus.GetAbility(MagicArts.Corpus).AddExperience(0);
            Bonisgaus.GetAbility(MagicArts.Herbam).AddExperience(0);
            Bonisgaus.GetAbility(MagicArts.Ignem).AddExperience(0);
            Bonisgaus.GetAbility(MagicArts.Imaginem).AddExperience(0);
            Bonisgaus.GetAbility(MagicArts.Mentem).AddExperience(0);
            Bonisgaus.GetAbility(MagicArts.Terram).AddExperience(0);
            Bonisgaus.GetAbility(MagicArts.Vim).AddExperience(0);
            Bonisgaus.GetAbility(Abilities.AreaLore).AddExperience(0);
            Bonisgaus.GetAbility(Abilities.ArtesLiberales).AddExperience(0);
            Bonisgaus.GetAbility(Abilities.English).AddExperience(75);
            Bonisgaus.GetAbility(Abilities.Etiquette).AddExperience(5);
            Bonisgaus.GetAbility(Abilities.Latin).AddExperience(0);
            Bonisgaus.GetAbility(Abilities.MagicTheory).AddExperience(0);
            Bonisgaus.GetAbility(Abilities.ParmaMagica).AddExperience(0);
            Bonisgaus.GetAbility(Abilities.Penetration).AddExperience(0);
            Bonisgaus.GetAbility(Abilities.Concentration).AddExperience(0);
        }

        public static void BuildCriamon()
        {
            Criamon = new Magus(Abilities.MagicTheory, Abilities.Latin, Abilities.ArtesLiberales, Abilities.AreaLore, BuildBasicPreferences());
            Criamon.Name = "Criamon";
            Criamon.GetAbility(MagicArts.Creo).AddExperience(0);
            Criamon.GetAbility(MagicArts.Intellego).AddExperience(0);
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
            Criamon.GetAbility(MagicArts.Vim).AddExperience(0);
            Criamon.GetAbility(Abilities.AreaLore).AddExperience(0);
            Criamon.GetAbility(Abilities.ArtesLiberales).AddExperience(0);
            Criamon.GetAbility(Abilities.English).AddExperience(75);
            Criamon.GetAbility(Abilities.Etiquette).AddExperience(5);
            Criamon.GetAbility(Abilities.Latin).AddExperience(0);
            Criamon.GetAbility(Abilities.MagicTheory).AddExperience(0);
            Criamon.GetAbility(Abilities.ParmaMagica).AddExperience(0);
            Criamon.GetAbility(Abilities.Penetration).AddExperience(0);
            Criamon.GetAbility(Abilities.Concentration).AddExperience(0);
        }

        public static void BuildDiedne()
        {
            Diedne = new Magus(Abilities.MagicTheory, Abilities.Latin, Abilities.ArtesLiberales, Abilities.AreaLore, BuildBasicPreferences());
            Diedne.Name = "Diedne";
            Diedne.GetAbility(MagicArts.Creo).AddExperience(0);
            Diedne.GetAbility(MagicArts.Intellego).AddExperience(0);
            Diedne.GetAbility(MagicArts.Muto).AddExperience(0);
            Diedne.GetAbility(MagicArts.Perdo).AddExperience(0);
            Diedne.GetAbility(MagicArts.Rego).AddExperience(0);
            Diedne.GetAbility(MagicArts.Animal).AddExperience(0);
            Diedne.GetAbility(MagicArts.Aquam).AddExperience(0);
            Diedne.GetAbility(MagicArts.Auram).AddExperience(0);
            Diedne.GetAbility(MagicArts.Corpus).AddExperience(0);
            Diedne.GetAbility(MagicArts.Herbam).AddExperience(0);
            Diedne.GetAbility(MagicArts.Ignem).AddExperience(0);
            Diedne.GetAbility(MagicArts.Imaginem).AddExperience(0);
            Diedne.GetAbility(MagicArts.Mentem).AddExperience(0);
            Diedne.GetAbility(MagicArts.Terram).AddExperience(0);
            Diedne.GetAbility(MagicArts.Vim).AddExperience(0);
            Diedne.GetAbility(Abilities.AreaLore).AddExperience(0);
            Diedne.GetAbility(Abilities.ArtesLiberales).AddExperience(0);
            Diedne.GetAbility(Abilities.English).AddExperience(75);
            Diedne.GetAbility(Abilities.Etiquette).AddExperience(5);
            Diedne.GetAbility(Abilities.Latin).AddExperience(0);
            Diedne.GetAbility(Abilities.MagicTheory).AddExperience(0);
            Diedne.GetAbility(Abilities.ParmaMagica).AddExperience(0);
            Diedne.GetAbility(Abilities.Penetration).AddExperience(0);
            Diedne.GetAbility(Abilities.Concentration).AddExperience(0);
        }

        public static void BuildFlambeau()
        {
            Flambeau = new Magus(Abilities.MagicTheory, Abilities.Latin, Abilities.ArtesLiberales, Abilities.AreaLore, BuildBasicPreferences());
            Flambeau.Name = "Flambeau";
            Flambeau.GetAbility(MagicArts.Creo).AddExperience(0);
            Flambeau.GetAbility(MagicArts.Intellego).AddExperience(0);
            Flambeau.GetAbility(MagicArts.Muto).AddExperience(0);
            Flambeau.GetAbility(MagicArts.Perdo).AddExperience(0);
            Flambeau.GetAbility(MagicArts.Rego).AddExperience(0);
            Flambeau.GetAbility(MagicArts.Animal).AddExperience(0);
            Flambeau.GetAbility(MagicArts.Aquam).AddExperience(0);
            Flambeau.GetAbility(MagicArts.Auram).AddExperience(0);
            Flambeau.GetAbility(MagicArts.Corpus).AddExperience(0);
            Flambeau.GetAbility(MagicArts.Herbam).AddExperience(0);
            Flambeau.GetAbility(MagicArts.Ignem).AddExperience(0);
            Flambeau.GetAbility(MagicArts.Imaginem).AddExperience(0);
            Flambeau.GetAbility(MagicArts.Mentem).AddExperience(0);
            Flambeau.GetAbility(MagicArts.Terram).AddExperience(0);
            Flambeau.GetAbility(MagicArts.Vim).AddExperience(0);
            Flambeau.GetAbility(Abilities.AreaLore).AddExperience(0);
            Flambeau.GetAbility(Abilities.ArtesLiberales).AddExperience(0);
            Flambeau.GetAbility(Abilities.English).AddExperience(75);
            Flambeau.GetAbility(Abilities.Etiquette).AddExperience(5);
            Flambeau.GetAbility(Abilities.Latin).AddExperience(0);
            Flambeau.GetAbility(Abilities.MagicTheory).AddExperience(0);
            Flambeau.GetAbility(Abilities.ParmaMagica).AddExperience(0);
            Flambeau.GetAbility(Abilities.Penetration).AddExperience(0);
            Flambeau.GetAbility(Abilities.Concentration).AddExperience(0);
        }

        public static void BuildGuernicus()
        {
            Guernicus = new Magus(Abilities.MagicTheory, Abilities.Latin, Abilities.ArtesLiberales, Abilities.AreaLore, BuildBasicPreferences());
            Guernicus.Name = "Guernicus";
            Guernicus.GetAbility(MagicArts.Creo).AddExperience(0);
            Guernicus.GetAbility(MagicArts.Intellego).AddExperience(0);
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
            Guernicus.GetAbility(MagicArts.Terram).AddExperience(0);
            Guernicus.GetAbility(MagicArts.Vim).AddExperience(0);
            Guernicus.GetAbility(Abilities.AreaLore).AddExperience(0);
            Guernicus.GetAbility(Abilities.ArtesLiberales).AddExperience(0);
            Guernicus.GetAbility(Abilities.English).AddExperience(75);
            Guernicus.GetAbility(Abilities.Etiquette).AddExperience(5);
            Guernicus.GetAbility(Abilities.Latin).AddExperience(0);
            Guernicus.GetAbility(Abilities.MagicTheory).AddExperience(0);
            Guernicus.GetAbility(Abilities.ParmaMagica).AddExperience(0);
            Guernicus.GetAbility(Abilities.Penetration).AddExperience(0);
            Guernicus.GetAbility(Abilities.Concentration).AddExperience(0);
        }

        public static void BuildJerbiton()
        {
            Jerbiton = new Magus(Abilities.MagicTheory, Abilities.Latin, Abilities.ArtesLiberales, Abilities.AreaLore, BuildBasicPreferences());
            Jerbiton.Name = "Jerbiton";
            Jerbiton.GetAbility(MagicArts.Creo).AddExperience(0);
            Jerbiton.GetAbility(MagicArts.Intellego).AddExperience(0);
            Jerbiton.GetAbility(MagicArts.Muto).AddExperience(0);
            Jerbiton.GetAbility(MagicArts.Perdo).AddExperience(0);
            Jerbiton.GetAbility(MagicArts.Rego).AddExperience(0);
            Jerbiton.GetAbility(MagicArts.Animal).AddExperience(0);
            Jerbiton.GetAbility(MagicArts.Aquam).AddExperience(0);
            Jerbiton.GetAbility(MagicArts.Auram).AddExperience(0);
            Jerbiton.GetAbility(MagicArts.Corpus).AddExperience(0);
            Jerbiton.GetAbility(MagicArts.Herbam).AddExperience(0);
            Jerbiton.GetAbility(MagicArts.Ignem).AddExperience(0);
            Jerbiton.GetAbility(MagicArts.Imaginem).AddExperience(0);
            Jerbiton.GetAbility(MagicArts.Mentem).AddExperience(0);
            Jerbiton.GetAbility(MagicArts.Terram).AddExperience(0);
            Jerbiton.GetAbility(MagicArts.Vim).AddExperience(0);
            Jerbiton.GetAbility(Abilities.AreaLore).AddExperience(0);
            Jerbiton.GetAbility(Abilities.ArtesLiberales).AddExperience(0);
            Jerbiton.GetAbility(Abilities.English).AddExperience(75);
            Jerbiton.GetAbility(Abilities.Etiquette).AddExperience(5);
            Jerbiton.GetAbility(Abilities.Latin).AddExperience(0);
            Jerbiton.GetAbility(Abilities.MagicTheory).AddExperience(0);
            Jerbiton.GetAbility(Abilities.ParmaMagica).AddExperience(0);
            Jerbiton.GetAbility(Abilities.Penetration).AddExperience(0);
            Jerbiton.GetAbility(Abilities.Concentration).AddExperience(0);
        }

        public static void BuildMercere()
        {
            Mercere = new Magus(Abilities.MagicTheory, Abilities.Latin, Abilities.ArtesLiberales, Abilities.AreaLore, BuildBasicPreferences());
            Mercere.Name = "Mercere";
            Mercere.GetAbility(MagicArts.Creo).AddExperience(0);
            Mercere.GetAbility(MagicArts.Intellego).AddExperience(0);
            Mercere.GetAbility(MagicArts.Muto).AddExperience(0);
            Mercere.GetAbility(MagicArts.Perdo).AddExperience(0);
            Mercere.GetAbility(MagicArts.Rego).AddExperience(0);
            Mercere.GetAbility(MagicArts.Animal).AddExperience(0);
            Mercere.GetAbility(MagicArts.Aquam).AddExperience(0);
            Mercere.GetAbility(MagicArts.Auram).AddExperience(0);
            Mercere.GetAbility(MagicArts.Corpus).AddExperience(0);
            Mercere.GetAbility(MagicArts.Herbam).AddExperience(0);
            Mercere.GetAbility(MagicArts.Ignem).AddExperience(0);
            Mercere.GetAbility(MagicArts.Imaginem).AddExperience(0);
            Mercere.GetAbility(MagicArts.Mentem).AddExperience(0);
            Mercere.GetAbility(MagicArts.Terram).AddExperience(0);
            Mercere.GetAbility(MagicArts.Vim).AddExperience(0);
            Mercere.GetAbility(Abilities.AreaLore).AddExperience(0);
            Mercere.GetAbility(Abilities.ArtesLiberales).AddExperience(0);
            Mercere.GetAbility(Abilities.English).AddExperience(75);
            Mercere.GetAbility(Abilities.Etiquette).AddExperience(5);
            Mercere.GetAbility(Abilities.Latin).AddExperience(0);
            Mercere.GetAbility(Abilities.MagicTheory).AddExperience(0);
            Mercere.GetAbility(Abilities.ParmaMagica).AddExperience(0);
            Mercere.GetAbility(Abilities.Penetration).AddExperience(0);
            Mercere.GetAbility(Abilities.Concentration).AddExperience(0);
        }

        public static void BuildMerinita()
        {
            Merinita = new Magus(Abilities.MagicTheory, Abilities.Latin, Abilities.ArtesLiberales, Abilities.AreaLore, BuildBasicPreferences());
            Merinita.Name = "Merinita";
            Merinita.GetAbility(MagicArts.Creo).AddExperience(0);
            Merinita.GetAbility(MagicArts.Intellego).AddExperience(0);
            Merinita.GetAbility(MagicArts.Muto).AddExperience(0);
            Merinita.GetAbility(MagicArts.Perdo).AddExperience(0);
            Merinita.GetAbility(MagicArts.Rego).AddExperience(0);
            Merinita.GetAbility(MagicArts.Animal).AddExperience(0);
            Merinita.GetAbility(MagicArts.Aquam).AddExperience(0);
            Merinita.GetAbility(MagicArts.Auram).AddExperience(0);
            Merinita.GetAbility(MagicArts.Corpus).AddExperience(0);
            Merinita.GetAbility(MagicArts.Herbam).AddExperience(0);
            Merinita.GetAbility(MagicArts.Ignem).AddExperience(0);
            Merinita.GetAbility(MagicArts.Imaginem).AddExperience(0);
            Merinita.GetAbility(MagicArts.Mentem).AddExperience(0);
            Merinita.GetAbility(MagicArts.Terram).AddExperience(0);
            Merinita.GetAbility(MagicArts.Vim).AddExperience(0);
            Merinita.GetAbility(Abilities.AreaLore).AddExperience(0);
            Merinita.GetAbility(Abilities.ArtesLiberales).AddExperience(0);
            Merinita.GetAbility(Abilities.English).AddExperience(75);
            Merinita.GetAbility(Abilities.Etiquette).AddExperience(5);
            Merinita.GetAbility(Abilities.Latin).AddExperience(0);
            Merinita.GetAbility(Abilities.MagicTheory).AddExperience(0);
            Merinita.GetAbility(Abilities.ParmaMagica).AddExperience(0);
            Merinita.GetAbility(Abilities.Penetration).AddExperience(0);
            Merinita.GetAbility(Abilities.Concentration).AddExperience(0);
        }

        public static void BuildTremere()
        {
            Tremere = new Magus(Abilities.MagicTheory, Abilities.Latin, Abilities.ArtesLiberales, Abilities.AreaLore, BuildBasicPreferences());
            Tremere.Name = "Tremere";
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
            Tremere.GetAbility(Abilities.ArtesLiberales).AddExperience(0);
            Tremere.GetAbility(Abilities.English).AddExperience(75);
            Tremere.GetAbility(Abilities.Etiquette).AddExperience(5);
            Tremere.GetAbility(Abilities.Latin).AddExperience(0);
            Tremere.GetAbility(Abilities.MagicTheory).AddExperience(0);
            Tremere.GetAbility(Abilities.ParmaMagica).AddExperience(0);
            Tremere.GetAbility(Abilities.Penetration).AddExperience(0);
            Tremere.GetAbility(Abilities.Concentration).AddExperience(0);
        }

        public static void BuildTytalus()
        {
            Tytalus = new Magus(Abilities.MagicTheory, Abilities.Latin, Abilities.ArtesLiberales, Abilities.AreaLore, BuildBasicPreferences());
            Tytalus.Name = "Tytalus";
            Tytalus.GetAbility(MagicArts.Creo).AddExperience(0);
            Tytalus.GetAbility(MagicArts.Intellego).AddExperience(0);
            Tytalus.GetAbility(MagicArts.Muto).AddExperience(0);
            Tytalus.GetAbility(MagicArts.Perdo).AddExperience(0);
            Tytalus.GetAbility(MagicArts.Rego).AddExperience(0);
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
            Tytalus.GetAbility(Abilities.AreaLore).AddExperience(0);
            Tytalus.GetAbility(Abilities.ArtesLiberales).AddExperience(0);
            Tytalus.GetAbility(Abilities.English).AddExperience(75);
            Tytalus.GetAbility(Abilities.Etiquette).AddExperience(5);
            Tytalus.GetAbility(Abilities.Latin).AddExperience(0);
            Tytalus.GetAbility(Abilities.MagicTheory).AddExperience(0);
            Tytalus.GetAbility(Abilities.ParmaMagica).AddExperience(0);
            Tytalus.GetAbility(Abilities.Penetration).AddExperience(0);
            Tytalus.GetAbility(Abilities.Concentration).AddExperience(0);
        }

        public static void BuildVerditius()
        {
            Verditius = new Magus(Abilities.MagicTheory, Abilities.Latin, Abilities.ArtesLiberales, Abilities.AreaLore, BuildBasicPreferences());
            Verditius.Name = "Verditius";
            Verditius.GetAbility(MagicArts.Creo).AddExperience(0);
            Verditius.GetAbility(MagicArts.Intellego).AddExperience(0);
            Verditius.GetAbility(MagicArts.Muto).AddExperience(0);
            Verditius.GetAbility(MagicArts.Perdo).AddExperience(0);
            Verditius.GetAbility(MagicArts.Rego).AddExperience(0);
            Verditius.GetAbility(MagicArts.Animal).AddExperience(0);
            Verditius.GetAbility(MagicArts.Aquam).AddExperience(0);
            Verditius.GetAbility(MagicArts.Auram).AddExperience(0);
            Verditius.GetAbility(MagicArts.Corpus).AddExperience(0);
            Verditius.GetAbility(MagicArts.Herbam).AddExperience(0);
            Verditius.GetAbility(MagicArts.Ignem).AddExperience(0);
            Verditius.GetAbility(MagicArts.Imaginem).AddExperience(0);
            Verditius.GetAbility(MagicArts.Mentem).AddExperience(0);
            Verditius.GetAbility(MagicArts.Terram).AddExperience(0);
            Verditius.GetAbility(MagicArts.Vim).AddExperience(0);
            Verditius.GetAbility(Abilities.AreaLore).AddExperience(0);
            Verditius.GetAbility(Abilities.ArtesLiberales).AddExperience(0);
            Verditius.GetAbility(Abilities.English).AddExperience(75);
            Verditius.GetAbility(Abilities.Etiquette).AddExperience(5);
            Verditius.GetAbility(Abilities.Latin).AddExperience(0);
            Verditius.GetAbility(Abilities.MagicTheory).AddExperience(0);
            Verditius.GetAbility(Abilities.ParmaMagica).AddExperience(0);
            Verditius.GetAbility(Abilities.Penetration).AddExperience(0);
            Verditius.GetAbility(Abilities.Concentration).AddExperience(0);
        }
    }
}
