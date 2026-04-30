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

        /// <summary>
        /// Factory method for bootstrapping a mage's starting state with research phases
        /// that were completed before the simulation begins. The returned phase is marked
        /// fully invented and stabilized, with BreakthroughPointsGained computed from
        /// the spell's level exactly as <c>Stabilize()</c> would have done.
        ///
        /// Callers should use spells whose level yields the intended breakthrough points:
        ///   Level 3 → 3 points, Level 4 → 4 points, Level 10 → 6 points, etc.
        /// </summary>
        public static ResearchProjectPhase CreateCompleted(Spell spell)
        {
            var phase = new ResearchProjectPhase(spell, 0);
            phase.InventionProgress = spell.Level;   // fully invented
            phase.IsStabilized = true;
            phase.BreakthroughPointsGained = SpellLevelMath.GetMagnitudesFromLevel(spell.Level);
            return phase;
        }
    }
}