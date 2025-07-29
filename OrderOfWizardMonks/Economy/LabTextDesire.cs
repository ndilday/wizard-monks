namespace WizardMonks.Economy
{
    public class LabTextDesire
    {
        public SpellBase SpellBase { get; private set; }
        public double MinimumLevel { get; private set; }
        public double MaximumLevel { get; private set; }
        public Character Character { get; private set; }
        public LabTextDesire(Character character, SpellBase spellBase, double minimumLevel, double maximumLevel)
        {
            SpellBase = spellBase;
            Character = character;
            MinimumLevel = minimumLevel;
            MaximumLevel = maximumLevel;
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
            return $"{SpellBase.ToString()} {MinimumLevel.ToString("#0.0")}-{MaximumLevel.ToString("#0.0")}";
        }
    }
}
