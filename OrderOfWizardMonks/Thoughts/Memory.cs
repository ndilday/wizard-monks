using System;
using System.Collections.Generic;


namespace WizardMonks.Thoughts
{
    public class Memory
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
                if(ThoughtMap.TryGetValue(category, out List<Thought> value))
                {
                    value.Add(thought);
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
