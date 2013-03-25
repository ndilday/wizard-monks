using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldSimulation
{
    static class NormalStatRoller
    {
        private static double Z_MAX = 5;
        private static Random rand = new Random();
        public static float StatFromProbability(double probability)
        {
            // for now, basic table
            if (probability < 0.00032)
                return -5;
            if (probability < 0.00192)
                return -4;
            if (probability < 0.00992)
                return -3;
            if(probability < 0.04992)
                return -2;
            if (probability < 0.24992)
                return -1;
            if (probability < 0.75)
                return 0;
            if (probability < 0.95)
                return 1;
            if (probability < 0.99)
                return 2;
            if (probability < 0.998)
                return 3;
            if (probability < 0.9996)
                return 4;
            return 5;
        }

        /*  ProbabilityForGivenZ  --  probability of normal z value

        Adapted from a polynomial approximation in:
                Ibbetson D, Algorithm 209
                Collected Algorithms of the CACM 1963 p. 616
        Note:
                This routine has six digit accuracy, so it is only useful for absolute
                z values <= 6.  For z values > to 6.0, poz() returns 0.0.
        */

        private static double ProbabilityForGivenZ(double z) 
        {
            double y, x, w;
        
            if (z == 0.0) {
                x = 0.0;
            } else {
                y = 0.5 * Math.Abs(z);
                if (y > (Z_MAX * 0.5)) {
                    x = 1.0;
                } else if (y < 1.0) {
                    w = y * y;
                    x = ((((((((0.000124818987 * w
                             - 0.001075204047) * w + 0.005198775019) * w
                             - 0.019198292004) * w + 0.059054035642) * w
                             - 0.151968751364) * w + 0.319152932694) * w
                             - 0.531923007300) * w + 0.797884560593) * y * 2.0F;
                } else {
                    y -= 2.0F;
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


        /*  ZForProbability  --  Compute critical normal z value to
                       produce given p.  We just do a bisection
                       search for a value within CHI_EPSILON,
                       relying on the monotonicity of pochisq().  */

        private static double ZForProbability(double probability) {
            double Z_EPSILON = 0.000001;     /* Accuracy of z approximation */
            double minz = -Z_MAX;
            double maxz = Z_MAX;
            double zval = 0.0;
            double pval;
        
            if (probability < 0.0 || probability > 1.0) {
                return -1;
            }
        
            while ((maxz - minz) > Z_EPSILON) {
                pval = ProbabilityForGivenZ(zval);
                if (pval > probability) {
                    maxz = zval;
                } else {
                    minz = zval;
                }
                zval = (maxz + minz) * 0.5;
            }
            return(zval);
        }

        public static double RandomStat()
        {
            return ZForProbability(rand.NextDouble());
        }
    }
}
