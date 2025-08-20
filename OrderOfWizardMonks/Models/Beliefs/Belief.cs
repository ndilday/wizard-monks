namespace WizardMonks.Models.Beliefs
{
    public class Belief
    {
        public string Topic { get; private set; }
        public double Magnitude { get; set; } // The strength of this belief.

        public Belief(string topic, double magnitude)
        {
            Topic = topic;
            Magnitude = magnitude;
        }

        public Belief(BeliefTopic topic, double magnitude) : this(topic.Name, magnitude) { }
    }
}
