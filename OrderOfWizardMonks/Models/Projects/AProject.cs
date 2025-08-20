using System;
using WizardMonks.Models.Characters;

namespace WizardMonks.Models.Projects
{
    [Serializable]
    public abstract class AProject
    {
        public Guid ProjectId { get; private set; }
        public Character Owner { get; private set; }
        public abstract string Description { get; }
        public abstract bool IsComplete { get; }

        protected AProject(Character owner)
        {
            ProjectId = Guid.NewGuid();
            Owner = owner;
        }
    }
}