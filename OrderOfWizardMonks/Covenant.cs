﻿using System;
using System.Collections.Generic;
using System.Linq;

using WizardMonks.Instances;

namespace WizardMonks
{
	[Serializable]
	public class Covenant
	{
		protected List<Magus> _magi;
		protected Dictionary<Ability, double> _visSources;
        protected Dictionary<Ability, double> _visStock;
		protected List<IBook> _library;
        public int Aura { get; set; }
        public string Name { get; set; }

        public Covenant()
        {
            _magi = new List<Magus>();
            _visSources = new Dictionary<Ability, double>();
            _visStock = new Dictionary<Ability, double>();
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


        #region Vis Functions
        public double AddVis(Ability visType, double amount)
        {
            if (MagicArts.IsArt(visType))
            {
                _visStock[visType] += amount;
                return _visStock[visType];
            }
            return 0;
        }

        public double RemoveVis(Ability visType, double amount)
        {
            if (!MagicArts.IsArt(visType))
            {
                throw new ArgumentException("Only magic arts have vis!");
            }
            if (_visStock[visType] < amount)
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
            return _visStock[visType];
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
