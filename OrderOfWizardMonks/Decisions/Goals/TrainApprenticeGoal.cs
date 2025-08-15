using System.Collections.Generic;
using System.Linq;
using WizardMonks.Activities.ExposingActivities;
using WizardMonks.Activities.MageActivities;
using WizardMonks.Instances;
using WizardMonks.Models.Characters;

namespace WizardMonks.Decisions.Goals
{
    public class TrainApprenticeGoal : AGoal
    {
        private readonly Magus _apprentice;
        private bool _seasonallyComplete = false;

        public TrainApprenticeGoal(Magus master, uint deadline, double desire)
         : base(master, deadline, desire)
        {
            _apprentice = master.Apprentice;
            if (_apprentice == null)
            {
                _completed = true;
                return;
            }
            Desire = desire * master.Personality.GetDesireMultiplier(HexacoFacet.Prudence);
        }

        // The goal is truly complete only when the apprentice is gone.
        // The seasonal requirement is handled by the GoalGenerator creating/removing the goal.
        public override bool IsComplete() => ((Magus)Character).Apprentice == null || _seasonallyComplete;

        // New method for the GoalGenerator to call when training is done for the year.
        public void MarkAsSeasonallyComplete()
        {
            _seasonallyComplete = true;
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, Desires desires, IList<string> log)
        {
            if (IsComplete()) return;

            var master = (Magus)Character;

            // Calculate urgency based on the deadline for THIS YEAR'S training.
            double seasonsRemaining = (AgeToCompleteBy ?? master.SeasonalAge + 1) - master.SeasonalAge;
            if (seasonsRemaining <= 0) seasonsRemaining = 1; // Avoid division by zero if on the last season

            var subjectToTeach = GetNextSubjectToTeach(master, _apprentice);

            if (subjectToTeach.Key != null)
            {
                double teachDesire = Desire / seasonsRemaining;
                log.Add($"[Goal] Train apprentice {_apprentice.Name} in {subjectToTeach.Key.AbilityName}. Deadline in {seasonsRemaining} seasons. Desire: {teachDesire:F2}");

                var teachActivity = new TeachActivity(_apprentice, subjectToTeach.Key, Abilities.Teaching, teachDesire);
                alreadyConsidered.Add(teachActivity);
            }
            else
            {
                log.Add($"[Goal] Apprentice {_apprentice.Name}'s training complete. Preparing for Gauntlet.");
                alreadyConsidered.Add(new GauntletApprentice(Abilities.MagicTheory, Desire * 20));
            }
        }


        /// <summary>
        /// Determines the most important subject to teach the apprentice this season based on a core curriculum.
        /// </summary>
        private KeyValuePair<Ability, double> GetNextSubjectToTeach(Magus master, Magus apprentice)
        {
            // Core curriculum with target levels.
            var curriculum = new Dictionary<Ability, double>
            {
                { Abilities.MagicTheory, 5 },
                { Abilities.ParmaMagica, 5 },
                { Abilities.Latin, 4 },
                { Abilities.ArtesLiberales, 1 },
                { MagicArts.GetEnumerator().OrderByDescending(art => master.GetAbility(art).Value).First(), 5 } // Master's best Art
            };

            // Find the first subject in the curriculum that the apprentice hasn't mastered yet.
            return curriculum.FirstOrDefault(subject => apprentice.GetAbility(subject.Key).Value < subject.Value);
        }
    }
}