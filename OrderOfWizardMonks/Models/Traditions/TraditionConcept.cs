using System;

namespace WizardMonks.Models.Traditions
{
    /// <summary>
    /// The atomic unit of magical knowledge within a MagicalTradition.
    /// Each concept represents one capability — a Range, Duration, SpellBase,
    /// LabActivity, MagicalAbility, or similar principle — that the tradition
    /// currently possesses.
    ///
    /// Concepts arrive in a tradition via:
    ///   - Initial construction (the tradition was always capable of this)
    ///   - Completed research (a mage integrated this from another tradition)
    ///   - Propagation (a mage read a lab text or book embedding this concept)
    ///   - Teaching (a mage was taught this concept directly)
    ///
    /// BreakthroughPointsRequired represents the research cost for a mage of
    /// a different tradition to integrate this concept into their own. It does
    /// not affect a practitioner of the source tradition, who already possesses
    /// it natively. A value of 0 means the concept is part of the tradition's
    /// baseline and requires no research to use.
    ///
    /// Rough thresholds per Hedge Magic Revised:
    ///   ~30 pts = Minor Breakthrough (new Range, Duration, Target)
    ///   ~45 pts = Major Breakthrough (new capability or teachable Virtue)
    ///   ~60 pts = Hermetic Breakthrough (breaks a Limit of Magic)
    ///      0 pts = Native to the tradition, no research required
    /// </summary>
    public class TraditionConcept
    {
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// Human-readable name used in logs and the UI.
        /// Defaults to the principle's DisplayName if not explicitly provided.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The number of breakthrough points a researcher must accumulate to
        /// integrate this concept into their own tradition.
        /// </summary>
        public ushort BreakthroughPointsRequired { get; }

        /// <summary>
        /// The actual magical capability this concept represents.
        /// </summary>
        public ITraditionPrinciple Principle { get; }

        public TraditionConcept(ITraditionPrinciple principle, ushort breakthroughPointsRequired = 0)
        {
            Principle = principle ?? throw new ArgumentNullException(nameof(principle));
            BreakthroughPointsRequired = breakthroughPointsRequired;
            Name = principle.DisplayName;
        }

        public TraditionConcept(string name, ITraditionPrinciple principle, ushort breakthroughPointsRequired = 0)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Principle = principle ?? throw new ArgumentNullException(nameof(principle));
            BreakthroughPointsRequired = breakthroughPointsRequired;
        }
    }
}