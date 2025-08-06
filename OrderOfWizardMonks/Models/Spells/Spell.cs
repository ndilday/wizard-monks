using System;
using WizardMonks.Core;

//using WizardMonks.Instances;

namespace WizardMonks.Models.Spells
{

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
}
