using System.Collections.Generic;
using WizardMonks.Decisions.Conditions;
using WizardMonks.Instances;

namespace WizardMonks.Decisions.Goals
{
    
    public class LongevityRitualGoal : AGoal
    {
        // TODO: Make this configurable
        // TODO: figure out a way to make vis desires split over multiple arts
        private static readonly List<Ability> visTypes = new List<Ability>(){ MagicArts.Creo, MagicArts.Vim };
        public LongevityRitualGoal(Magus magus, uint? ageToCompleteBy, double desire) : base(magus, ageToCompleteBy, desire)
        {
            // target longevity ritual for age 35
            // assume that it will happen at 35, and therefore require 7 vis
            this.Conditions.Add(new VisCondition(magus, 140, desire, visTypes, 7, 1));
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log)
        {
            base.AddActionPreferencesToList(alreadyConsidered, log);
        }

        public override void ModifyVisDesires(Magus magus, VisDesire[] visDesires)
        {
            foreach(Ability visAbility in visTypes)
            {
                visDesires[visAbility.AbilityId].Quantity += 7;
            }
        }
    }
}
