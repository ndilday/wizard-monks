using System;
using System.Collections.Generic;
using System.Linq;

namespace WizardMonks
{
    public enum PreferenceType
    {
        AgeToApprentice,
        Writing,
        Ability,
        Art
    }

    public class Preference
    {
        private PreferenceType _type;
        private object _specifier;

        public Preference(PreferenceType type, object spec, double value)
        {
            _type = type;
            _specifier = spec;
            Value = value;
        }

        public PreferenceType Type
        {
            get
            {
                return _type;
            }
        }
        public object Specifier
        {
            get
            {
                return _specifier;
            }
        }
        public double Value { get; set; }
    }

    public static class PreferenceFactory
    {
        public static SortedSet<Preference> CreatePreferenceList(bool isMagus)
        {
            SortedSet<Preference> list = new SortedSet<Preference>();
            if (isMagus)
            {
                list.Add(new Preference(PreferenceType.AgeToApprentice, null, Die.Instance.RollDouble() * 500));
                double writingDesire = Die.Instance.RollDouble();
                foreach (Ability art in MagicArts.GetEnumerator())
                {
                    double artDesire = Die.Instance.RollDouble();
                    list.Add(new Preference(PreferenceType.Art, art, artDesire));
                    list.Add(new Preference(PreferenceType.Writing, art, (artDesire + writingDesire) / 2));
                }
            }

            return list;
        }
    }
}
