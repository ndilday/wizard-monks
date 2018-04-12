using System.Collections.Generic;
using WizardMonks.Decisions.Conditions;
using WizardMonks.Instances;

namespace WizardMonks.Decisions.Goals
{
    
    public class LongevityRitualGoal : AGoal
    {
        private static readonly List<Ability> visTypes = new List<Ability>(){ MagicArts.Creo, MagicArts.Vim };
        private readonly VisCondition _visCondition;
        public LongevityRitualGoal(Magus magus, double desire, uint ageToCompleteBy = 140) : base(magus, desire, ageToCompleteBy)
        {
            _visCondition = new VisCondition(magus, ageToCompleteBy, desire, visTypes, (magus.SeasonalAge / 20) + 1, 1);
            Conditions.Add(_visCondition);
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log)
        {
            _visCondition.ModifyVisAmount((Character.SeasonalAge / 20) + 1);
            string startingLog = "Interested in making a longevity ritual, desire " + Desire.ToString("0.0");
            log.Add(startingLog);
            base.AddActionPreferencesToList(alreadyConsidered, log);
        }

        public override void ModifyVisDesires(Magus magus, VisDesire[] visDesires)
        {
            base.ModifyVisDesires(magus, visDesires);
        }
    }
}
