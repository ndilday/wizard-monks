using WizardMonks.Instances;
using WizardMonks.Models.Beliefs;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Covenants;
using WizardMonks.Services.Characters;
using WizardMonks.Services.Traditions;

namespace WizardMonks.Activities.ExposingActivities
{
    public class RecruitHedgeMageActivity : AExposingActivity
    {
        public HedgeMagus Target { get; private set; }
        public HermeticMagus Master { get; private set; }
        public bool ForceSuccess { get; set; } // For scripting the Founding

        private readonly GiftOpeningService _openingService;

        public RecruitHedgeMageActivity(
            HedgeMagus target,
            HermeticMagus master,
            GiftOpeningService openingService,
            Ability exposure,
            double desire)
            : base(exposure, desire)
        {
            Action = Activity.RecruitHedgeMage;
            Target = target;
            Master = master;
            _openingService = openingService;
            ForceSuccess = false;
        }

        protected override void DoAction(Character character)
        {
            var recruiter = (HermeticMagus)character;

            // Contested roll: Recruiter's Charm/Intrigue vs. Target's Folk Ken/Resolve
            // ForceSuccess allows the Founding scenario to bypass the roll for
            // canonical recruitments that must succeed.
            bool success = ForceSuccess || /* contested roll logic — TODO */ true;

            if (!success)
            {
                recruiter.Log.Add($"Failed to recruit {Target.Name}. They remain independent.");
                // Future: This is where a Flambeau might generate a ConflictGoal.
                return;
            }

            // Attempt to open the Target's Gift for Hermetic Arts.
            var newHermeticMagus = Target.BecomeHermeticMagus(
                Master,
                HousesEnum.Apprentice,
                _openingService);

            if (newHermeticMagus == null)
            {
                // Opening failed — Master's InVi total was insufficient.
                // The hedge mage remains independent.
                recruiter.Log.Add(
                    $"Recruited {Target.Name}, but {Master.Name}'s Opening Total " +
                    $"was insufficient to open their Gift for Hermetic Arts.");
                return;
            }

            // The Master now knows about this new potential student.
            // Trianoma's word is trusted, so confidence is high.
            var profileForMaster = new BeliefProfile(SubjectType.Character, 0.9);
            profileForMaster.AddOrUpdateBelief(new(BeliefTopics.HedgeMage, 1.0));
            Master.AddOrUpdateKnowledge(newHermeticMagus, profileForMaster);

            // The new mage's belief profile records the Master as their teacher.
            var newMagusProfileOfMaster = newHermeticMagus.GetBeliefProfile(Master);
            newMagusProfileOfMaster.AddOrUpdateBelief(new(BeliefTopics.HermeticTeacher, 1.0));

            // The recruiter updates their belief about the new mage
            // (replacing their prior HedgeMage belief with a known Hermetic one).
            recruiter.AddOrUpdateKnowledge(newHermeticMagus, profileForMaster);

            // Move the new mage to the Master's covenant.
            // BecomeHermeticMagus already does this if Master.Covenant is set,
            // but we guard here in case it was null at that point.
            if (Master.Covenant != null &&
                Master.Covenant.GetRoleForMagus(newHermeticMagus) == null)
            {
                Master.Covenant.AddMagus(newHermeticMagus, CovenantRole.Visitor);
            }

            recruiter.Log.Add(
                $"Successfully recruited {Target.Name}. They have joined the Order " +
                $"and been sent to {Master.Name} for training.");
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