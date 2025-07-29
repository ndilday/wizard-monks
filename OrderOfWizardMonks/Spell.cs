using System;
using WizardMonks.Core;

//using WizardMonks.Instances;

namespace WizardMonks
{
    #region Spell Attributes
    public abstract class SpellAttribute
    {
        public byte Level { get; private set; }
        public bool NeedsRitual { get; private set; }

        public SpellAttribute(byte level, bool needsRitual)
        {
            Level = level;
            NeedsRitual = needsRitual;
        }
    }

    public class EffectRange : SpellAttribute
    {
        public Ranges Range { get; private set; }
        public EffectRange(Ranges range, byte level, bool needsRitual = false) : base(level, needsRitual)
        {
            Range = range;
        }
    }

    public enum Ranges
    {
        Personal,
        Touch,
        Eye,
        Voice,
        Sight,
        Arcane
    }

    public class EffectDuration : SpellAttribute
    {
        public Durations Duration { get; private set; }
        public EffectDuration(Durations duration, byte level, bool needsRitual = false) : base(level, needsRitual)
        {
            Duration = duration;
        }
    }

    public enum Durations
    {
        Instantaneous,
        Concentration,
        Diameter,
        Sun,
        Ring,
        Moon,
        Year
    }

    public class EffectTarget : SpellAttribute
    {
        public Targets Target { get; private set; }
        public EffectTarget(Targets target, byte level, bool needsRitual = false)
            : base(level, needsRitual)
        {
            Target = target;
        }
    }

    public enum Targets
    {
        Individual,
        Taste,
        Part,
        Touch,
        Group,
        Smell,
        Circle,
        Room,
        Structure,
        Hearing,
        Boundary,
        Sight
    }

    public class EffectUse : SpellAttribute
    {
        public Uses Use { get; private set; }
        public EffectUse(Uses uses, byte level, bool needsRitual = false)
            : base(level, needsRitual)
        {
            Use = uses;
        }
    }
    
    public enum Uses
    {
        FindVis,
        FindAura
    }
    #endregion

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

    public class Spell
    {
        public EffectRange Range { get; private set; }
        public EffectDuration Duration { get; private set; }
        public EffectTarget Target { get; private set; }
        public SpellBase Base { get; private set; }
        public byte Modifiers { get; private set; }
        public bool IsRitual { get; private set; }

        public SpellArts RequisiteArts { get; private set; }
        public string Name { get; private set; }

        public ushort Level
        {
            get
            {
                int rdtMagnitudes = Range.Level + Duration.Level + Target.Level;
                double totalMagnitudes = Base.Magnitude + rdtMagnitudes;

                return SpellLevelMath.GetLevelFromMagnitude(totalMagnitudes);
            }
        }

        public Spell(EffectRange range, EffectDuration duration, EffectTarget target, SpellBase spellBase, byte modifiers, bool isRitual, string name)
        {
            Range = range;
            Duration = duration;
            Target = target;
            Base = spellBase;
            Modifiers = modifiers;
            IsRitual = isRitual;
            Name = name;
        }
    }

    public class SpellBase
    {
        public SpellArts Arts { get; private set; }
        public ArtPair ArtPair { get; private set; }

        public TechniqueEffects TechniqueEffects { get; private set; }
        public FormEffects FormEffects { get; private set; }

        public ushort Magnitude { get; protected set; }

        public string Name { get; private set; }

        public SpellBase(TechniqueEffects techniqueEffects, FormEffects formEffects, SpellArts arts, ArtPair artPair, ushort magnitude, string name)
        {
            TechniqueEffects = techniqueEffects;
            FormEffects = formEffects;
            Magnitude = magnitude;
            Arts = arts;
            ArtPair = artPair;
            Name = name;
        }
    }

    [Flags]
    public enum TechniqueEffects : long
    {
        // Creo
        RecoveryBonus = 0x0000000000000001,
        Heal = 0x0000000000000002,
        CureDisease = 0x0000000000000004,
        Mature = 0x0000000000000008,
        RestoreSense = 0x0000000000000010,
        RestoreLimb = 0x0000000000000020,
        IncreaseAttribute = 0x0000000000000040,
        Create = 0x0000000000000080,
        RaiseFromDead = 0x0000000000000100,
        // Intellego
        GetMentalImage = 0x0000000000001000,
        SenseConsciousness = 0x0000000000002000,
        GetGeneralInformation = 0x0000000000004000,
        SenseDominantDrive = 0x0000000000008000,
        GetSpecificAnswer = 0x0000000000010000,
        LearnHistory = 0x0000000000020000,
        SpeakWith = 0x0000000000040000,
        ReadRecentMemories = 0x0000000000080000,
        MindProbe = 0x0000000000100000,
        MakeSensesUnhinderedBy = 0x0000000000200000,
        Detect = 0x0000000000400000,
        Quantify = 0x0000000000800000,
        // Muto
        SuperficialChange = 0x0000000001000000,
        MajorChange = 0x0000000002000000,
        SubstancialChange = 0x0000000004000000,
        MinorUnnaturalChange = 0x0000000008000000,
        MajorUnnaturalChange = 0x0000000010000000,
        ChangeToHuman = 0x0000000020000000,
        ChangeToPlant = 0x0000000040000000,
        // Perdo
        SuperficialDamage = 0x0000001000000000,
        Destroy = 0x0000002000000000,
        Pain = 0x0000004000000000,
        Fatigue = 0x0000008000000000,
        Injure = 0x0000010000000000,
        Wound = 0x0000020000000000,
        Cripple = 0x0000040000000000,
        Age = 0x0000080000000000,
        DestroyLimb = 0x0000100000000000,
        Kill = 0x0000200000000000,
        DestroyProperty = 0x0000400000000000,
        Reduce = 0x0000800000000000,
        // Rego
        Ward = 0x0001000000000000,
        Manipulate = 0x0002000000000000,
        PlantSuggestion = 0x0004000000000000,
        Paralyze = 0x0008000000000000,
        Control = 0x0010000000000000,
        Calm = 0x0020000000000000
    }

    [Flags]
    public enum FormEffects : long
    {
        // Animal
        Animal = 0x0000000000000001,
        Insect = 0x0000000000000002,
        MagicAnimal = 0x0000000000000004,
        PlainAnimalProduct = 0x0000000000000010,
        TreatedAnimalProduct = 0x0000000000000020,
        ProcessedAnimalProduct = 0x0000000000000040,
        // Aquam
        Water = 0x0000000000000100,
        NaturalLiquid = 0x0000000000000200,
        CorrosiveLiquid = 0x0000000000000400,
        UnnaturalLiquid = 0x000000000000800,
        VeryUnnaturalLiquid = 0x0000000000001000,
        Poison = 0x0000000000002000,
        Spring = 0x0000000000004000,
        Geyser = 0x0000000000008000,
        // Auram
        MinorWeather = 0x0000000000100000,
        NormalWeather = 0x0000000000200000,
        SevereWeather = 0x0000000000400000,
        VerySevereWeather = 0x0000000000800000,
        DebilitatingAir = 0x0000000001000000,
        //Vim
        Aura = 0x0100000000000000,
        Vis = 0x0200000000000000,
        Gift = 0x0400000000000000,
    }
}
