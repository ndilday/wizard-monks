using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardMonks.Organizations
{
    public class Affiliation
    {
        public Organization Organization { get; private set; }

        // ranges from -1 (agent attempting to bring the organization down from the inside)
        // to 1 (a zealot who would do anything for the organization)
        public double Loyalty { get; private set; }
        public Affiliation(Organization organization, double loyalty)
        {
            Organization = organization;
            Loyalty = loyalty;
        }
    }
}
