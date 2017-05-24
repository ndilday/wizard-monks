using System.Collections.Generic;
using WizardMonks.Decisions.Conditions;
using WizardMonks.Instances;

namespace WizardMonks.Decisions.Goals
{
    
    public class LongevityRitualGoal : AGoal
    {
        private static readonly List<Ability> visTypes = new List<Ability>(){ MagicArts.Creo, MagicArts.Vim };
        public LongevityRitualGoal(Magus magus, uint? ageToCompleteBy, double desire) : base(magus, ageToCompleteBy, desire)
        {
            this.Conditions.Add(new VisCondition(magus, 140, desire, visTypes, (magus.SeasonalAge / 20) + 1, 1));
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log)
        {
            this.Conditions[0] = new VisCondition((Magus)this.Conditions[0].Character, 140, this.Conditions[0].Desire, visTypes, (this.Conditions[0].Character.SeasonalAge / 20) + 1, 1);
            base.AddActionPreferencesToList(alreadyConsidered, log);
        }

        public override void ModifyVisDesires(Magus magus, VisDesire[] visDesires)
        {
            visDesires[14].Quantity += (magus.SeasonalAge / 20) + 1;
            visDesires[0].Quantity += (magus.SeasonalAge / 20) + 1;
        }
    }
}
