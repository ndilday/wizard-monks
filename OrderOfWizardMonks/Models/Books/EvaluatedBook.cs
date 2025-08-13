
using WizardMonks.Models.Characters;

namespace WizardMonks.Models.Books
{
    public class EvaluatedBook
    {
        public ABook Book { get; set; }
        public double PerceivedValue { get; set; }
        public Ability ExposureAbility { get; set; }
    }
}
