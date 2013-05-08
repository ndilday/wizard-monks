using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks.Instances
{
    public static class Preferences
    {
        public static Preference VisDesire { get; private set; }

        static Preferences()
        {
            VisDesire = new Preference(PreferenceType.Vis, null);
        }
    }
}
