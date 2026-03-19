using WizardMonks.Activities;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Spells;

namespace WizardMonks.Models.Traditions
{
    /// <summary>
    /// Marker interface for the atomic unit of magical knowledge that can be
    /// carried by a TraditionConcept. Each implementation represents a distinct
    /// category of capability a magical tradition can possess.
    /// </summary>
    public interface ITraditionPrinciple
    {
        string DisplayName { get; }
    }

    /// <summary>
    /// A spell Range (e.g., Touch, Voice, Arcane Connection) that this tradition
    /// knows how to use. A mage whose tradition contains this principle may
    /// use this Range when inventing spells.
    /// </summary>
    public class RangePrinciple : ITraditionPrinciple
    {
        public EffectRange Range { get; }
        public string DisplayName => $"Range: {Range.Range}";

        public RangePrinciple(EffectRange range)
        {
            Range = range;
        }
    }

    /// <summary>
    /// A spell Duration (e.g., Diameter, Sun, Moon) that this tradition
    /// knows how to use.
    /// </summary>
    public class DurationPrinciple : ITraditionPrinciple
    {
        public EffectDuration Duration { get; }
        public string DisplayName => $"Duration: {Duration.Duration}";

        public DurationPrinciple(EffectDuration duration)
        {
            Duration = duration;
        }
    }

    /// <summary>
    /// A spell Target (e.g., Individual, Group, Boundary) that this tradition
    /// knows how to use.
    /// </summary>
    public class TargetPrinciple : ITraditionPrinciple
    {
        public EffectTarget Target { get; }
        public string DisplayName => $"Target: {Target.Target}";

        public TargetPrinciple(EffectTarget target)
        {
            Target = target;
        }
    }

    /// <summary>
    /// A SpellBase effect that this tradition can work with. The ArtPair on
    /// the SpellBase drives the Art-distribution calculation during Gift opening.
    /// </summary>
    public class SpellBasePrinciple : ITraditionPrinciple
    {
        public SpellBase SpellBase { get; }
        public string DisplayName => $"Spell Effect: {SpellBase.Name}";

        public SpellBasePrinciple(SpellBase spellBase)
        {
            SpellBase = spellBase;
        }
    }

    /// <summary>
    /// A laboratory or magical activity (e.g., InventSpells, OpenArts) that
    /// this tradition can perform. Concepts of this type typically represent
    /// Major or Hermetic Breakthroughs when integrated from a hedge tradition.
    /// Note: some activities (e.g., StudyVis) are not strictly lab activities
    /// but are still gated by tradition capability and modeled here.
    /// </summary>
    public class LabActivityPrinciple : ITraditionPrinciple
    {
        public Activity Activity { get; }
        public string DisplayName => $"Activity: {Activity}";

        public LabActivityPrinciple(Activity activity)
        {
            Activity = activity;
        }
    }

    /// <summary>
    /// An Ability that is a core part of this magical tradition's practice.
    /// For Hermetic magi this includes Magic Theory, Parma Magica, Finesse,
    /// Penetration, and Concentration. For hedge traditions it includes their
    /// own Theory Ability (e.g., "Gruagach Theory") and any tradition-specific
    /// Arcane Abilities.
    ///
    /// Abilities wrapped in this principle contribute to the Ease Factor when
    /// a second tradition attempts to open over this one, proportional to the
    /// character's score in them.
    /// </summary>
    public class MagicalAbilityPrinciple : ITraditionPrinciple
    {
        public Ability Ability { get; }
        public string DisplayName => $"Magical Ability: {Ability.AbilityName}";

        public MagicalAbilityPrinciple(Ability ability)
        {
            Ability = ability;
        }
    }
}