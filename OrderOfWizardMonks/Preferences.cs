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
        Art,
        Vis
    }

    public class Preference
    {
        private PreferenceType _type;
        private object _specifier;

        public Preference(PreferenceType type, object spec)
        {
            _type = type;
            _specifier = spec;
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
    }
}
