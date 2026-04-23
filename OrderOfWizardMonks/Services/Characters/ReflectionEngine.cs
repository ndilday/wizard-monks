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

        private static IEnumerable<(string dimension, float value, float weight)>
            ExtractEvidence(MemoryEntry entry)
        {
            float importance = entry.ImportanceWeight;

            switch (entry.SourceEvent.Category)
            {
                case WorldEventCategory.LabSuccess:
                case WorldEventCategory.SpellInvented:
                case WorldEventCategory.BreakthroughMade:
                    yield return ("MagicalCompetence",
                        entry.SourceEvent.IsPositiveOutcome == true ? 0.6f : -0.3f,
                        importance);
                    break;

                case WorldEventCategory.LabFailure:
                    yield return ("MagicalCompetence", -0.4f, importance);
                    break;

                case WorldEventCategory.AgingCrisis:
                    yield return ("Vulnerability", 0.7f, importance);
                    break;

                case WorldEventCategory.RecruitmentSucceeded:
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
            }
        }
    }
}