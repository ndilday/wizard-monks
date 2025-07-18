
namespace WizardMonks.Activities.MageActivities
{
    public class InventSpellActivity : AExposingMageActivity
    {
        public Spell Spell { get; private set; }

        public InventSpellActivity(Spell spell, Ability exposure, double desire)
            : base(exposure, desire)
        {
            Spell = spell;
            Action = Activity.InventSpells;
        }

        protected override void DoMageAction(Magus mage)
        {
            // TODO: multiple spells
            mage.InventSpell(Spell);
            mage.Log.Add("Inventing " + Spell.Name);
        }

        public override bool Matches(IActivity action)
        {
            if (action.Action != Activity.InventSpells)
            {
                return false;
            }
            InventSpellActivity invent = (InventSpellActivity)action;
            // TODO: fix this later
            return invent.Spell.Base == Spell.Base;
        }

        public override string Log()
        {
            return "Inventing " + Spell.Name + " worth " + Desire.ToString("0.000");
        }
    }

}
