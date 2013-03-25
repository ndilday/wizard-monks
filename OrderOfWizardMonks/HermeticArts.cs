using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks
{
    public static class MagicArts
    {
        public static Ability Creo { get; set; }
        public static Ability Intellego { get; set; }
        public static Ability Muto { get; set; }
        public static Ability Perdo { get; set; }
        public static Ability Rego { get; set; }
        public static Ability Animal { get; set; }
        public static Ability Aquam { get; set; }
        public static Ability Auram { get; set; }
        public static Ability Corpus { get; set; }
        public static Ability Herbam { get; set; }
        public static Ability Ignem { get; set; }
        public static Ability Imaginem { get; set; }
        public static Ability Mentem { get; set; }
        public static Ability Terram { get; set; }
        public static Ability Vim { get; set; }

        static MagicArts()
        {
            Creo = new Ability(300, AbilityType.Art, "Creo");
            Intellego = new Ability(301, AbilityType.Art, "Intellego");
            Muto = new Ability(302, AbilityType.Art, "Muto");
            Perdo = new Ability(303, AbilityType.Art, "Perdo");
            Rego = new Ability(304, AbilityType.Art, "Rego");
            Animal = new Ability(305, AbilityType.Art, "Animal");
            Aquam = new Ability(350, AbilityType.Art, "Aquam");
            Auram = new Ability(351, AbilityType.Art, "Auram");
            Corpus = new Ability(352, AbilityType.Art, "Corpus");
            Herbam = new Ability(353, AbilityType.Art, "Herbam");
            Ignem = new Ability(354, AbilityType.Art, "Ignem");
            Imaginem = new Ability(355, AbilityType.Art, "Imaginem");
            Mentem = new Ability(356, AbilityType.Art, "Mentem");
            Terram = new Ability(357, AbilityType.Art, "Terram");
            Vim = new Ability(358, AbilityType.Art, "Vim");
        }
    }

    public class Arts
    {
        protected AcceleratedAbility creo;
        protected AcceleratedAbility intellego;
        protected AcceleratedAbility muto;
        protected AcceleratedAbility perdo;
        protected AcceleratedAbility rego;
        protected AcceleratedAbility animal;
        protected AcceleratedAbility aquam;
        protected AcceleratedAbility auram;
        protected AcceleratedAbility corpus;
        protected AcceleratedAbility herbam;
        protected AcceleratedAbility ignem;
        protected AcceleratedAbility imaginem;
        protected AcceleratedAbility mentem;
        protected AcceleratedAbility terram;
        protected AcceleratedAbility vim;

        public AcceleratedAbility Creo
        {
            get
            {
                return creo;
            }
        }

        public AcceleratedAbility Intellego
        {
            get
            {
                return intellego;
            }
        }

        public AcceleratedAbility Muto
        {
            get
            {
                return muto;
            }
        }

        public AcceleratedAbility Perdo
        {
            get
            {
                return perdo;
            }
        }

        public AcceleratedAbility Rego
        {
            get
            {
                return rego;
            }
        }

        public AcceleratedAbility Animal
        {
            get
            {
                return animal;
            }
        }

        public AcceleratedAbility Aquam
        {
            get
            {
                return aquam;
            }
        }

        public AcceleratedAbility Auram
        {
            get
            {
                return auram;
            }
        }

        public AcceleratedAbility Corpus
        {
            get
            {
                return corpus;
            }
        }

        public AcceleratedAbility Herbam
        {
            get
            {
                return herbam;
            }
        }

        public AcceleratedAbility Ignem
        {
            get
            {
                return ignem;
            }
        }

        public AcceleratedAbility Imaginem
        {
            get
            {
                return imaginem;
            }
        }

        public AcceleratedAbility Mentem
        {
            get
            {
                return mentem;
            }
        }

        public AcceleratedAbility Terram
        {
            get
            {
                return terram;
            }
        }

        public AcceleratedAbility Vim
        {
            get
            {
                return vim;
            }
        }

        public Arts()
        {
            creo = new AcceleratedAbility(MagicArts.Creo);
            intellego = new AcceleratedAbility(MagicArts.Intellego);
            muto = new AcceleratedAbility(MagicArts.Muto);
            perdo = new AcceleratedAbility(MagicArts.Perdo);
            rego = new AcceleratedAbility(MagicArts.Rego);
            animal = new AcceleratedAbility(MagicArts.Animal);
            aquam = new AcceleratedAbility(MagicArts.Aquam);
            auram = new AcceleratedAbility(MagicArts.Auram);
            corpus = new AcceleratedAbility(MagicArts.Corpus);
            herbam = new AcceleratedAbility(MagicArts.Herbam);
            ignem = new AcceleratedAbility(MagicArts.Ignem);
            imaginem = new AcceleratedAbility(MagicArts.Imaginem);
            mentem = new AcceleratedAbility(MagicArts.Mentem);
            terram = new AcceleratedAbility(MagicArts.Terram);
            vim = new AcceleratedAbility(MagicArts.Vim);
        }
    }
}
