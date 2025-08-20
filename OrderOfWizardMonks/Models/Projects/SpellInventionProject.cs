using System;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Spells;

namespace WizardMonks.Models.Projects
{
    [Serializable]
    public class SpellInventionProject : AProject
    {
        public Spell SpellToInvent { get; private set; }
        public double Progress { get; private set; }

        public override string Description => $"Inventing the spell '{SpellToInvent.Name}' (Lvl {SpellToInvent.Level})";
        public override bool IsComplete => Progress >= SpellToInvent.Level;

        public SpellInventionProject(Character owner, Spell spellToInvent) : base(owner)
        {
            SpellToInvent = spellToInvent;
            Progress = 0;
        }

        public void AddProgress(double amount)
        {
            Progress += amount;
        }
    }
}