using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks
{
	[Serializable]
	class Covenant
	{
		protected List<Magus> Magi;
		protected Dictionary<Ability, byte> visSources;
        protected Dictionary<Ability, byte> visStocks;
		protected List<IBook> library;
        public byte Aura { get; set; }

        public Covenant()
        {
            Magi = new List<Magus>();
            visSources = new Dictionary<Ability, byte>();
            visStocks = new Dictionary<Ability, byte>();
            library = new List<IBook>();
            Aura = 0;
        }
	}
}
