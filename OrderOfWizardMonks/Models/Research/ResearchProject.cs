using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Models.Characters;

namespace WizardMonks.Models.Research
{
    public class ResearchProject
    {
        public Guid Id { get; private set; }
        public Magus Researcher { get; private set; }
        public string ResearchGoal { get; private set; } 
        public BreakthroughDefinition Breakthrough { get; private set; }

        public List<ResearchProjectPhase> CompletedPhases { get; private set; }
        public ResearchProjectPhase CurrentPhase { get; private set; }

        public double BreakthroughPointsAccumulated => CompletedPhases.Sum(p => p.BreakthroughPointsGained) - 5;
        public ushort BreakthroughPointsRequired => Breakthrough.BreakthroughPointsRequired;

        public bool HasAchievedDiscovery { get; set; } = false;

        public ResearchProject(Magus researcher, BreakthroughDefinition target)
        {
            this.Breakthrough = target;
            this.Researcher = researcher;
        }
    }
}
