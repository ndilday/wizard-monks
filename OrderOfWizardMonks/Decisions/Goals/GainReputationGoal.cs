using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Activities;
using WizardMonks.Activities.ExposingActivities;
using WizardMonks.Decisions.Conditions;
using WizardMonks.Decisions.Conditions.Helpers;
using WizardMonks.Instances;
using WizardMonks.Models.Books;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Spells;
using WizardMonks.Services.Characters;

namespace WizardMonks.Decisions.Goals
{
    public class GainReputationGoal : AGoal
    {
        public GainReputationGoal(Magus magus, double desire)
            : base(magus, null, desire) // Lifelong ambition
        {
            Desire *= magus.Personality.GetPrestigeMotivation();
        }

        public override bool IsComplete() => false; // This goal is a continuous driver of behavior

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, Desires desires, IList<string> log)
        {
            var magus = (Magus)Character;
            var selfProfile = magus.GetBeliefProfile(magus);

            // Find all topics the magus wants a reputation in (has a self-belief about)
            var reputationalFoci = selfProfile.GetAllBeliefs().Where(b => b.Magnitude > 0);

            if (!reputationalFoci.Any())
            {
                // If no specific focus, default to their highest Art
                var bestArt = magus.Arts.OrderByDescending(a => a.Value).First().Ability;
                reputationalFoci = new List<Models.Beliefs.Belief> { new(bestArt.AbilityName, 10) };
            }

            foreach (var focus in reputationalFoci)
            {
                if (Abilities.AbilityDictionary.TryGetValue(focus.Topic, out Ability abilityTopic))
                {
                    // Path 1: Reputation through Books
                    EvaluateWritingBookForReputation(magus, abilityTopic, focus.Magnitude, alreadyConsidered, desires, log);

                    // Path 2: Reputation through Spells/Lab Texts (if the focus is an Art)
                    if (MagicArts.IsArt(abilityTopic))
                    {
                        EvaluateInventingSpellForReputation(magus, abilityTopic, focus.Magnitude, alreadyConsidered, desires, log);
                    }
                }
            }
        }

