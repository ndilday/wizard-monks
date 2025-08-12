using System;
using System.Collections.Generic;
using WizardMonks.Beliefs;
using WizardMonks.Models.Theory;

namespace WizardMonks.Models.Books
{
    [Serializable]
    public abstract class AWritable
    {
        public Character Author { get; set; }
        public string AuthorName
        {
            get
            {
                return Author.Name;
            }
        }
        public Idea EmbeddedIdea { get; set; }
        public BreakthroughDefinition EmbeddedBreakthrough { get; set; }
        public List<Belief> BeliefPayload { get; set; } = [];
    }
}
