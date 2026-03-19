using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Activities;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Spells;

namespace WizardMonks.Models.Traditions
{
    /// <summary>
    /// Represents the complete, current magical capabilities of a single mage's
    /// practice. Every GiftedCharacter whose Gift has been Opened owns one
    /// instance of this class. It is null on a GiftedCharacter whose Gift has
    /// not yet been Opened.
    ///
    /// A MagicalTradition is NOT a shared object. It is personal to the mage
    /// who holds it. The apparent similarity between Hermetic magi's traditions
    /// is an emergent consequence of shared lineage and knowledge-sharing culture,
    /// not a shared object reference.
    ///
    /// When a new concept spreads (via a lab text, book, or teaching), each mage
    /// who receives it integrates a copy into their own tradition instance.
    ///
    /// Lineage format: "OpenerName[TraditionName]>OpenerName[TraditionName]"
    /// Each ">" represents a re-opening event. For example, Flambeau's lineage
    /// after being opened by Bonisagus would be:
    ///   "UnknownMaster[FlambeuFireTradition]>Bonisagus[Hermetic Magic]"
    /// </summary>
    public class MagicalTradition
    {
        private readonly List<TraditionConcept> _concepts;
        private readonly List<TraditionActivityFormula> _activityFormulas;

        /// <summary>
        /// Human-readable name of the tradition (e.g., "Hermetic Magic",
        /// "Gruagach", "Elementalist").
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Narrative description of this tradition's character and origins.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Encoded provenance string. Format:
        ///   "OpenerName[TraditionName]>OpenerName[TraditionName]>..."
        /// Each segment represents one Opening event. Multiple segments mean
        /// the Gift was re-opened one or more times. Used for lineage analysis
        /// and historical reconstruction when ancestor objects are no longer
        /// in memory.
        /// </summary>
        public string Lineage { get; private set; }

        /// <summary>
        /// The GiftedCharacter who performed the most recent Opening of the
        /// Gift that created this tradition instance. This is the magical
        /// parent — immutable after Opening, distinct from the social
        /// apprenticeship relationship (HermeticMagus.Apprentice).
        /// Null only for the very first tradition in a lineage (Bonisagus
        /// himself, whose tradition was self-developed).
        /// </summary>
        public GiftedCharacter Opener { get; private set; }

        /// <summary>
        /// The divisor applied when computing spontaneous magic totals for
        /// fatiguing spontaneous casting. Hermetic magic uses 5. Traditions
        /// with superior spontaneous magic use a lower value.
        /// </summary>
        public double SpontaneousMagicDivisor { get; }

        /// <summary>
        /// The Ability that represents theoretical understanding of this
        /// tradition. For Hermetic magi this is MagicTheory. For hedge
        /// traditions it is their own Theory Ability. Every tradition has
        /// exactly one Theory Ability, even if no current practitioner has
        /// invested in it.
        /// </summary>
        public Ability TheoryAbility { get; }

        /// <summary>
        /// The complete set of capabilities this tradition currently contains.
        /// All concepts are currently known — no potential or speculative state
        /// is stored here. Potential expansions live on the mage as
        /// BreakthroughIdea instances.
        /// </summary>
        public IReadOnlyList<TraditionConcept> Concepts => _concepts;

        /// <summary>
        /// The formulas this tradition uses to compute activity totals.
        /// Each formula is keyed by Activity and encodes which abilities
        /// contribute, whether the aura is included, and any divisor.
        /// Queried by the lab service when computing totals for activities
        /// such as DistillVis, InventSpells, LongevityRitual, StudyVis, etc.
        /// </summary>
        public IReadOnlyList<TraditionActivityFormula> ActivityFormulas => _activityFormulas;

