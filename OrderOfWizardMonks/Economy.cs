using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks
{
    public class VisDesire
    {
        public Ability Art { get; private set; }
        public double Quantity { get; set; }
        public VisDesire(Ability art, double quantity = 0)
        {
            Art = art;
            Quantity = quantity;
        }
    }
    public class VisDesires
    {
        VisDesire[] _visDesires;
        public Magus Magus { get; private set; }

        public VisDesires(Magus mage, VisDesire[] desires)
        {
            if (desires.Length != 15)
            {
                throw new ArgumentException("desires of incorrect length");
            }
            Magus = mage;
            _visDesires = desires;
        }

        public double GetVisDesire(Ability art)
        {
            if (art.AbilityId < 300 || art.AbilityId > 314)
            {
                throw new ArgumentException("incorrect ability passed to GetVisDesire");
            }
            return _visDesires[art.AbilityId % 300].Quantity;
        }
    }

    public class BookDesire
    {
        public Ability Ability { get; private set; }
        public double MinimumLevel { get; private set; }
    }

    public class CharacterDesires
    {
        VisDesire[] _visDesires;
        Dictionary<Ability, BookDesire> _bookDesires;
        Character _character;

        public CharacterDesires(Character character, VisDesire[] visDesires, IEnumerable<BookDesire> booksDesired)
        {
            _character = character;
            _visDesires = visDesires;
            _bookDesires = booksDesired.ToDictionary(l => l.Ability);
        }
    }

    public static class Economy
    {
        public static double ExperienceToVis(double gain, double currentLevel)
        {
            return 0;
        }
    }
}
