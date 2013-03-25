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
		protected Dictionary<AcceleratedAbility, byte> visSources;
        protected Dictionary<AcceleratedAbility, byte> visStocks;
		protected List<Tractatus> library;
        public byte Aura { get; set; }

        public Covenant()
        {
            Magi = new List<Magus>();
            visSources = new Dictionary<AcceleratedAbility, byte>();
            visStocks = new Dictionary<AcceleratedAbility, byte>();
            library = new List<Tractatus>();
        }
	}
}