        private void EvaluateWritingBookForReputation(Magus magus, Ability topic, double focusStrength, ConsideredActions alreadyConsidered, Desires desires, IList<string> log)
        {
            // A. Consider writing a book NOW
            var prospectiveBook = GetProspectiveBook(magus, topic);
            if (prospectiveBook != null)
            {
                double currentReputationValue = CalculateReputationValueForBook(magus, prospectiveBook);
                double desireToWriteNow = Desire * focusStrength * (currentReputationValue / 100.0); // Normalize
                log.Add($"[Reputation] Considering writing '{prospectiveBook.Title}' now for {topic.AbilityName} fame, worth {desireToWriteNow:F2}");
                alreadyConsidered.Add(new WriteActivity(topic, prospectiveBook.Title, Abilities.Latin, prospectiveBook.Level, desireToWriteNow));
            }

            // B. Consider improving the underlying ability FIRST for a better book later
            CalculateDesireFunc desireFunc = (gain, depth) =>
            {
                var improvedAbility = magus.GetAbility(topic).MakeCopy();
                improvedAbility.AddExperience(gain * 10); // Heuristic: estimate XP needed for value gain
                var futureBook = GetProspectiveBook(magus, topic, improvedAbility.Value);
                double futureReputationValue = CalculateReputationValueForBook(magus, futureBook);
                double marginalGain = futureReputationValue - (prospectiveBook != null ? CalculateReputationValueForBook(magus, prospectiveBook) : 0);
                return (Desire * focusStrength * (marginalGain / 100.0)) / depth;
            };

            var increaseHelper = new AbilityIncreaseHelper(magus, magus.SeasonalAge + 40, 2, topic, desireFunc);
            increaseHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);
        }

        private void EvaluateInventingSpellForReputation(Magus magus, Ability topic, double focusStrength, ConsideredActions alreadyConsidered, Desires desires, IList<string> log)
        {
            if (magus.Laboratory == null)
            {
                HasLabCondition hasLab = new HasLabCondition(magus, (ushort)(AgeToCompleteBy - 1), Desire, 2);
            }
            var artPair = FindBestArtPairForTopic(magus, topic);
            if (artPair == null) return;

            // A. Consider inventing an impressive spell NOW
            double currentLabTotal = magus.GetLabTotal(artPair, Activity.InventSpells);
            double maxLevelNow = currentLabTotal / 2.0;
            if (maxLevelNow > 5)
            {
                double currentReputationValue = maxLevelNow * 1.5; // Simple heuristic for now
                double desireToInventNow = Desire * focusStrength * (currentReputationValue / 100.0);
                log.Add($"[Reputation] Considering inventing a Level {maxLevelNow:F0} {topic.AbilityName} spell for fame, worth {desireToInventNow:F2}");
                // TODO: To make this actionable, we need a concrete spell to invent.
                // We'll create a placeholder spell idea for the helper.
                var placeholderSpellBase = new SpellBase(
                    TechniqueEffects.Manipulate, // Placeholder effect
                    FormEffects.Animal,          // Placeholder effect
                    (SpellArts)Enum.Parse(typeof(SpellArts), artPair.Technique.AbilityName) | (SpellArts)Enum.Parse(typeof(SpellArts), artPair.Form.AbilityName),
                    artPair,
                    SpellTag.Utility,
                    (ushort)(maxLevelNow / 5), // Approximate magnitude
                    $"Impressive {topic.AbilityName} Work"
                );

                var spellHelper = new LearnSpellHelper(magus, magus.SeasonalAge + 4, 1, placeholderSpellBase, (gain, depth) => desireToInventNow);
                spellHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);
            }

            // B. Consider improving the Lab Total FIRST for a better spell later
            CalculateDesireFunc desireFunc = (gain, depth) =>
            {
                double futureLabTotal = currentLabTotal + gain;
                double maxLevelFuture = futureLabTotal / 2.0;
                double futureReputationValue = maxLevelFuture * 1.5;
                double marginalGain = futureReputationValue - (maxLevelNow > 0 ? maxLevelNow * 1.5 : 0);
                return (Desire * focusStrength * (marginalGain / 100.0)) / depth;
            };

            var labTotalHelper = new LabTotalIncreaseHelper(magus, magus.SeasonalAge + 40, 2, artPair, Activity.InventSpells, desireFunc);
            labTotalHelper.AddActionPreferencesToList(alreadyConsidered, desires, log);
        }

        private ArtPair FindBestArtPairForTopic(Magus magus, Ability topic)
        {
            // Ensure the topic is a valid magic Art before proceeding.
            if (!MagicArts.IsArt(topic))
            {
                return null;
            }

            ArtPair bestPair = null;
            double maxLabTotal = double.MinValue; // Start with a very low value to ensure any valid total is higher.

            if (MagicArts.IsTechnique(topic))
            {
                // The topic is a Technique, so we find the best Form to pair it with.
                var forms = MagicArts.GetEnumerator().Where(a => MagicArts.IsForm(a));
                foreach (var form in forms)
                {
                    var currentPair = new ArtPair(topic, form);
                    // We use InventSpells as the activity since that's the context for this evaluation.
                    double currentLabTotal = magus.GetLabTotal(currentPair, Activity.InventSpells);
                    if (currentLabTotal > maxLabTotal)
                    {
                        maxLabTotal = currentLabTotal;
                        bestPair = currentPair;
                    }
                }
            }
            else // The topic must be a Form.
            {
                // The topic is a Form, so we find the best Technique to pair it with.
                var techniques = MagicArts.GetEnumerator().Where(a => MagicArts.IsTechnique(a));
                foreach (var technique in techniques)
                {
                    var currentPair = new ArtPair(technique, topic);
                    double currentLabTotal = magus.GetLabTotal(currentPair, Activity.InventSpells);
                    if (currentLabTotal > maxLabTotal)
                    {
                        maxLabTotal = currentLabTotal;
                        bestPair = currentPair;
                    }
                }
            }
            return bestPair;
        }

        private ABook GetProspectiveBook(Magus author, Ability topic, double futureAbilityScore = -1)
        {
            double abilityScore = futureAbilityScore == -1 ? author.GetAbility(topic).Value : futureAbilityScore;
            if (author.CanWriteTractatus(author.GetAbility(topic)))
            {
                return new Tractatus { Author = author, Quality = 6 + author.GetAttributeValue(AttributeType.Communication), Topic = topic, Title = "Prospective Tractatus" };
            }
            else
            {
                double level = abilityScore / 2.0;
                if (level < 1) return null;
                return new Summa { Author = author, Level = level, Quality = 6 + author.GetAttributeValue(AttributeType.Communication), Topic = topic, Title = "Prospective Summa" };
            }
        }

        private double CalculateReputationValueForBook(Magus author, ABook book)
        {
            var payload = author.GenerateProspectiveBeliefPayload(book);
            return payload.Sum(b => author.CalculateBeliefValue(b));
        }
    }
}