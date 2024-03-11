using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WizardMonks.Thoughts
{
    internal class Memory
    {
        public Dictionary<string, List<Thought>> ThoughtMap {  get; private set; }
        public Memory() 
        {
            ThoughtMap = [];
        }

        public void AddThought(Thought thought)
        {
            foreach(string category in thought.Categories) 
            {
                if(ThoughtMap.ContainsKey(category))
                {
                    ThoughtMap[category].Add(thought);
                }
                else
                {
                    ThoughtMap[category] = [thought];
                }
            }
        }

        public bool DoesKnow(Thought thought)
        {
            foreach (string category in thought.Categories)
            {

                if (ThoughtMap.TryGetValue(category, out List<Thought> thoughtList))
                {
                    if (thoughtList.Contains(thought))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
