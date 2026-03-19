using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Instances;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Traditions;

namespace WizardMonks.Services.Traditions
{
    /// <summary>
    /// Encapsulates all logic related to Opening the Gift — both the standard
    /// first Opening (producing a clean clone of the master's tradition) and
    /// Hermetic re-opening of an already-opened Gift (the founding scenario).
    ///
    /// Opening mechanics (from Hedge Magic Revised, Introduction):
    ///
    /// Ease Factor = 2 × (sum of apprentice's existing magical Art/Ability scores)
    ///             + 12 if Gift has already been Opened by any tradition
    ///             + 30 if this is a Hermetic Opening of an already-opened Gift
    ///
    /// Retention ratio (graduated, departing from RAW binary):
    ///   OpeningTotal  < EaseFactor         → Cannot open; ratio = 0
    ///   EaseFactor   <= OpeningTotal < 2×EF → ratio = (OT/EF) - 1.0, in [0, 1)
    ///   2×EaseFactor <= OpeningTotal        → ratio = 1.0 (full retention)
    ///
    /// The retention ratio is applied uniformly to all prior Art scores.
    /// Translated experience is then distributed across Hermetic Arts in
    /// proportion to the spell base distribution of the prior tradition.
    /// </summary>
    public class GiftOpeningService
    {
        /// <summary>
        /// Performs a standard first Opening of the Arts for an apprentice
        /// whose Gift has not previously been opened by any tradition.
        ///
        /// On success, the apprentice's Gift is opened with a clone of the
        /// master's tradition, lineage recorded. Returns the result.
        /// </summary>
        public GiftOpeningResult OpenArtsStandard(
            GiftedCharacter master,
            GiftedCharacter apprentice)
        {
            if (master.Tradition == null)
                throw new InvalidOperationException(
                    $"{master.Name} has no MagicalTradition and cannot open an apprentice's Gift.");
            if (apprentice.IsOpened)
                throw new InvalidOperationException(
                    $"{apprentice.Name}'s Gift is already open. Use ReOpenArtsHermetic instead.");

            var newTradition = master.Tradition.CloneForOpening();
            newTradition.RecordOpening(master, master.Tradition.Name);

            return new GiftOpeningResult(
                succeeded: true,
                retentionRatio: 1.0,
                tradition: newTradition,
                openingTotal: 0,
                easeFactor: 0);
        }

        /// <summary>
        /// Attempts a Hermetic Opening of the Arts for a GiftedCharacter whose
        /// Gift has already been Opened by a hedge tradition. This is the
        /// founding scenario: Bonisagus opening each pre-Hermetic Founder.
        ///
        /// The master's InVi Lab Total is used as the Opening Total (computed
        /// externally and passed in, to keep the service independent of the
        /// lab total calculation machinery).
        ///
        /// On success, the apprentice's prior Art experience is translated into
        /// Hermetic Arts according to the retention ratio and spell base
        /// distribution. On failure, the apprentice's tradition is unchanged.
        /// </summary>
        public GiftOpeningResult ReOpenArtsHermetic(
            GiftedCharacter master,
            GiftedCharacter apprentice,
            double openingTotal)
        {
            if (master.Tradition == null)
                throw new InvalidOperationException(
                    $"{master.Name} has no MagicalTradition.");
            if (!apprentice.IsOpened)
                throw new InvalidOperationException(
                    $"{apprentice.Name}'s Gift has not been opened yet. Use OpenArtsStandard instead.");

            double easeFactor = CalculateEaseFactor(apprentice, alreadyOpened: true, hermeticReopen: true);

            if (openingTotal < easeFactor)
            {
                return new GiftOpeningResult(
                    succeeded: false,
                    retentionRatio: 0,
                    tradition: null,
                    openingTotal: openingTotal,
                    easeFactor: easeFactor);
            }

            // Retention ratio: 0 at exactly EF, 1.0 at 2×EF, linear between.
            double retentionRatio = Math.Clamp((openingTotal / easeFactor) - 1.0, 0.0, 1.0);

            // Clone the master's tradition for the new opening.
            var newTradition = master.Tradition.CloneForOpening();

            // Record both the prior opening (preserved from the apprentice's
            // existing tradition) and the new Hermetic opening.
            // The lineage string from the prior tradition is carried forward
            // as the base, then the new opening is appended.
            if (!string.IsNullOrEmpty(apprentice.Tradition.Lineage))
            {
                // Manually prepend the prior lineage since CloneForOpening
                // starts with an empty lineage.
                newTradition.PrependLineage(apprentice.Tradition.Lineage);
            }
            newTradition.RecordOpening(master, master.Tradition.Name);

            // Translate pre-Hermetic Art experience into Hermetic Arts.
            TranslatePreHermeticArts(apprentice, newTradition, retentionRatio);

            return new GiftOpeningResult(
                succeeded: true,
                retentionRatio: retentionRatio,
                tradition: newTradition,
                openingTotal: openingTotal,
                easeFactor: easeFactor);
        }

        #region Private Helpers

