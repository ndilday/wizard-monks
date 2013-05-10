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
        public int Base { get; private set; }

        public IEnumerable<Ability> RequisiteTechniques { get; private set; }
        public IEnumerable<Ability> RequisiteForms { get; private set; }
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

        public Spell(ArtPair artPair, Range range, Duration duration, Target target, int baseLevel, string name)
        {
            BaseArts = artPair;
            Range = range;
            Duration = duration;
            Target = target;
            Base = baseLevel;
            Name = name;
        }
    }
}
