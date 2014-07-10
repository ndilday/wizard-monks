using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks.Instances
{
    public static class Ranges
    {
        public static EffectRange Personal;
        public static EffectRange Touch;
        public static EffectRange Eye;
        public static EffectRange Voice;
        public static EffectRange Sight;
        public static EffectRange Arcane;

        static Ranges()
        {
            Personal = new EffectRange(WizardMonks.Ranges.Personal, 0);
            Touch = new EffectRange(WizardMonks.Ranges.Touch, 1);
            Eye = new EffectRange(WizardMonks.Ranges.Eye, 1);
            Voice = new EffectRange(WizardMonks.Ranges.Voice, 2);
            Sight = new EffectRange(WizardMonks.Ranges.Sight, 3);
            Arcane = new EffectRange(WizardMonks.Ranges.Arcane, 4);
        }
    }

    public static class Durations
    {
        public static EffectDuration Instant;
        public static EffectDuration Concentration;
        public static EffectDuration Diameter;
        public static EffectDuration Sun;
        public static EffectDuration Ring;
        public static EffectDuration Moon;
        public static EffectDuration Year;

        static Durations()
        {
            Instant = new EffectDuration(WizardMonks.Durations.Instantaneous, 0);
            Concentration = new EffectDuration(WizardMonks.Durations.Concentration, 1);
            Diameter = new EffectDuration(WizardMonks.Durations.Diameter, 1);
            Sun = new EffectDuration(WizardMonks.Durations.Sun, 2);
            Ring = new EffectDuration(WizardMonks.Durations.Ring, 2);
            Moon = new EffectDuration(WizardMonks.Durations.Moon, 3);
            Year = new EffectDuration(WizardMonks.Durations.Year, 4, true);
        }
    }

    public static class Targets
    {
        public static EffectTarget Individual;
        public static EffectTarget Taste;
        public static EffectTarget Circle;
        public static EffectTarget Part;
        public static EffectTarget Touch;
        public static EffectTarget Group;
        public static EffectTarget Smell;
        public static EffectTarget Room;
        public static EffectTarget Structure;
        public static EffectTarget Hearing;
        public static EffectTarget Boundary;
        public static EffectTarget Sight;

        static Targets()
        {
            Individual = new EffectTarget(WizardMonks.Targets.Individual, 0);
            Taste = new EffectTarget(WizardMonks.Targets.Taste, 0);
            Circle = new EffectTarget(WizardMonks.Targets.Circle, 0);
            Part = new EffectTarget(WizardMonks.Targets.Part, 1);
            Touch = new EffectTarget(WizardMonks.Targets.Touch, 1);
            Group = new EffectTarget(WizardMonks.Targets.Group, 2);
            Smell = new EffectTarget(WizardMonks.Targets.Smell, 2);
            Room = new EffectTarget(WizardMonks.Targets.Room, 2);
            Structure = new EffectTarget(WizardMonks.Targets.Structure, 3);
            Hearing = new EffectTarget(WizardMonks.Targets.Hearing, 3);
            Boundary = new EffectTarget(WizardMonks.Targets.Boundary, 4, true);
            Sight = new EffectTarget(WizardMonks.Targets.Sight, 4);
        }
    }
}
