using System.Collections.Generic;
using WizardMonks.Models;

namespace WizardMonks.Instances
{
    public static class LabFeatures
    {
        public static readonly LabFeature HighlyOrganized;
        public static readonly LabFeature Spotless;
        public static readonly LabFeature HiddenDefect;

        static LabFeatures()
        {
            HighlyOrganized = new LabFeature("Highly Organized", 0, 0, 0, 1, 0, 0, 0, [], []);

            Spotless = new LabFeature("Spotless", 0, 1, 1, 0, 0, 0, 0, new Dictionary<Ability, double> { { MagicArts.Creo, 1 } }, []);

            HiddenDefect = new LabFeature("Hidden Defect", 0, 0, 0, 0, -3, 0, 0, [], []);

            // We would continue to add all other Virtues and Flaws here...
        }
    }
}
