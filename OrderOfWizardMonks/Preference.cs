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

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(Preference))
            {
                return false;
            }
            Preference pref = (Preference)obj;
            return (pref.Type == this.Type && 
                ((pref.Specifier == null && this.Specifier == null) ||
                    (pref.Specifier == this.Specifier)));
        }

        public override int GetHashCode()
        {
            return _specifier == null ? _type.GetHashCode() : _specifier.GetHashCode() + _type.GetHashCode();
        }
    }
}
