using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WizardMonks.Models.Spells;

namespace WizardMonks.Instances
{
    public static class EffectRanges
    {
        public static EffectRange Personal;
        public static EffectRange Touch;
        public static EffectRange Eye;
        public static EffectRange Voice;
        public static EffectRange Sight;
        public static EffectRange Arcane;

        static EffectRanges()
        {
            Personal = new EffectRange(Ranges.Personal, 0);
            Touch = new EffectRange(Ranges.Touch, 1);
            Eye = new EffectRange(Ranges.Eye, 1);
            Voice = new EffectRange(Ranges.Voice, 2);
            Sight = new EffectRange(Ranges.Sight, 3);
            Arcane = new EffectRange(Ranges.Arcane, 4);
        }
    }

    public static class EffectDurations
    {
        public static EffectDuration Instant;
        public static EffectDuration Concentration;
        public static EffectDuration Diameter;
        public static EffectDuration Sun;
        public static EffectDuration Ring;
        public static EffectDuration Moon;
        public static EffectDuration Year;

        static EffectDurations()
        {
            Instant = new EffectDuration(Durations.Instantaneous, 0);
            Concentration = new EffectDuration(Durations.Concentration, 1);
            Diameter = new EffectDuration(Durations.Diameter, 1);
            Sun = new EffectDuration(Durations.Sun, 2);
            Ring = new EffectDuration(Durations.Ring, 2);
            Moon = new EffectDuration(Durations.Moon, 3);
            Year = new EffectDuration(Durations.Year, 4, true);
        }
    }

    public static class EffectTargets
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

        static EffectTargets()
        {
            Individual = new EffectTarget(Targets.Individual, 0);
            Taste = new EffectTarget(Targets.Taste, 0);
            Circle = new EffectTarget(Targets.Circle, 0);
            Part = new EffectTarget(Targets.Part, 1);
            Touch = new EffectTarget(Targets.Touch, 1);
            Group = new EffectTarget(Targets.Group, 2);
            Smell = new EffectTarget(Targets.Smell, 2);
            Room = new EffectTarget(Targets.Room, 2);
            Structure = new EffectTarget(Targets.Structure, 3);
            Hearing = new EffectTarget(Targets.Hearing, 3);
            Boundary = new EffectTarget(Targets.Boundary, 4, true);
            Sight = new EffectTarget(Targets.Sight, 4);
        }
    }
}
