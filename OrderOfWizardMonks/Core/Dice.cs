using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks.Core
{
    public interface IAMDie
    {
        ushort RollStressDie(byte botchDiceCount, out byte botchesRolled);
        ushort RollExplodingDie();
        ushort RollSimpleDie();
        double RollDouble();
        double RollNormal(double mean, double stdDev);
        double RollNormal();
    }

    public class Die : IAMDie
    {
        private Random _random = new Random();
        private static volatile IAMDie _singletonDie;
        private static readonly object _lockerObject;
        private static readonly int Z_MAX = 5;

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

        public double RollDouble()
        {
            return _random.NextDouble();
        }

        public double RollNormal(double mean, double stdDev)
        {
            double val = RollNormal();
            return mean + (stdDev * val);
        }


        public double RollNormal()
        {
            double p = RollDouble();
            double Z_EPSILON = 0.000001;     /* Accuracy of z approximation */
            double minz = -Z_MAX;
            double maxz = Z_MAX;
            double zval = 0.0;
            double pval;
        
            if (p < 0.0 || p > 1.0) {
                return -1;
            }
        
            while ((maxz - minz) > Z_EPSILON) {
                pval = poz(zval);
                if (pval > p) {
                    maxz = zval;
                } else {
                    minz = zval;
                }
                zval = (maxz + minz) * 0.5;
            }
            return(zval);
        }

        private double poz(double z) 
        {
            double y, x, w;
        
            if (z == 0.0) 
            {
                x = 0.0;
            } 
            else 
            {
                y = 0.5 * Math.Abs(z);
                if (y > (Z_MAX * 0.5)) 
                {
                    x = 1.0;
                } 
                else if (y < 1.0) 
                {
                    w = y * y;
                    x = ((((((((0.000124818987 * w
                             - 0.001075204047) * w + 0.005198775019) * w
                             - 0.019198292004) * w + 0.059054035642) * w
                             - 0.151968751364) * w + 0.319152932694) * w
                             - 0.531923007300) * w + 0.797884560593) * y * 2.0;
                } 
                else 
                {
                    y -= 2.0;
                    x = (((((((((((((-0.000045255659 * y
                                   + 0.000152529290) * y - 0.000019538132) * y
                                   - 0.000676904986) * y + 0.001390604284) * y
                                   - 0.000794620820) * y - 0.002034254874) * y
                                   + 0.006549791214) * y - 0.010557625006) * y
                                   + 0.011630447319) * y - 0.009279453341) * y
                                   + 0.005353579108) * y - 0.002141268741) * y
                                   + 0.000535310849) * y + 0.999936657524;
                }
            }
            return z > 0.0 ? ((x + 1.0) * 0.5) : ((1.0 - x) * 0.5);
        }
    }
}
