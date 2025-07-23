namespace WizardMonks.Economy
{
    public class LabTextDesire
    {
        public SpellBase SpellBase { get; private set; }
        public double CurrentLevel { get; private set; }
        public Character Character { get; private set; }
        public LabTextDesire(Character character, SpellBase spellBase, double curLevel = 0)
        {
            SpellBase = spellBase;
            CurrentLevel = curLevel;
            Character = character;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(LabTextDesire))
            {
                return false;
            }
            LabTextDesire labTextDesire = (LabTextDesire)obj;
            return labTextDesire.SpellBase == SpellBase && labTextDesire.Character == Character;
        }

        public override int GetHashCode()
        {
            return SpellBase.GetHashCode();
        }

        public override string ToString()
        {
            return $"{SpellBase.ToString()} {CurrentLevel.ToString()}";
        }
    }
}
