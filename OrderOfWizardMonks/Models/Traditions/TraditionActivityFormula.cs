using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Activities;
using WizardMonks.Models.Characters;

namespace WizardMonks.Models.Traditions
{
    /// <summary>
    /// Defines one component of an activity total formula: a specific Ability
    /// (or Art) and the coefficient by which its score is multiplied when
    /// computing the total. A coefficient of 1.0 means the score is added
    /// directly; 0.5 means half the score; 2.0 means double, etc.
    /// </summary>
    public class FormulaComponent
    {
        public Ability Ability { get; }
        public double Coefficient { get; }

        public FormulaComponent(Ability ability, double coefficient = 1.0)
        {
            Ability = ability ?? throw new ArgumentNullException(nameof(ability));
            if (coefficient == 0)
                throw new ArgumentOutOfRangeException(nameof(coefficient), "Coefficient must be non-zero.");
            Coefficient = coefficient;
        }
    }

    /// <summary>
    /// Encodes the formula a magical tradition uses to compute the total for a
    /// specific activity. The computed total is:
    ///
    ///   (sum of character.GetAbility(component.Ability).Value * component.Coefficient
    ///    for each component)
    ///   + aura strength (if IncludesAura is true)
    ///   / Divisor
    ///
    /// This allows traditions with structurally different activity mechanics —
    /// different arts, different ability combinations, different divisors — to
    /// be expressed as data rather than requiring separate service classes.
    ///
    /// Example: Hermetic vis distillation total is CrVi Lab Total / 10:
    ///   Components: [(Creo, 1), (Vim, 1), (Intelligence, 1), (MagicTheory, 1)]
    ///   IncludesAura: true
    ///   IncludesLabBonus: true
    ///   Divisor: 10
    ///
    /// Example: Hermetic vis study quality is a stress die + aura:
    ///   Components: []  (no fixed ability components — driven by die roll)
    ///   IncludesAura: true
    ///   IncludesLabBonus: false
    ///   Divisor: 1
    ///   BaseBonus: 0  (the die roll is handled at the activity layer)
    /// </summary>
    public class TraditionActivityFormula
    {
        /// <summary>The activity this formula applies to.</summary>
        public Activity Activity { get; }

        /// <summary>
        /// The ability/art components and their coefficients.
        /// Each component's contribution is Value * Coefficient.
        /// </summary>
        public IReadOnlyList<FormulaComponent> Components { get; }

        /// <summary>
        /// Whether the strength of the character's current aura is added
        /// to the total before applying the divisor.
        /// </summary>
        public bool IncludesAura { get; }

        /// <summary>
        /// Whether the character's laboratory general quality bonus is added
        /// to the total before applying the divisor. Only meaningful for
        /// activities performed in a laboratory.
        /// </summary>
        public bool IncludesLabBonus { get; }

        /// <summary>
        /// A flat bonus added to the total before applying the divisor.
        /// Useful for traditions whose activity totals include a fixed modifier.
        /// </summary>
        public double BaseBonus { get; }

        /// <summary>
        /// The value by which the summed total is divided to produce the
        /// final result. Defaults to 1.0 (no division).
        /// For Hermetic vis distillation this is 10.
        /// </summary>
        public double Divisor { get; }

        public TraditionActivityFormula(
            Activity activity,
            IEnumerable<FormulaComponent> components,
            bool includesAura = true,
            bool includesLabBonus = false,
            double baseBonus = 0.0,
            double divisor = 1.0)
        {
            if (divisor == 0)
                throw new ArgumentOutOfRangeException(nameof(divisor), "Divisor must be non-zero.");

            Activity = activity;
            Components = components != null
                ? new List<FormulaComponent>(components)
                : new List<FormulaComponent>();
            IncludesAura = includesAura;
            IncludesLabBonus = includesLabBonus;
            BaseBonus = baseBonus;
            Divisor = divisor;
        }

        /// <summary>
        /// Evaluates this formula for a given character, aura strength, and
        /// optional lab quality bonus. Returns the computed activity total.
        /// </summary>
        public double Evaluate(
            GiftedCharacter character,
            double auraStrength = 0,
            double labBonus = 0)
        {
            double total = BaseBonus;

            foreach (var component in Components)
            {
                total += character.GetAbility(component.Ability).Value * component.Coefficient;
            }

            if (IncludesAura)
                total += auraStrength;

            if (IncludesLabBonus)
                total += labBonus;

            return total / Divisor;
        }
    }
}