using WizardMonks.Models.Characters;

namespace WizardMonks.Economy
{
    public class BookDesire
    {
        public Ability Ability { get; private set; }
        public double CurrentLevel { get; private set; }
        public double Desire { get; set; }
        public Character Character { get; private set; }
        public BookDesire(Character character, Ability ability, double desire, double curLevel = 0)
        {
            Ability = ability;
            CurrentLevel = curLevel;
            Character = character;
            Desire = desire;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(BookDesire))
            {
                return false;
            }
            BookDesire bookDesire = (BookDesire)obj;
            return bookDesire.Ability == Ability && bookDesire.Character == Character;
        }

        public override int GetHashCode()
        {
            return Ability.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Ability.ToString()} {CurrentLevel.ToString()}";
        }
    }
}
