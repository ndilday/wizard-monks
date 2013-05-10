﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks.Instances
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
            Animal = new Ability(350, AbilityType.Art, "Animal");
            Aquam = new Ability(351, AbilityType.Art, "Aquam");
            Auram = new Ability(352, AbilityType.Art, "Auram");
            Corpus = new Ability(353, AbilityType.Art, "Corpus");
            Herbam = new Ability(354, AbilityType.Art, "Herbam");
            Ignem = new Ability(355, AbilityType.Art, "Ignem");
            Imaginem = new Ability(356, AbilityType.Art, "Imaginem");
            Mentem = new Ability(357, AbilityType.Art, "Mentem");
            Terram = new Ability(358, AbilityType.Art, "Terram");
            Vim = new Ability(359, AbilityType.Art, "Vim");
        }

        public static bool IsTechnique(Ability ability)
        {
            return ability.AbilityId == MagicArts.Creo.AbilityId ||
                   ability.AbilityId == MagicArts.Intellego.AbilityId ||
                   ability.AbilityId == MagicArts.Muto.AbilityId ||
                   ability.AbilityId == MagicArts.Perdo.AbilityId ||
                   ability.AbilityId == MagicArts.Rego.AbilityId;
        }

        public static bool IsForm(Ability ability)
        {
            return ability.AbilityId == MagicArts.Animal.AbilityId ||
                   ability.AbilityId == MagicArts.Aquam.AbilityId ||
                   ability.AbilityId == MagicArts.Auram.AbilityId ||
                   ability.AbilityId == MagicArts.Corpus.AbilityId ||
                   ability.AbilityId == MagicArts.Herbam.AbilityId ||
                   ability.AbilityId == MagicArts.Ignem.AbilityId ||
                   ability.AbilityId == MagicArts.Imaginem.AbilityId ||
                   ability.AbilityId == MagicArts.Mentem.AbilityId ||
                   ability.AbilityId == MagicArts.Terram.AbilityId ||
                   ability.AbilityId == MagicArts.Vim.AbilityId;
        }

        public static bool IsArt(Ability ability)
        {
            return ability.AbilityId >= 300 && ability.AbilityId < 400;
        }

        public static IEnumerable<Ability> GetEnumerator()
        {
            yield return Creo;
            yield return Intellego;
            yield return Muto;
            yield return Perdo;
            yield return Rego;
            yield return Animal;
            yield return Aquam;
            yield return Auram;
            yield return Corpus;
            yield return Herbam;
            yield return Ignem;
            yield return Imaginem;
            yield return Mentem;
            yield return Terram;
            yield return Vim;
        }
    }

    public static class MagicArtPairs
    {
        public static ArtPair CrAn = new ArtPair(MagicArts.Creo, MagicArts.Animal);
        public static ArtPair CrAq = new ArtPair(MagicArts.Creo, MagicArts.Aquam);
        public static ArtPair CrAu = new ArtPair(MagicArts.Creo, MagicArts.Auram);
        public static ArtPair CrCo = new ArtPair(MagicArts.Creo, MagicArts.Corpus);
        public static ArtPair CrHe = new ArtPair(MagicArts.Creo, MagicArts.Herbam);
        public static ArtPair CrIg = new ArtPair(MagicArts.Creo, MagicArts.Ignem);
        public static ArtPair CrIm = new ArtPair(MagicArts.Creo, MagicArts.Imaginem);
        public static ArtPair CrMe = new ArtPair(MagicArts.Creo, MagicArts.Mentem);
        public static ArtPair CrTe = new ArtPair(MagicArts.Creo, MagicArts.Terram);
        public static ArtPair CrVi = new ArtPair(MagicArts.Creo, MagicArts.Vim);
        public static ArtPair InAn = new ArtPair(MagicArts.Intellego, MagicArts.Animal);
        public static ArtPair InAq = new ArtPair(MagicArts.Intellego, MagicArts.Aquam);
        public static ArtPair InAu = new ArtPair(MagicArts.Intellego, MagicArts.Auram);
        public static ArtPair InCo = new ArtPair(MagicArts.Intellego, MagicArts.Corpus);
        public static ArtPair InHe = new ArtPair(MagicArts.Intellego, MagicArts.Herbam);
        public static ArtPair InIg = new ArtPair(MagicArts.Intellego, MagicArts.Ignem);
        public static ArtPair InIm = new ArtPair(MagicArts.Intellego, MagicArts.Imaginem);
        public static ArtPair InMe = new ArtPair(MagicArts.Intellego, MagicArts.Mentem);
        public static ArtPair InTe = new ArtPair(MagicArts.Intellego, MagicArts.Terram);
        public static ArtPair InVi = new ArtPair(MagicArts.Intellego, MagicArts.Vim);
        public static ArtPair MuAn = new ArtPair(MagicArts.Muto, MagicArts.Animal);
        public static ArtPair MuAq = new ArtPair(MagicArts.Muto, MagicArts.Aquam);
        public static ArtPair MuAu = new ArtPair(MagicArts.Muto, MagicArts.Auram);
        public static ArtPair MuCo = new ArtPair(MagicArts.Muto, MagicArts.Corpus);
        public static ArtPair MuHe = new ArtPair(MagicArts.Muto, MagicArts.Herbam);
        public static ArtPair MuIg = new ArtPair(MagicArts.Muto, MagicArts.Ignem);
        public static ArtPair MuIm = new ArtPair(MagicArts.Muto, MagicArts.Imaginem);
        public static ArtPair MuMe = new ArtPair(MagicArts.Muto, MagicArts.Mentem);
        public static ArtPair MuTe = new ArtPair(MagicArts.Muto, MagicArts.Terram);
        public static ArtPair MuVi = new ArtPair(MagicArts.Muto, MagicArts.Vim);
        public static ArtPair PeAn = new ArtPair(MagicArts.Perdo, MagicArts.Animal);
        public static ArtPair PeAq = new ArtPair(MagicArts.Perdo, MagicArts.Aquam);
        public static ArtPair PeAu = new ArtPair(MagicArts.Perdo, MagicArts.Auram);
        public static ArtPair PeCo = new ArtPair(MagicArts.Perdo, MagicArts.Corpus);
        public static ArtPair PeHe = new ArtPair(MagicArts.Perdo, MagicArts.Herbam);
        public static ArtPair PeIg = new ArtPair(MagicArts.Perdo, MagicArts.Ignem);
        public static ArtPair PeIm = new ArtPair(MagicArts.Perdo, MagicArts.Imaginem);
        public static ArtPair PeMe = new ArtPair(MagicArts.Perdo, MagicArts.Mentem);
        public static ArtPair PeTe = new ArtPair(MagicArts.Perdo, MagicArts.Terram);
        public static ArtPair PeVi = new ArtPair(MagicArts.Perdo, MagicArts.Vim);
        public static ArtPair ReAn = new ArtPair(MagicArts.Rego, MagicArts.Animal);
        public static ArtPair ReAq = new ArtPair(MagicArts.Rego, MagicArts.Aquam);
        public static ArtPair ReAu = new ArtPair(MagicArts.Rego, MagicArts.Auram);
        public static ArtPair ReCo = new ArtPair(MagicArts.Rego, MagicArts.Corpus);
        public static ArtPair ReHe = new ArtPair(MagicArts.Rego, MagicArts.Herbam);
        public static ArtPair ReIg = new ArtPair(MagicArts.Rego, MagicArts.Ignem);
        public static ArtPair ReIm = new ArtPair(MagicArts.Rego, MagicArts.Imaginem);
        public static ArtPair ReMe = new ArtPair(MagicArts.Rego, MagicArts.Mentem);
        public static ArtPair ReTe = new ArtPair(MagicArts.Rego, MagicArts.Terram);
        public static ArtPair ReVi = new ArtPair(MagicArts.Rego, MagicArts.Vim);
    }

    public class Arts : IEnumerable<CharacterAbilityBase>
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

        public CharacterAbilityBase GetAbility(Ability ability)
        {
            switch (ability.AbilityId)
            {
                case 300:
                    return creo;
                case 301:
                    return intellego;
                case 302:
                    return muto;
                case 303:
                    return perdo;
                case 304:
                    return rego;
                case 350:
                    return animal;
                case 351:
                    return aquam;
                case 352:
                    return auram;
                case 353:
                    return corpus;
                case 354:
                    return herbam;
                case 355:
                    return ignem;
                case 356:
                    return imaginem;
                case 357:
                    return mentem;
                case 358:
                    return terram;
                case 359:
                    return vim;
                default:
                    return null;
            }
        }

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

        public IEnumerator<CharacterAbilityBase> GetEnumerator()
        {
            yield return creo;
            yield return intellego;
            yield return muto;
            yield return perdo;
            yield return rego;
            yield return animal;
            yield return aquam;
            yield return auram;
            yield return corpus;
            yield return herbam;
            yield return ignem;
            yield return imaginem;
            yield return mentem;
            yield return terram;
            yield return vim;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
