using System;

namespace WizardMonks.Core
{
    /// <summary>
    /// A static utility class to handle the non-linear arithmetic of Ars Magica spell levels and magnitudes.
    /// </summary>
    public static class SpellLevelMath
    {
        /// <summary>
        /// Converts a spell's level into its total number of effective magnitudes.
        /// Levels 1-5 correspond to 1-5 magnitudes. Above level 5, each magnitude adds 5 levels.
        /// </summary>
        /// <param name="level">The spell level.</param>
        /// <returns>The total number of magnitudes.</returns>
        public static ushort GetMagnitudesFromLevel(double level)
        {
            ushort shortLevel = (ushort)level;
            if (level <= 5)
            {
                return shortLevel;
            }
            // For levels > 5, it's the base 5 magnitudes plus one for every 5 levels above that.
            return (ushort)(5 + ((shortLevel - 5) / 5));
        }

        /// <summary>
        /// Converts a spell's level into its total number of effective magnitudes.
        /// Levels 1-5 correspond to 1-5 magnitudes. Above level 5, each magnitude adds 5 levels.
        /// </summary>
        /// <param name="level">The spell level.</param>
        /// <returns>The total number of magnitudes.</returns>
        public static ushort GetLevelFromMagnitude(double magnitude)
        {
            ushort shortMag = (ushort)magnitude;
            if (shortMag <= 5)
            {
                return shortMag;
            }
            // For magnitude > 5, it's the base 5 magnitudes plus five for every level above that.
            return (ushort)(shortMag * 5 - 20);
        }

        /// <summary>
        /// Calculates the difference in magnitudes between two spell levels
        /// </summary>
        /// <param name="larger">The larger spell level</param>
        /// <param name="lesser">The smaller spell level</param>
        /// <returns>The magnitude difference between levels</returns>
        public static ushort GetMagnitudeDifferenceBetweenLevels(ushort larger, ushort lesser)
        {
            if (lesser < larger) return 0;
            return (ushort)(GetMagnitudesFromLevel(larger) - GetMagnitudesFromLevel(lesser));
        }
    }
}