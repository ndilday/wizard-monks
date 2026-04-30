using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Events;

namespace WizardMonks.Services.Characters
{
    /// <summary>
    /// Synthesizes patterns from unprocessed memory entries and updates a character's
    /// belief profiles accordingly.
    /// </summary>
    public sealed class ReflectionEngine
    {
        /// <summary>
        /// Runs one reflection pass for a character.
        /// </summary>
        public void Reflect(
            Character character,
            CharacterMemoryStream memoryStream,
            CharacterBeliefStore beliefStore,
            int currentTick)
        {
            var unprocessed = memoryStream.GetUnprocessed().ToList();
            if (unprocessed.Count == 0) return;

            var grouped = GroupBySubject(character, unprocessed);

            foreach (var (subjectId, entries) in grouped)
            {
                var profile = subjectId == character.Id
                    ? beliefStore.SelfBeliefs
                    : beliefStore.GetOrCreate(subjectId);

                SynthesizeBeliefs(profile, entries, currentTick);
            }

            foreach (var entry in unprocessed)
                memoryStream.MarkProcessed(entry.Id);
        }

        private static Dictionary<Guid, List<MemoryEntry>> GroupBySubject(
            Character character,
            IReadOnlyList<MemoryEntry> entries)
        {
            var groups = new Dictionary<Guid, List<MemoryEntry>>();

            foreach (var entry in entries)
            {
                Guid subjectId = entry.SourceEvent.Subject?.Id ?? character.Id;

                if (!groups.TryGetValue(subjectId, out var list))
                {
                    list = new List<MemoryEntry>();
                    groups[subjectId] = list;
                }
                list.Add(entry);
            }

            return groups;
        }

        private static void SynthesizeBeliefs(
            CharacterBeliefProfile profile,
            IReadOnlyList<MemoryEntry> entries,
            int currentTick)
        {
            var evidence = new Dictionary<string, (float WeightedValue, float TotalWeight)>();

            foreach (var entry in entries)
            {
                foreach (var (dimension, value, weight) in ExtractEvidence(entry))
                {
                    if (!evidence.TryGetValue(dimension, out var existing))
                        evidence[dimension] = (value * weight, weight);
                    else
                        evidence[dimension] = (
                            existing.WeightedValue + value * weight,
                            existing.TotalWeight + weight);
                }
            }

            foreach (var (dimension, (weightedValue, totalWeight)) in evidence)
            {
                if (totalWeight < 0.001f) continue;
                float evidenceValue = weightedValue / totalWeight;
                profile.Upsert(dimension, evidenceValue, totalWeight, currentTick);
            }
        }

        /// <summary>
        /// Extracts (dimension, value, weight) tuples from a single memory entry.
        ///
        /// The subject of the source event determines which belief profile receives
        /// this evidence — that routing is handled by GroupBySubject. Here we only
        /// decide what the evidence says about that subject.
        ///
        /// Evidence dimensions use the canonical BeliefTopic string keys so they
        /// remain compatible with the existing BeliefTopic registry.
        /// </summary>
        private static IEnumerable<(string dimension, float value, float weight)>
            ExtractEvidence(MemoryEntry entry)
        {
            float importance = entry.ImportanceWeight;
            bool positive = entry.SourceEvent.IsPositiveOutcome ?? false;

            switch (entry.SourceEvent.Category)
            {
                case WorldEventCategory.LabSuccess:
                case WorldEventCategory.SpellInvented:
                case WorldEventCategory.BreakthroughMade:
                    // Subject is competent; positive outcome reinforces that.
                    yield return ("MagicalCompetence", positive ? 0.6f : -0.3f, importance);
                    break;

                case WorldEventCategory.LabFailure:
                    yield return ("MagicalCompetence", -0.4f, importance);
                    break;

                case WorldEventCategory.AgingCrisis:
                    // Subject is vulnerable — evidence about their mortality.
                    yield return ("Vulnerability", 0.7f, importance);
                    break;

                case WorldEventCategory.TeachingReceived:
                    // Subject is the teacher. Teaching is an act of generosity and
                    // competence — evidence of Trustworthiness and MagicalCompetence.
                    // The student also gains self-belief evidence (handled separately
                    // because the student is a Participant, not the Subject; the
                    // GroupBySubject routing directs this entry to the teacher's profile).
                    yield return ("Trustworthiness", positive ? 0.5f : -0.2f, importance);
                    yield return ("MagicalCompetence", positive ? 0.3f : 0.0f, importance);
                    break;

                case WorldEventCategory.BookReceived:
                case WorldEventCategory.LabTextReceived:
                    // Subject shared knowledge. Positive evidence of Trustworthiness.
                    yield return ("Trustworthiness", positive ? 0.4f : 0.0f, importance);
                    break;

                case WorldEventCategory.RecruitmentSucceeded:
                    // Subject brought someone into the fold — social and cooperative evidence.
                    yield return ("Sociability", 0.5f, importance);
                    yield return ("Trustworthiness", 0.4f, importance);
                    break;

                case WorldEventCategory.RecruitmentFailed:
                    yield return ("Sociability", -0.3f, importance);
                    break;

                case WorldEventCategory.ApprenticeFound:
                case WorldEventCategory.ApprenticeGauntleted:
                    yield return ("Trustworthiness", 0.5f, importance);
                    break;

                case WorldEventCategory.ApprenticeLost:
                    yield return ("Trustworthiness", -0.6f, importance);
                    break;

                    // Categories that produce emotion tokens but no belief evidence yet.
                    // AgingNormal, LongevityRitualVoided, AuraChanged, VisSourceDiscovered,
                    // CovenantFounded, CharacterObserved, ScenarioEvent — extend here as needed.
            }
        }
    }
}