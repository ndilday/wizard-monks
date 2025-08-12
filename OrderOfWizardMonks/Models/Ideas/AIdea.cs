using System;

namespace WizardMonks.Models.Ideas
{
    public enum IdeaType
    {
        NewSpell,
        EnchantedItem, // Future use
        Breakthrough // Future use
    }

    /// <summary>
    /// Represents a unique idea or breakthrough a magus can have.
    /// This serves as a blueprint for a potential future project.
    /// </summary>
    public abstract class AIdea
    {
        public Guid Id { get; private set; }
        public IdeaType Type { get; protected set; }
        public string Description { get; protected set; }

        protected AIdea(string description)
        {
            Id = Guid.NewGuid();
            Description = description;
        }
    }
}