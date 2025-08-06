using System;

namespace WizardMonks.Models
{
    public enum IdeaType
    {
        NewSpell,
        EnchantedItem, // Future use
        OriginalResearch // Future use
    }

    /// <summary>
    /// Represents a unique idea or breakthrough a magus can have.
    /// This serves as a blueprint for a potential future project.
    /// </summary>
    public abstract class Idea
    {
        public Guid Id { get; private set; }
        public IdeaType Type { get; protected set; }
        public string Description { get; protected set; }

        protected Idea(string description)
        {
            Id = Guid.NewGuid();
            Description = description;
        }
    }

    public class SpellIdea : Idea
    {
        public ArtPair Arts { get; private set; }
        // We can add more properties later, like a suggested R/D/T, 
        // a required component, or a thematic link to a lab improvement.

        public SpellIdea(ArtPair arts, string description) : base(description)
        {
            Type = IdeaType.NewSpell;
            Arts = arts;
        }
    }
}