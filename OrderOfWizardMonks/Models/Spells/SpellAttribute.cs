namespace WizardMonks.Models.Spells
{
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
}