        public MagicalTradition(
            string name,
            string description,
            string lineage,
            double spontaneousMagicDivisor,
            Ability theoryAbility,
            IEnumerable<TraditionConcept> initialConcepts = null,
            IEnumerable<TraditionActivityFormula> activityFormulas = null,
            GiftedCharacter opener = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? string.Empty;
            Lineage = lineage ?? string.Empty;
            SpontaneousMagicDivisor = spontaneousMagicDivisor > 0
                ? spontaneousMagicDivisor
                : throw new ArgumentOutOfRangeException(
                    nameof(spontaneousMagicDivisor), "Must be greater than zero.");
            TheoryAbility = theoryAbility ?? throw new ArgumentNullException(nameof(theoryAbility));
            _concepts = initialConcepts != null
                ? new List<TraditionConcept>(initialConcepts)
                : [];
            _activityFormulas = activityFormulas != null
                ? new List<TraditionActivityFormula>(activityFormulas)
                : [];
            Opener = opener;
        }

        #region Concept Queries

        public bool HasConcept(TraditionConcept concept) =>
            _concepts.Any(c => c.Id == concept.Id);

        /// <summary>
        /// Returns all concepts whose principle is of the given type.
        /// </summary>
        public IEnumerable<TraditionConcept> GetConceptsOfType<T>() where T : ITraditionPrinciple =>
            _concepts.Where(c => c.Principle is T);

        /// <summary>
        /// Returns true if this tradition has a LabActivityPrinciple for
        /// the given activity, indicating the tradition supports it.
        /// </summary>
        public bool SupportsActivity(Activity activity) =>
            _concepts.Any(c =>
                c.Principle is LabActivityPrinciple lap &&
                lap.Activity == activity);

        /// <summary>
        /// Returns all SpellBase instances known to this tradition.
        /// Used by GiftOpeningService for the Art-distribution calculation.
        /// </summary>
        public IEnumerable<SpellBase> GetKnownSpellBases() =>
            _concepts
                .Where(c => c.Principle is SpellBasePrinciple)
                .Select(c => ((SpellBasePrinciple)c.Principle).SpellBase);

        /// <summary>
        /// Returns the activity formula for the given activity, or null if
        /// this tradition has no formula defined for it.
        /// </summary>
        public TraditionActivityFormula GetFormula(Activity activity) =>
            _activityFormulas.FirstOrDefault(f => f.Activity == activity);

        #endregion

        #region Concept Mutation

        /// <summary>
        /// Adds a new concept to this tradition, representing a capability
        /// gained through research completion, reading, or teaching.
        /// Duplicate concepts (by Id) are silently ignored.
        /// </summary>
        public void IntegrateConcept(TraditionConcept concept)
        {
            if (concept == null) throw new ArgumentNullException(nameof(concept));
            if (!HasConcept(concept))
                _concepts.Add(concept);
        }

        /// <summary>
        /// Seeds zero-cost concepts from another tradition into this one,
        /// adding any not already present. This is the mechanism by which
        /// reading a lab text propagates Minor Breakthrough concepts (new
        /// Ranges, Durations, etc.) to the reader's own tradition.
        ///
        /// Only concepts with BreakthroughPointsRequired == 0 propagate
        /// automatically. Higher-difficulty concepts require deliberate research.
        /// </summary>
        public void SeedConceptsFrom(MagicalTradition sourceTradition)
        {
            if (sourceTradition == null)
                throw new ArgumentNullException(nameof(sourceTradition));

            foreach (var concept in sourceTradition.Concepts
                         .Where(c => c.BreakthroughPointsRequired == 0))
            {
                IntegrateConcept(concept);
            }
        }

        #endregion

        #region Lineage & Opening

        /// <summary>
        /// Records a Gift Opening event in both the structured Opener field
        /// and the denormalized Lineage string. Called by GiftOpeningService
        /// when an Opening succeeds.
        ///
        /// For a first Opening: sets Lineage to "OpenerName[TraditionName]"
        /// For a re-Opening:    appends ">OpenerName[TraditionName]"
        /// </summary>
        public void RecordOpening(GiftedCharacter opener, string traditionName)
        {
            if (opener == null) throw new ArgumentNullException(nameof(opener));
            if (string.IsNullOrWhiteSpace(traditionName))
                throw new ArgumentException("Tradition name must be provided.", nameof(traditionName));

            Opener = opener;
            string segment = $"{opener.Name}[{traditionName}]";
            Lineage = string.IsNullOrEmpty(Lineage)
                ? segment
                : $"{Lineage}>{segment}";
        }

