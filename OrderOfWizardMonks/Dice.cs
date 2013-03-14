using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrderOfHermes
{
    public interface IAMDie
    {
        ushort RollStressDie(byte botchDiceCount, out byte botchesRolled);
        ushort RollExplodingDie();
        ushort RollSimpleDie();
    }

    public class Die : IAMDie
    {
        private Random _random = new Random();
        private static volatile IAMDie _singletonDie;
        private static readonly object _lockerObject;

        static Die()
        {
            _lockerObject = new object();
        }

        public static IAMDie Instance
        {
            get
            {
                if(_singletonDie == null)
                {
                    lock( _lockerObject)
                    {
                        if(_singletonDie == null)
                        {
                            _singletonDie = new Die();
                        }
                    }
                }

                return _singletonDie;
            }
            set
            {
                lock (_lockerObject)
                {
                    _singletonDie = value;
                }
            }
        }

        public ushort RollStressDie(byte botchDiceCount, out byte botchesRolled)
        {
            botchesRolled = 0;
            int roll = _random.Next(0, 9);
            int multiplier = 1;
            while (roll == 1)
            {
                multiplier++;
                roll = _random.Next(1, 10);
            }

            if (roll == 0)
            {
                for (byte i = 0; i < botchDiceCount; i++)
                {
                    if (_random.Next(0, 9) == 0)
                    {
                        botchesRolled++;
                    }
                }
                return 0;
            }
            else
            {
                return Convert.ToUInt16(roll * multiplier);
            }
        }

        public ushort RollExplodingDie()
        {
            int roll = _random.Next(1, 10);
            int multiplier = 1;
            while (roll == 1)
            {
                multiplier++;
                roll = _random.Next(1, 10);
            }

            return Convert.ToUInt16(roll * multiplier);
        }

        public ushort RollSimpleDie()
        {
            return Convert.ToUInt16(_random.Next(1, 10));
        }
    }
}
