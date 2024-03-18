using System.Collections.Generic;
using WizardMonks.Characters;

namespace WizardMonks.Organizations
{
    public class Organization
    {
        public string Name { get; private set; }
        public ushort SeasonFounded { get; private set; }
        public Organization ParentOrganization { get; private set; }
        public List<Organization> ChildOrganizations { get; private set; }
        public List<Character> Members { get; private set; }

        public Organization() 
        {
            ChildOrganizations = [];
        }

        public Organization(string name, ushort seasonFounded, Organization parentOrganization) : this()
        {
            Name = name;
            SeasonFounded = seasonFounded;
            ParentOrganization = parentOrganization;
            Members = [];
        }

        public void AddMember(Character newMember)
        {
            Members.Add(newMember);
        }

        public void RemoveMember(Character newMember) 
        {  
            Members.Remove(newMember); 
        }
    }
}
