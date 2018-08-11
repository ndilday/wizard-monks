using System;
using System.Collections.Generic;
using System.Linq;

using WizardMonks.Instances;

namespace WizardMonks
{
	[Serializable]
	public class Covenant : Area
	{
		protected List<Magus> _magi;
		protected Dictionary<Ability, double> _visSources;
        protected Dictionary<Ability, double> _visStock;
		protected List<IBook> _library;
        public Aura Aura { get; private set; }

        public Covenant(string name, Area location, Aura aura = null) : base(name, location.AreaLore, location) 
        {
            _magi = new List<Magus>();
            _visSources = new Dictionary<Ability, double>();
            _visStock = new Dictionary<Ability, double>();
            _library = new List<IBook>();
            Aura = aura;
        }

        public void AddMagus(Magus mage)
        {
            if (!_magi.Contains(mage))
            {
                _magi.Add(mage);
            }
        }

        public void RemoveMagus(Magus mage)
        {
            if (_magi.Contains(mage))
            {
                _magi.Remove(mage);
            }
        }

        public void AddBook(IBook book)
        {
            // TODO: handle book duplicates when we handle copying books
            if (!_library.Contains(book))
            {
                _library.Add(book);
            }
        }


        #region Vis Functions
        public double AddVis(Ability visType, double amount)
        {
            if (MagicArts.IsArt(visType))
            {
                if (_visStock.ContainsKey(visType))
                {
                    _visStock[visType] += amount;
                }
                else
                {
                    _visStock[visType] = amount;
                }
                return _visStock[visType];
            }
            return 0;
        }

        public double RemoveVis(Ability visType, double amount)
        {
            if (amount <= 0)
            {
                return _visStock.ContainsKey(visType) ? _visStock[visType] : 0;
            }
            if (!MagicArts.IsArt(visType))
            {
                throw new ArgumentException("Only magic arts have vis!");
            }
            if(!_visStock.ContainsKey(visType) || _visStock[visType] < amount)
            {
                throw new ArgumentException("Insufficient vis available!");
            }
            _visStock[visType] -= amount;
            return _visStock[visType];
        }

        public double GetVis(Ability visType)
        {
            if (!MagicArts.IsArt(visType))
            {
                throw new ArgumentException("Only magic arts have vis!");
            }
            return _visStock.ContainsKey(visType) ? _visStock[visType] : 0;
        }
        #endregion

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
