using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WizardMonks.Models.Books;
using WizardMonks.Models.Characters;

namespace WizardMonks.Activities.MageActivities
{
    public class LearnSpellFromLabTextActivity : AExposingMageActivity
    {
        public LabText LabText { get; set; }

        public LearnSpellFromLabTextActivity(LabText labText, Ability exposure, double desire)
            : base(exposure, desire)
        {
            LabText = labText;
            Action = Activity.InventSpells;
        }

        protected override void DoMageAction(Magus mage)
        {
            // TODO: multiple spells
            mage.LearnSpellFromLabText(LabText);
            mage.Log.Add($"Learning {LabText.SpellContained.Name} from lab text");
        }

        public override bool Matches(IActivity action)
        {
            if (action.Action != Activity.InventSpells)
            {
                return false;
            }
            if(action.GetType() != typeof(LearnSpellFromLabTextActivity))
            {
                return false;
            }
            LearnSpellFromLabTextActivity invent = (LearnSpellFromLabTextActivity)action;
            // TODO: fix this later
            return invent.LabText.SpellContained == LabText.SpellContained;
        }

        public override string Log()
        {
            return $"Learning {LabText.SpellContained.Name} from lab text worth {Desire.ToString("0.000")}";
        }
    }
}