        private static double CalculateEaseFactor(
            GiftedCharacter apprentice,
            bool alreadyOpened,
            bool hermeticReopen)
        {
            double sumOfScores = SumExistingMagicalScores(apprentice);
            double easeFactor = 2.0 * sumOfScores;

            if (alreadyOpened)
                easeFactor += 12;

            if (hermeticReopen)
                easeFactor += 30;

            return easeFactor;
        }

        /// <summary>
        /// Sums the scores of all tradition-defined magical abilities on the
        /// apprentice. These are the abilities wrapped in MagicalAbilityPrinciple
        /// concepts in the apprentice's current tradition.
        /// </summary>
        private static double SumExistingMagicalScores(GiftedCharacter apprentice)
        {
            if (!apprentice.IsOpened) return 0;

            return apprentice.Tradition.Concepts
                .Where(c => c.Principle is MagicalAbilityPrinciple)
                .Sum(c =>
                {
                    var ability = ((MagicalAbilityPrinciple)c.Principle).Ability;
                    return apprentice.GetAbility(ability).Value;
                });
        }

        /// <summary>
        /// Translates the apprentice's pre-Hermetic Art experience into the
        /// Hermetic Art scores on the new tradition's HermeticMagus (or
        /// GiftedCharacter), applying the retention ratio and distributing
        /// across Hermetic Arts according to the spell base distribution of
        /// the prior tradition.
        ///
        /// For each MagicalAbilityPrinciple in the prior tradition:
        ///   retainedExperience = originalExperience × retentionRatio
        ///   That experience is distributed across Hermetic Art pairs weighted
        ///   by the fraction of the prior tradition's spell bases mapping to each.
        ///   Each matched pair's Technique and Form each receive half the share.
        ///
        /// Note: this adds experience to the apprentice object directly.
        /// The caller must have already called OpenGift on the apprentice with
        /// the new tradition before this method is invoked, so that the
        /// Hermetic Art slots exist to receive the experience.
        /// </summary>
        private static void TranslatePreHermeticArts(
            GiftedCharacter apprentice,
            MagicalTradition newTradition,
            double retentionRatio)
        {
            if (retentionRatio <= 0) return;

            var artDistribution = apprentice.Tradition.ComputeSpellBaseArtDistribution();

            if (artDistribution.Count == 0)
            {
                // No spell bases defined — no Art translation possible.
                // The mage starts from zero in all Hermetic Arts.
                return;
            }

            foreach (var concept in apprentice.Tradition.Concepts
                         .Where(c => c.Principle is MagicalAbilityPrinciple))
            {
                var ability = ((MagicalAbilityPrinciple)concept.Principle).Ability;
                double originalExperience = apprentice.GetAbility(ability).Experience;
                double retainedExperience = originalExperience * retentionRatio;

                if (retainedExperience <= 0) continue;

                DistributeExperienceAcrossHermeticArts(
                    apprentice,
                    retainedExperience,
                    artDistribution);
            }
        }

        /// <summary>
        /// Distributes a pool of retained experience across Hermetic Arts
        /// on the apprentice, weighted by the art pair distribution from the
        /// prior tradition's spell bases.
        ///
        /// Each matched ArtPair receives (distribution fraction × experience pool),
        /// split evenly between Technique and Form.
        /// </summary>
        private static void DistributeExperienceAcrossHermeticArts(
            GiftedCharacter apprentice,
            double experiencePool,
            Dictionary<ArtPair, double> distribution)
        {
            foreach (var (artPair, fraction) in distribution)
            {
                double shareForPair = experiencePool * fraction;
                double sharePerArt = shareForPair / 2.0;

                apprentice.GetAbility(artPair.Technique).AddExperience(sharePerArt);
                apprentice.GetAbility(artPair.Form).AddExperience(sharePerArt);
            }
        }

        #endregion
    }

    /// <summary>
    /// The result of a Gift Opening attempt.
    /// </summary>
    public class GiftOpeningResult
    {
        public bool Succeeded { get; }
        public double RetentionRatio { get; }
        public MagicalTradition Tradition { get; }
        public double OpeningTotal { get; }
        public double EaseFactor { get; }

        public GiftOpeningResult(
            bool succeeded,
            double retentionRatio,
            MagicalTradition tradition,
            double openingTotal,
            double easeFactor)
        {
            Succeeded = succeeded;
            RetentionRatio = retentionRatio;
            Tradition = tradition;
            OpeningTotal = openingTotal;
            EaseFactor = easeFactor;
        }

        public string Describe()
        {
            if (!Succeeded)
                return $"Gift Opening failed. Opening Total {OpeningTotal:F1} did not meet Ease Factor {EaseFactor:F1}.";

            return RetentionRatio >= 1.0
                ? $"Gift fully opened. All prior capabilities retained. (OT {OpeningTotal:F1} / EF {EaseFactor:F1})"
                : $"Gift opened with partial retention ({RetentionRatio:P0}). (OT {OpeningTotal:F1} / EF {EaseFactor:F1})";
        }
    }
}