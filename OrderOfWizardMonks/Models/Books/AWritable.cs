using System;
using System.Collections.Generic;
using WizardMonks.Models.Beliefs;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Ideas;
using WizardMonks.Models.Research;

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
        public AIdea EmbeddedIdea { get; set; }
        public BreakthroughDefinition EmbeddedBreakthrough { get; set; }
        public List<Belief> BeliefPayload { get; set; } = [];
    }
}
