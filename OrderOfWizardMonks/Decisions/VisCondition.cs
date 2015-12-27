﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks.Decisions
{
    class VisCondition : ACondition
    {
        private Magus _magus;
        public List<Ability> VisTypes { get; private set; }
        public double AmountNeeded { get; private set; }

        public VisCondition(Character character, List<Ability> abilities, double totalNeeded) :
            base(character)
        {
            VisTypes = abilities;
            AmountNeeded = totalNeeded;
        }

        public VisCondition(Magus magus, Ability ability, double totalNeeded) :
            base(magus)
        {
            VisTypes = new List<Ability>(1);
            VisTypes.Add(ability);
            AmountNeeded = totalNeeded;
        }

        public override void AddActionPreferencesToList(ConsideredActions alreadyConsidered, IList<string> log)
        {
            throw new NotImplementedException();
        }

        public override bool ConditionFulfilled
        {
            get
            {
                double total = 0;
                foreach(Ability ability in VisTypes)
                {
                    total += _magus.GetVisCount(ability);
                }
                return total >= AmountNeeded;
            }
        }
    }
}
