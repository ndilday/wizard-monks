using System.Collections.Generic;
using System.Linq;

namespace WizardMonks
{
    public class Area
    {
        public List<Character> Denizens { get; private set; }
        public List<Area> SubAreas { get; private set; }
        public Area ParentArea { get; private set; }
        public List<Covenant> Covenants { get; private set; }
        public Ability AreaLore { get; private set; }
        public string Name { get; set; }

        public Area(string name, Ability areaLore, Area parentArea = null)
        {
            Denizens = new List<Character>();
            SubAreas = new List<Area>();
            Covenants = new List<Covenant>();
            ParentArea = parentArea;
            Name = name;
            AreaLore = areaLore;
        }
    }
}
