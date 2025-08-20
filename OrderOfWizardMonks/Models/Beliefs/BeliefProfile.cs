using System.Collections.Generic;

namespace WizardMonks.Models.Beliefs
{
    public enum SubjectType { Character, Covenant, Aura, Book, Spell, Idea, Other }

    public class BeliefProfile
    {
        public SubjectType Type { get; private set; }
        public double Confidence { get; set; }
        private readonly Dictionary<string, Belief> _beliefs;

        public BeliefProfile(SubjectType type, double initialConfidence)
        {
            Type = type;
            Confidence = initialConfidence;
            _beliefs = [];
        }

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

        public IEnumerable<Belief> GetAllBeliefs()
        {
            return _beliefs.Values;
        }

        public void RemoveBelief(string topic)
        {
            _beliefs?.Remove(topic);
        }
    }
}
