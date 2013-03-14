
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrderOfHermes
{
	[Serializable]
	public class IBook
	{
		public Character Author { get; set; }
		public Ability Topic { get; set; }
		public byte Quality { get; set; }
		public byte Level { get; set; }
	}

	[Serializable]
	public class Summa : IBook
	{
	}

    [Serializable]
    public class Tractatus : IBook
    {
        
    }
}
