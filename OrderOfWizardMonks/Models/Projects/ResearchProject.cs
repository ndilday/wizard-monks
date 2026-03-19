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
        public ResearchProject(HermeticMagus researcher, BreakthroughDefinition target) : base(researcher)
        {
            Breakthrough = target;
            CompletedPhases = new List<ResearchProjectPhase>();
            // Populated by ResearchService
            CurrentPhase = null;
        }

        public void StartNewPhase(ResearchProjectPhase phase)
        {
            CurrentPhase = phase;
        }
    }
}