        /// <summary>
        /// Prepends an existing lineage string to this tradition's lineage.
        /// Used by GiftOpeningService when re-opening a Gift to carry forward
        /// the prior tradition's opening history before appending the new opening.
        /// Should only be called on a freshly cloned tradition before RecordOpening.
        /// </summary>
        internal void PrependLineage(string priorLineage)
        {
            if (string.IsNullOrEmpty(priorLineage)) return;
            Lineage = string.IsNullOrEmpty(Lineage)
                ? priorLineage
                : $"{priorLineage}>{Lineage}";
        }

        /// <summary>
        /// Returns the number of times this Gift has been Opened, derived
        /// from the Lineage string. A value of 1 means opened once (standard).
        /// A value of 2+ means the Gift was re-opened at least once.
        /// Returns 0 if Lineage is empty (self-developed tradition, e.g. Bonisagus).
        /// </summary>
        public int OpeningCount =>
            string.IsNullOrEmpty(Lineage)
                ? 0
                : Lineage.Count(c => c == '[');

        #endregion

        #region Art Distribution

        /// <summary>
        /// Computes the proportional distribution of this tradition's spell
        /// bases across Hermetic Art pairs. The result maps each ArtPair to
        /// its fraction of the total spell base count.
        ///
        /// Used by GiftOpeningService to translate pre-Hermetic Art scores
        /// into Hermetic Art experience during Gift re-opening.
        ///
        /// Returns an empty dictionary if the tradition has no spell bases.
        /// </summary>
        public Dictionary<ArtPair, double> ComputeSpellBaseArtDistribution()
        {
            var spellBases = GetKnownSpellBases().ToList();

            if (spellBases.Count == 0)
                return [];

            var counts = new Dictionary<ArtPair, int>(new ArtPairEqualityComparer());

            foreach (var spellBase in spellBases)
            {
                var pair = spellBase.ArtPair;
                if (counts.ContainsKey(pair))
                    counts[pair]++;
                else
                    counts[pair] = 1;
            }

            double total = spellBases.Count;
            return counts.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value / total,
                new ArtPairEqualityComparer());
        }

        #endregion

        #region Cloning

        /// <summary>
        /// Creates a deep clone of this tradition for use by a new apprentice
        /// whose Gift is being Opened from this master's tradition. The clone
        /// shares the same concept and formula instances (both are immutable)
        /// but holds separate list instances, so future additions to either
        /// tradition do not affect the other.
        ///
        /// The clone's Lineage and Opener are not set here — they are set
        /// by GiftOpeningService via RecordOpening after the Opening is
        /// confirmed successful.
        /// </summary>
        public MagicalTradition CloneForOpening()
        {
            return new MagicalTradition(
                Name,
                Description,
                lineage: string.Empty,
                SpontaneousMagicDivisor,
                TheoryAbility,
                _concepts,
                _activityFormulas,
                opener: null);
        }

        #endregion
    }

    /// <summary>
    /// Value-semantic equality comparer for ArtPair, used in dictionary
    /// operations. ArtPair does not override Equals/GetHashCode.
    /// </summary>
    internal class ArtPairEqualityComparer : IEqualityComparer<ArtPair>
    {
        public bool Equals(ArtPair x, ArtPair y)
        {
            if (x is null && y is null) return true;
            if (x is null || y is null) return false;
            return x.Technique.AbilityId == y.Technique.AbilityId
                && x.Form.AbilityId == y.Form.AbilityId;
        }

        public int GetHashCode(ArtPair obj)
        {
            if (obj is null) return 0;
            return HashCode.Combine(obj.Technique.AbilityId, obj.Form.AbilityId);
        }
    }
}