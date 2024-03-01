using System;
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

        public static int Count = 15;

        static MagicArts()
        {
            Creo = new Ability(300, AbilityType.Art, "Creo");
            Intellego = new Ability(301, AbilityType.Art, "Intellego");
            Muto = new Ability(302, AbilityType.Art, "Muto");
            Perdo = new Ability(303, AbilityType.Art, "Perdo");
            Rego = new Ability(304, AbilityType.Art, "Rego");
            Animal = new Ability(305, AbilityType.Art, "Animal");
            Aquam = new Ability(306, AbilityType.Art, "Aquam");
            Auram = new Ability(307, AbilityType.Art, "Auram");
            Corpus = new Ability(308, AbilityType.Art, "Corpus");
            Herbam = new Ability(309, AbilityType.Art, "Herbam");
            Ignem = new Ability(310, AbilityType.Art, "Ignem");
            Imaginem = new Ability(311, AbilityType.Art, "Imaginem");
            Mentem = new Ability(312, AbilityType.Art, "Mentem");
            Terram = new Ability(313, AbilityType.Art, "Terram");
            Vim = new Ability(314, AbilityType.Art, "Vim");
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
        public static ArtPair CrAn = new(MagicArts.Creo, MagicArts.Animal);
        public static ArtPair CrAq = new(MagicArts.Creo, MagicArts.Aquam);
        public static ArtPair CrAu = new(MagicArts.Creo, MagicArts.Auram);
        public static ArtPair CrCo = new(MagicArts.Creo, MagicArts.Corpus);
        public static ArtPair CrHe = new(MagicArts.Creo, MagicArts.Herbam);
        public static ArtPair CrIg = new(MagicArts.Creo, MagicArts.Ignem);
        public static ArtPair CrIm = new(MagicArts.Creo, MagicArts.Imaginem);
        public static ArtPair CrMe = new(MagicArts.Creo, MagicArts.Mentem);
        public static ArtPair CrTe = new(MagicArts.Creo, MagicArts.Terram);
        public static ArtPair CrVi = new(MagicArts.Creo, MagicArts.Vim);
        public static ArtPair InAn = new(MagicArts.Intellego, MagicArts.Animal);
        public static ArtPair InAq = new(MagicArts.Intellego, MagicArts.Aquam);
        public static ArtPair InAu = new(MagicArts.Intellego, MagicArts.Auram);
        public static ArtPair InCo = new(MagicArts.Intellego, MagicArts.Corpus);
        public static ArtPair InHe = new(MagicArts.Intellego, MagicArts.Herbam);
        public static ArtPair InIg = new(MagicArts.Intellego, MagicArts.Ignem);
        public static ArtPair InIm = new(MagicArts.Intellego, MagicArts.Imaginem);
        public static ArtPair InMe = new(MagicArts.Intellego, MagicArts.Mentem);
        public static ArtPair InTe = new(MagicArts.Intellego, MagicArts.Terram);
        public static ArtPair InVi = new(MagicArts.Intellego, MagicArts.Vim);
        public static ArtPair MuAn = new(MagicArts.Muto, MagicArts.Animal);
        public static ArtPair MuAq = new(MagicArts.Muto, MagicArts.Aquam);
        public static ArtPair MuAu = new(MagicArts.Muto, MagicArts.Auram);
        public static ArtPair MuCo = new(MagicArts.Muto, MagicArts.Corpus);
        public static ArtPair MuHe = new(MagicArts.Muto, MagicArts.Herbam);
        public static ArtPair MuIg = new(MagicArts.Muto, MagicArts.Ignem);
        public static ArtPair MuIm = new(MagicArts.Muto, MagicArts.Imaginem);
        public static ArtPair MuMe = new(MagicArts.Muto, MagicArts.Mentem);
        public static ArtPair MuTe = new(MagicArts.Muto, MagicArts.Terram);
        public static ArtPair MuVi = new(MagicArts.Muto, MagicArts.Vim);
        public static ArtPair PeAn = new(MagicArts.Perdo, MagicArts.Animal);
        public static ArtPair PeAq = new(MagicArts.Perdo, MagicArts.Aquam);
        public static ArtPair PeAu = new(MagicArts.Perdo, MagicArts.Auram);
        public static ArtPair PeCo = new(MagicArts.Perdo, MagicArts.Corpus);
        public static ArtPair PeHe = new(MagicArts.Perdo, MagicArts.Herbam);
        public static ArtPair PeIg = new(MagicArts.Perdo, MagicArts.Ignem);
        public static ArtPair PeIm = new(MagicArts.Perdo, MagicArts.Imaginem);
        public static ArtPair PeMe = new(MagicArts.Perdo, MagicArts.Mentem);
        public static ArtPair PeTe = new(MagicArts.Perdo, MagicArts.Terram);
        public static ArtPair PeVi = new(MagicArts.Perdo, MagicArts.Vim);
        public static ArtPair ReAn = new(MagicArts.Rego, MagicArts.Animal);
        public static ArtPair ReAq = new(MagicArts.Rego, MagicArts.Aquam);
        public static ArtPair ReAu = new(MagicArts.Rego, MagicArts.Auram);
        public static ArtPair ReCo = new(MagicArts.Rego, MagicArts.Corpus);
        public static ArtPair ReHe = new(MagicArts.Rego, MagicArts.Herbam);
        public static ArtPair ReIg = new(MagicArts.Rego, MagicArts.Ignem);
        public static ArtPair ReIm = new(MagicArts.Rego, MagicArts.Imaginem);
        public static ArtPair ReMe = new(MagicArts.Rego, MagicArts.Mentem);
        public static ArtPair ReTe = new(MagicArts.Rego, MagicArts.Terram);
        public static ArtPair ReVi = new(MagicArts.Rego, MagicArts.Vim);
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
                case 305:
                    return animal;
                case 306:
                    return aquam;
                case 307:
                    return auram;
                case 308:
                    return corpus;
                case 309:
                    return herbam;
                case 310:
                    return ignem;
                case 311:
                    return imaginem;
                case 312:
                    return mentem;
                case 313:
                    return terram;
                case 314:
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
