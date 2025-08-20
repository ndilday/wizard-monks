using WizardMonks.Instances;
using WizardMonks.Models.Beliefs;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Covenants;
using WizardMonks.Services.Characters;

namespace WizardMonks.Activities.ExposingActivities
{
    public class RecruitHedgeMageActivity : AExposingActivity
    {
        public HedgeMagus Target { get; private set; }
        public Magus Master { get; private set; }
        public bool ForceSuccess { get; set; } // For scripting the Founding

        public RecruitHedgeMageActivity(HedgeMagus target, Magus master, Ability exposure, double desire)
            : base(exposure, desire)
        {
            Action = Activity.RecruitHedgeMage;
            Target = target;
            Master = master;
            ForceSuccess = false;
        }

        protected override void DoAction(Character character)
        {
            var recruiter = (Magus)character;

            // Contested roll: Recruiter's Charm/Intrigue vs. Target's Folk Ken/Resolve
            bool success = ForceSuccess || /* contested roll logic */ true;

            if (success)
            {
                // Convert the HedgeMagus to a Hermetic Magus (in apprentice state)
                var newHermeticMagus = Target.BecomeHermeticMagus(HousesEnum.Apprentice);

                // Bonisagus (the Master) now "learns of" this new magus.
                // Trianoma's word is trusted, so confidence is high (e.g., 0.9).
                var profileForMaster = new BeliefProfile(SubjectType.Character, 0.9);
                profileForMaster.AddOrUpdateBelief(new(BeliefTopics.HedgeMage, 1.0));
                Master.AddOrUpdateKnowledge(newHermeticMagus, profileForMaster);

                // Bonisagus's own belief profile about the new magus is updated.
                var bonisagusProfile = newHermeticMagus.GetBeliefProfile(Master);
                bonisagusProfile.AddOrUpdateBelief(new(BeliefTopics.HermeticTeacher, 1.0));

                // Move them to the Master's covenant
                Master.Covenant.AddMagus(newHermeticMagus, CovenantRole.Visitor);
                recruiter.GetBeliefProfile(Target).AddOrUpdateBelief(new(BeliefTopics.HedgeMage, 0));

                recruiter.Log.Add($"Successfully recruited {Target.Name}. They have been sent to {Master.Name} for training.");
            }
            else
            {
                recruiter.Log.Add($"Failed to recruit {Target.Name}. They remain independent.");
                // Future: This is where a Flambeau might generate a "ConflictGoal".
            }
        }

        public override bool Matches(IActivity action)
        {
            return action.Action == Activity.RecruitHedgeMage;
        }

        public override string Log()
        {
            return "Recruiting Hedge Mage worth " + Desire.ToString("0.000");
        }
    }
}