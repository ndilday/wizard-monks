using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks.Instances
{
    public static class SpellModifiers
    {
        public static int GetModifier(Range range)
        {
            switch (range)
            {
                case Range.Personal:
                    return 0;
                case Range.Touch:
                case Range.Eye:
                    return 1;
                case Range.Voice:
                    return 2;
                case Range.Sight:
                    return 3;
                case Range.Arcane:
                    return 4;
                default:
                    return 0;
            }
        }

        public static int GetModifier(Duration duration)
        {
            switch (duration)
            {
                case Duration.Instantaneous:
                    return 0;
                case Duration.Concentration:
                case Duration.Diameter:
                    return 1;
                case Duration.Sun:
                case Duration.Circle:
                    return 2;
                case Duration.Moon:
                    return 3;
                case Duration.Year:
                    return 4;
                default:
                    return 0;

            }
        }

        public static int GetModifier(Target target)
        {
            switch (target)
            {
                case Target.Individual:
                    return 0;
                case Target.Part:
                    return 1;
                case Target.Group:
                case Target.Ring:
                    return 2;
                case Target.Room:
                    return 3;
                case Target.Structure:
                    return 4;
                case Target.Boundary:
                    return 5;
                default:
                    return 0;
            }
        }
    }
}
