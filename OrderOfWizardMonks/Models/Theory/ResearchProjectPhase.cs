using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WizardMonks.Core;
using WizardMonks.Models.Spells;

namespace WizardMonks.Models.Theory
{
    public class ResearchProjectPhase
    {
        public Spell ExperimentalSpell { get; private set; }
        public bool DiscoveryAchieved { get; set; } = false;
        public bool IsStabilized { get; set; } = false;
        public int SeasonsToStabilize { get; private set; }
        public int BreakthroughPointsGained { get; private set; }

        public ResearchProjectPhase(Spell spell, int seasonsToInvent)
        {
            ExperimentalSpell = spell;
            SeasonsToStabilize = seasonsToInvent;
        }

        public void Stabilize()
        {
            IsStabilized = true;
            BreakthroughPointsGained = SpellLevelMath.GetMagnitudesFromLevel(ExperimentalSpell.Level);
        }
    }
}
