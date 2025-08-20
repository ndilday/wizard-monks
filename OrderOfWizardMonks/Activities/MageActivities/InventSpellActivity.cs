using System;
using System.Linq;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Projects;
using WizardMonks.Models.Spells;
using WizardMonks.Services.Characters;

namespace WizardMonks.Activities.MageActivities
{
    public class InventSpellActivity : AExposingMageActivity
    {
        public Guid ProjectId { get; private set; }

        public InventSpellActivity(Guid projectId, Ability exposure, double desire)
            : base(exposure, desire)
        {
            ProjectId = projectId;
            Action = Activity.InventSpells;
        }

        protected override void DoMageAction(Magus mage)
        {
            var project = mage.ActiveProjects
                .OfType<SpellInventionProject>()
                .FirstOrDefault(p => p.ProjectId == ProjectId);
            if (project == null)
            {
                mage.Log.Add("Attempted to work on a non-existent spell invention project.");
                return;
            }
            Spell spell = project.SpellToInvent;
            double labTotal = mage.GetSpellLabTotal(spell);

            if (labTotal < spell.Level)
            {
                mage.Log.Add($"Lab conditions are no longer sufficient to continue inventing '{spell.Name}'.");
                return;
            }

            double progressThisSeason = labTotal - spell.Level;
            project.AddProgress(progressThisSeason);

            mage.Log.Add($"Worked on inventing '{spell.Name}'. Progress: {project.Progress:F1}/{spell.Level:F0}.");

            if (project.IsComplete)
            {
                mage.LearnSpell(spell); // This is a new private helper method we will create
                mage.ActiveProjects.Remove(project);
                mage.Log.Add($"Successfully invented '{spell.Name}'!");
            }
        }

        public override bool Matches(IActivity action)
        {
            if(action is InventSpellActivity invent)
            {
                return invent.ProjectId == ProjectId;
            }
            return false;
        }

        public override string Log()
        {
            return "Inventing a spell worth " + Desire.ToString("0.000");
        }
    }

}
