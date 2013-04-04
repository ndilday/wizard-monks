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
        public static SortedList<PreferenceType, Preference> CreatePreferenceList(bool isMagus)
        {
            SortedList<PreferenceType, Preference> list = new SortedList<PreferenceType, Preference>();
            if (isMagus)
            {
                list.Add(PreferenceType.AgeToApprentice, 
                         new Preference(PreferenceType.AgeToApprentice, null, Die.Instance.RollDouble() * 100));
                foreach (Ability art in MagicArts.GetEnumerator())
                {
                    list.Add(PreferenceType.Art, new Preference(PreferenceType.Art, art, Die.Instance.RollDouble()));
                }
            }

            return list;
        }
    }
}
