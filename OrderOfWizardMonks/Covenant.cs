using System;
using System.Collections.Generic;
using System.Linq;

namespace WizardMonks
{
	[Serializable]
	public class Covenant
	{
		protected List<Magus> _magi;
		protected Dictionary<Ability, byte> _visSources;
        protected Dictionary<Ability, byte> _visStock;
		protected List<IBook> _library;
        public int Aura { get; set; }
        public string Name { get; set; }

        public Covenant()
        {
            _magi = new List<Magus>();
            _visSources = new Dictionary<Ability, byte>();
            _visStock = new Dictionary<Ability, byte>();
            _library = new List<IBook>();
            Aura = 0;
        }

        public void AddMagus(Magus mage)
        {
            if (!_magi.Contains(mage))
            {
                _magi.Add(mage);
            }
        }

        public void AddBook(IBook book)
        {
            _library.Add(book);
        }

        public List<IBook> GetLibrary()
        {
            return _library;
        }

        public IEnumerable<IBook> GetLibrary(Ability ability)
        {
            return _library.Where(b => b.Topic == ability);
        }
	}
}
