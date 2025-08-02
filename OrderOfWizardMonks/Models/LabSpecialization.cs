using WizardMonks.Activities;

namespace WizardMonks.Models
{
    public enum SpecializationStage
    {
        None,
        MinorFocus,
        MinorFeature,
        MajorFocus,
        MajorFeature
    }

    public class LabSpecialization
    {
        // A specialization can be for an Art OR an Activity, but not both.
        public Ability ArtTopic { get; private set; }
        public Activity ActivityTopic { get; private set; }
        public SpecializationStage Stage { get; private set; }

        public LabSpecialization(Ability art)
        {
            ArtTopic = art;
            ActivityTopic = Activity.Sundry; // Using Sundry as a placeholder for "not applicable"
            Stage = SpecializationStage.MinorFocus;
        }

        public LabSpecialization(Activity activity)
        {
            ActivityTopic = activity;
            ArtTopic = null;
            Stage = SpecializationStage.MinorFocus;
        }

        public LabSpecialization()
        {
            ArtTopic = null;
            ActivityTopic = Activity.Sundry;
            Stage = SpecializationStage.None;
        }

        public (double Quality, double Aesthetics, double Bonus) GetCurrentBonuses()
        {
            return Stage switch
            {
                SpecializationStage.MinorFocus => (-1, 1, 2),
                SpecializationStage.MinorFeature => (0, 1, 2),
                SpecializationStage.MajorFocus => (-1, 2, 4),
                SpecializationStage.MajorFeature => (0, 2, 4),
                _ => (0, 0, 0),
            };
        }

        public (int MagicTheory, int Refinement) GetPrerequisitesForNextStage()
        {
            return Stage switch
            {
                SpecializationStage.MinorFocus => (4, 1),   // To become MinorFeature
                SpecializationStage.MinorFeature => (5, 2), // To become MajorFocus
                SpecializationStage.MajorFocus => (6, 3),   // To become MajorFeature
                _ => (99, 99)                                // Maxed out
            };
        }

        public bool Upgrade()
        {
            if (Stage < SpecializationStage.MajorFeature)
            {
                Stage++;
                return true;
            }
            return false;
        }

        public string GetName()
        {
            string topic = ArtTopic?.AbilityName ?? ActivityTopic.ToString();
            return Stage switch
            {
                SpecializationStage.MinorFocus => $"Minor {topic} Focus",
                SpecializationStage.MinorFeature => $"Minor {topic} Feature",
                SpecializationStage.MajorFocus => $"Major {topic} Focus",
                SpecializationStage.MajorFeature => $"Major {topic} Feature",
                _ => "No Specialization"
            };
        }
    }
}
