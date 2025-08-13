using System.Collections.Generic;

namespace WizardMonks.Models.Beliefs
{
    public class BeliefProfile
    {
        private readonly Dictionary<string, Belief> _beliefs = [];

        public void AddOrUpdateBelief(Belief newBelief)
        {
            if (_beliefs.TryGetValue(newBelief.Topic, out var existingBelief))
            {
                // Logic for updating a belief - maybe it averages, maybe it adds.
                // For now, let's just add to the magnitude.
                existingBelief.Magnitude += newBelief.Magnitude;
            }
            else
            {
                _beliefs[newBelief.Topic] = newBelief;
            }
        }

        public double GetBeliefMagnitude(string topic)
        {
            return _beliefs.TryGetValue(topic, out var belief) ? belief.Magnitude : 0.0;
        }
    }
}
