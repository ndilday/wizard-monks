using WizardMonks.Models.Projects;

namespace WizardMonks.Models.Ideas
{
    public class BreakthroughIdea : AIdea
    {
        public BreakthroughDefinition TargetBreakthrough { get; private set; }

        public BreakthroughIdea(BreakthroughDefinition breakthrough)
            : base($"Research into {breakthrough.Name}")
        {
            Type = IdeaType.Breakthrough; // Add this to the enum
            TargetBreakthrough = breakthrough;
        }
    }
}
