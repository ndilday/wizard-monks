using System.Collections.Generic;
using System.Linq;
using WizardMonks.Models.Characters;

namespace WizardMonks.Models.Projects
{
    public class ResearchProject : AProject
    {
        public BreakthroughDefinition Breakthrough { get; private set; }

        public List<ResearchProjectPhase> CompletedPhases { get; private set; }
        public ResearchProjectPhase CurrentPhase { get; private set; }

        // Implement abstract members from AProject
        public override string Description => $"Original Research: {Breakthrough.Name}";
        public override bool IsComplete => HasAchievedDiscovery;

        public double BreakthroughPointsAccumulated => CompletedPhases.Sum(p => p.BreakthroughPointsGained);
        public ushort BreakthroughPointsRequired => Breakthrough.BreakthroughPointsRequired;
        public bool HasAchievedDiscovery { get; set; } = false;

        // Modified constructor to call the base constructor
        public ResearchProject(Magus researcher, BreakthroughDefinition target) : base(researcher)
        {
            Breakthrough = target;
            CompletedPhases = new List<ResearchProjectPhase>();
            // Typically, a project would start with its first phase initialized.
            // This can be handled by the Goal/Helper that creates the project.
            CurrentPhase = null;
        }

        public void StartNewPhase()
        {
            // Logic to determine the next experimental spell would go here.
            // For now, we can placeholder this.
            // Spell experimentalSpell = ...
            // this.CurrentPhase = new ResearchProjectPhase(experimentalSpell);
        }
    }
}