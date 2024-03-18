using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardMonks.Thoughts
{
    public class Thought
    {
        public List<string> Categories { get; private set; }
        public Thought() 
        {
            Categories = [];
        }
    }
}
