using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Instances;
using WizardMonks.Models.Beliefs;
using WizardMonks.Models.Books;
using WizardMonks.Models.Characters;

namespace WizardMonks.Models.Covenants
{
    public enum CovenantRole
    {
        Founder,
        FullMember,
        Apprentice,
        Visitor // For magi staying for an extended period, like the hedge wizards
    }

    [Serializable]
	public class Covenant : IBeliefSubject
	{
        protected Dictionary<Magus, CovenantRole> _inhabitants;
        protected Dictionary<Ability, double> _visSources;
        protected Dictionary<Ability, double> _visStock;
		protected List<ABook> _library;
        public Aura Aura { get; private set; }
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Name { get; set; }

        public Covenant()
        {
            _inhabitants = [];
            _visSources = [];
            _visStock = [];
            _library = [];
            Aura = null;
        }

        public Covenant(Aura aura) : this()
        {
            Aura = aura;
        }

        public void AddMagus(Magus mage, CovenantRole role = CovenantRole.FullMember)
        {
            if (!_inhabitants.ContainsKey(mage))
            {
                _inhabitants.Add(mage, role);
            }
            else
            {
                // If they are already here, maybe just update their role
                _inhabitants[mage] = role;
            }
        }

        public void RemoveMagus(Magus mage)
        {
            if (_inhabitants.ContainsKey(mage))
            {
                _inhabitants.Remove(mage);
            }
        }

        public IEnumerable<Magus> GetMagiByRole(CovenantRole role)
        {
            return _inhabitants.Where(kvp => kvp.Value == role).Select(kvp => kvp.Key);
        }

        public CovenantRole? GetRoleForMagus(Magus mage)
        {
            if (_inhabitants.TryGetValue(mage, out CovenantRole role))
            {
                return role;
            }
            return null;
        }

        public void AddBook(ABook book)
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

        public List<ABook> GetLibrary()
        {
            return _library;
        }

        public IEnumerable<ABook> GetLibrary(Ability ability)
        {
            return _library.Where(b => b.Topic == ability);
        }
	}
}
