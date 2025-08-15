using System.Collections.Generic;
using System.Linq;
using WizardMonks.Activities;
using WizardMonks.Activities.ExposingActivities;
using WizardMonks.Decisions.Conditions;
using WizardMonks.Instances;
using WizardMonks.Models.Characters;

namespace WizardMonks.Decisions.Goals
{
    public class LearnFromMasterGoal : AGoal
    {
        public Magus Master { get; private set; }

        public LearnFromMasterGoal(Magus student, Magus master, double desire)
            : base(student, student.SeasonalAge + 60, desire) // Deadline: 15 years (60 seasons)
        {
            Master = master;

            // Condition 1: Learn Parma Magica to at least level 1
            Conditions.Add(new AbilityScoreCondition(student, (uint)AgeToCompleteBy, desire, Abilities.ParmaMagica, 1));

            // Condition 2: Learn Magic Theory to at least level 1
            Conditions.Add(new AbilityScoreCondition(student, (uint)AgeToCompleteBy, desire, Abilities.MagicTheory, 1));

            // Condition 3 (Implicit): Participate in collaboration.
            // This is handled by setting MandatoryAction, not a formal condition.
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, Desires desires, IList<string> log)
        {
            // This goal is unique. Its purpose is to set up a mandatory collaboration.
            var student = (Magus)Character;

            // If the core learning is not yet done, we enforce the teaching/learning actions.
            if (!Conditions.TrueForAll(c => c.ConditionFulfilled))
            {
                // Student's mandatory action is to learn Parma (or Theory if Parma is done)
                var parmaCondition = (AbilityScoreCondition)Conditions.First(c => ((AbilityScoreCondition)c).Abilities.Contains(Abilities.ParmaMagica));
                if (!parmaCondition.ConditionFulfilled)
                {
                    student.MandatoryAction = new LearnActivity(GetTeachingQuality(Master, Abilities.ParmaMagica), Master.GetAbility(Abilities.ParmaMagica).Value, Abilities.ParmaMagica, Master);
                }
                else
                {
                    student.MandatoryAction = new LearnActivity(GetTeachingQuality(Master, Abilities.MagicTheory), Master.GetAbility(Abilities.MagicTheory).Value, Abilities.MagicTheory, Master);
                }
                student.IsCollaborating = true;

                // Set the Master's mandatory action as well
                Master.MandatoryAction = new TeachActivity(student, ((LearnActivity)student.MandatoryAction).Topic, Abilities.Teaching, Desire);
                Master.IsCollaborating = true;

                // Hedge wizard also helps Bonisagus in his lab, this can be a background activity for now.
                // A more complex model could have this be its own activity that produces research points.
            }
            else
            {
                // All learning conditions are met, the goal is complete.
                _completed = true;
            }
        }

        private double GetTeachingQuality(Magus teacher, Ability topic)
        {
            return teacher.GetAbility(Abilities.Teaching).Value + teacher.GetAttributeValue(AttributeType.Communication) + 6;
        }
    }
}