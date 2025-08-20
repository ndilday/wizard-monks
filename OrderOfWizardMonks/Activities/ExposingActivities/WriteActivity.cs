using System;
using System.Linq;
using WizardMonks.Core;
using WizardMonks.Instances;
using WizardMonks.Models.Beliefs;
using WizardMonks.Models.Books;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Projects;
using WizardMonks.Services.Characters;

namespace WizardMonks.Activities.ExposingActivities
{
    [Serializable]
    public class WriteActivity : AExposingActivity
    {
        public Guid? ProjectId { get; private set; }
        public Ability Topic { get; private set; }
        public string Title { get; private set; }
        public double Level { get; private set; } // Used for Tractatus check and logging

        public WriteActivity(Ability topic, string title, Guid? projectId, Ability exposure, double level, double desire)
            : base(exposure, desire)
        {
            Topic = topic;
            Title = title;
            ProjectId = projectId;
            Level = level;
            Action = Activity.WriteBook;
        }

        protected override void DoAction(Character character)
        {
            if (!character.CanWrite())
            {
                character.Log.Add($"Cannot write; proficiency in {character.WritingLanguage.AbilityName} and {character.WritingCharacterAbility.Ability.AbilityName} is insufficient.");
                return;
            }

            if (ProjectId.HasValue) // This is a Summa project
            {
                WorkOnSumma(character);
            }
            else // This is a single-season Tractatus
            {
                WriteTractatus(character);
            }
        }

        private void WorkOnSumma(Character character)
        {
            var project = character.ActiveProjects.OfType<SummaWritingProject>().FirstOrDefault(p => p.ProjectId == ProjectId.Value);
            if (project == null)
            {
                character.Log.Add($"Attempted to work on a non-existent writing project for '{Title}'.");
                return;
            }

            double progressThisSeason = character.GetAttributeValue(AttributeType.Communication) + character.GetAbility(character.WritingLanguage).Value;
            project.AddProgress(progressThisSeason);

            if (project.IsComplete)
            {
                character.AddBookToCollection(project.Summa);
                character.BooksWritten.Add(project.Summa);
                character.ActiveProjects.Remove(project);
                character.Log.Add($"Finished writing {project.Summa.Title}: Q{project.Summa.Quality:0.0}, L{project.Summa.Level:0.0}");
            }
            else
            {
                character.Log.Add($"Worked on {project.Summa.Title}. Progress: {project.Progress:F0}/{project.PointsNeeded:F0}.");
            }
        }

        private void WriteTractatus(Character author)
        {
            Tractatus t = new()
            {
                Author = author,
                Quality = author.GetAttributeValue(AttributeType.Communication) + 6,
                Topic = this.Topic,
                Title = this.Title
            };

            // Generate Belief Payload
            t.BeliefPayload.Add(new Belief(Topic.AbilityName, BeliefToReputationNormalizer.CommunicationFromQuality(t.Quality) / 6.0));
            t.BeliefPayload.Add(new Belief(BeliefTopics.Communication.Name, BeliefToReputationNormalizer.FromAttributeScore(author.GetAttributeValue(AttributeType.Communication))));

            if (Die.Instance.RollDouble() < 0.10)
            {
                var randomFacet = (HexacoFacet)(Die.Instance.RollDouble() * 24);
                t.BeliefPayload.Add(new Belief(randomFacet.ToString(), BeliefToReputationNormalizer.FromPersonalityFacet(author.Personality.GetFacet(randomFacet))));
            }

            author.AddBookToCollection(t);
            author.BooksWritten.Add(t);
            author.Log.Add($"Wrote {t.Title}: Q{t.Quality:0.0}");
        }

        public override bool Matches(IActivity action)
        {
            if (action is not WriteActivity writing) return false;

            // If either has a ProjectId, they must match. If both are null, they match on Title/Topic.
            if (this.ProjectId.HasValue || writing.ProjectId.HasValue)
            {
                return this.ProjectId == writing.ProjectId;
            }
            return writing.Topic == Topic && writing.Title == Title;
        }

        public override string Log()
        {
            if (Level == 1000)
            {
                return "Writing tractatus on " + Topic.AbilityName + " worth " + Desire.ToString("0.000");
            }
            return "Writing summa on " + Topic.AbilityName + " worth " + Desire.ToString("0.000");
        }
    }
}