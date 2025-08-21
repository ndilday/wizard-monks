using WizardMonks.Core;
using WizardMonks.Models.Spells;

namespace WizardMonks.Models.Projects
{
    public class ResearchProjectPhase
    {
        public Spell ExperimentalSpell { get; private set; }
        public double InventionProgress { get; private set; }
        public bool IsInvented => InventionProgress >= ExperimentalSpell.Level;
        public bool IsStabilized { get; private set; } = false;
        public int SeasonsToStabilize { get; private set; }
        public int BreakthroughPointsGained { get; private set; }

        public ResearchProjectPhase(Spell spell, int seasonsToInvent)
        {
            ExperimentalSpell = spell;
            InventionProgress = 0;
            // The seasons to stabilize is often related to the seasons it took to invent.
            SeasonsToStabilize = seasonsToInvent;
        }

        public void AddInventionProgress(double progress)
        {
            if (!IsInvented)
            {
                InventionProgress += progress;
            }
        }

        public void WorkOnStabilization()
        {
            if (IsInvented && !IsStabilized)
            {
                SeasonsToStabilize--;
                if (SeasonsToStabilize <= 0)
                {
                    Stabilize();
                }
            }
        }

        private void Stabilize()
        {
            IsStabilized = true;
            BreakthroughPointsGained = SpellLevelMath.GetMagnitudesFromLevel(ExperimentalSpell.Level);
        }
    }
}