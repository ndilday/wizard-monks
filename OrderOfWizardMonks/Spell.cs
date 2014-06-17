using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WizardMonks.Instances;

namespace WizardMonks
{
    public enum Range
    {
        Personal,
        Touch,
        Eye,
        Voice,
        Sight,
        Arcane
    }

    public enum Duration
    {
        Instantaneous,
        Concentration,
        Diameter,
        Sun,
        Circle,
        Moon,
        Year
    }

    public enum Target
    {
        Individual,
        Part,
        Group,
        Ring,
        Room,
        Structure,
        Boundary
    }

    public class Spell
    {
        public ArtPair BaseArts { get; private set; }

        public Range Range { get; private set; }
        public Duration Duration { get; private set; }
        public Target Target { get; private set; }
        public byte Base { get; private set; }
        public byte Modifiers { get; private set; }
        public bool IsRitual { get; private set; }

        public SpellArts RequisiteTechniques { get; private set; }
        public SpellArts RequisiteForms { get; private set; }
        public string Name { get; private set; }

        public double Level
        {
            get
            {
                int totalModifier = SpellModifiers.GetModifier(Duration) +
                    SpellModifiers.GetModifier(Range) +
                    SpellModifiers.GetModifier(Target);
                if (Base + totalModifier > 5)
                {
                    return (Base + totalModifier - 4) * 5;
                }
                return Base + totalModifier;
            }
        }

        public Spell(ArtPair artPair, Range range, Duration duration, Target target, byte baseLevel, byte modifiers, bool isRitual, string name)
        {
            BaseArts = artPair;
            Range = range;
            Duration = duration;
            Target = target;
            Base = baseLevel;
            Modifiers = modifiers;
            IsRitual = isRitual;
            Name = name;
        }
    }

    [Flags]
    public enum SpellArts
    {
        Creo = 0x8000,
        Intellego = 0x4000,
        Muto = 0x2000,
        Perdo = 0x1000,
        Rego = 0x0800,
        Animal = 0x0400,
        Aquam = 0x0200,
        Auram = 0x0100,
        Corpus = 0x0080,
        Herbam = 0x0040,
        Ignem = 0x0020,
        Imaginem = 0x0010,
        Mentem = 0x0008,
        Terram = 0x0004,
        Vim = 0x0002,
        Techniques = 0xF800,
        Forms = 0x07FE
    }

    public abstract class SpellBase
    {
        public abstract SpellArts Arts {get;}
        public abstract ArtPair ArtPair {get;}

        public ushort ID { get; protected set; }
        public ushort Level { get; protected set; }
    }

    public class CrAnSpellBase : SpellBase
    {
        public override SpellArts Arts
        {
            get
            {
                return SpellArts.Creo | SpellArts.Animal;
            }
        }

        public override ArtPair ArtPair
        {
            get
            {
                return MagicArtPairs.CrAn;
            }
        }

        public CrAnSpellBase(byte id, byte level)
        {
            ID = id;
            Level = level;
        }
    }

    #region Animal Spells
    public static class CrAnBases
    {
        public static readonly CrAnSpellBase CreateAnimalProduct;
        public static readonly CrAnSpellBase CreateInsect;
        public const byte CreateAnimalCorpse = 10;
        public const byte CreateBird = 10;
        public const byte CreateReptile = 10;
        public const byte CreateAmphibian = 10;
        public const byte CreateFish = 10;
        public const byte CreateMammal = 15;
        public const byte AcceleratedMaturityDay = 15;
        public const byte AcceleratedMaturityTwoHour = 20;
        public const byte StopDisease = 25;
        public const byte RestoreSense = 25;
        public const byte RestoreLimb = 25;
        public const byte IncreaseChar0 = 30;
        public const byte AcceleratedMaturityDiameter = 30;
        public const byte IncreaseChar1 = 35;
        public const byte IncreaseChar2 = 40;
        public const byte IncreaseChar3 = 45;
        public const byte IncreaseChar4 = 50;
        public const byte CreateMagicBeast = 50;
        public const byte IncreaseChar5 = 55;
        public const byte RaiseDead = 75;

        static CrAnBases()
        {
            CreateAnimalProduct = new CrAnSpellBase(1, 5);
            CreateInsect = new CrAnSpellBase(2, 5);
        }
    }

    public static class InAnBases
    {
        public const byte MentalImage = 1;
        public const byte KnowGeneralInformation = 3;
        public const byte KnowConsciousness = 3;
        public const byte KnowMotivation = 4;
        public const byte KnowSingleFact = 4;
        public const byte KnowSingleFactProduct = 5;
        public const byte SpeakAnimal = 10;
        public const byte ReadRecentMemories = 15;
        public const byte MindProbe = 20;
    }

    public static class MuAnBases
    {

    }
    #endregion

    #region Vim Spells
    public static class InViBases
    {
        public const byte DetectAura = 1;
        public const byte DetectVis = 1;
        public const byte KnowAuraStrength = 2;
        public const byte KnowRegioBoundaries = 3;
        public const byte KnowVisAmount = 4;
        public const byte KnowVisType = 4;
    }
    #endregion
}
