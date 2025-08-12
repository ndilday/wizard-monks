using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardMonks.Models.Ideas
{
    public class SpellIdea : AIdea
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
