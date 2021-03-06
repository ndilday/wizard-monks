﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks.Instances
{
    // TODO: serialize this class
    static class SpellBases
    {
        private static Dictionary<Ability, Dictionary<Ability, List<SpellBase>>> _spellBasesByArts;
        private static Dictionary<TechniqueEffects, Dictionary<FormEffects, SpellBase>> _spellBasesByEffects;

        static SpellBases()
        {
            _spellBasesByArts = new Dictionary<Ability, Dictionary<Ability, List<SpellBase>>>();
            _spellBasesByEffects = new Dictionary<TechniqueEffects, Dictionary<FormEffects, SpellBase>>();

            #region CrAn
            Add(new SpellBase(TechniqueEffects.Create, FormEffects.PlainAnimalProduct, SpellArts.Creo | SpellArts.Animal, MagicArtPairs.CrAn, 5));
            Add(new SpellBase(TechniqueEffects.Create, FormEffects.TreatedAnimalProduct, SpellArts.Creo | SpellArts.Animal, MagicArtPairs.CrAn, 10));
            Add(new SpellBase(TechniqueEffects.Create, FormEffects.ProcessedAnimalProduct, SpellArts.Creo | SpellArts.Animal, MagicArtPairs.CrAn, 15));
            Add(new SpellBase(TechniqueEffects.Create, FormEffects.Insect, SpellArts.Creo | SpellArts.Animal, MagicArtPairs.CrAn, 5));
            #endregion

        }

        static void Add(SpellBase spellBase)
        {
            AddByArt(spellBase);
            AddByEffect(spellBase);
        }

        static void AddByArt(SpellBase spellBase)
        {
            if (!_spellBasesByArts.ContainsKey(spellBase.ArtPair.Technique))
            {
                _spellBasesByArts[spellBase.ArtPair.Technique] = new Dictionary<Ability, List<SpellBase>>();
            }
            if (!_spellBasesByArts[spellBase.ArtPair.Technique].ContainsKey(spellBase.ArtPair.Form))
            {
                _spellBasesByArts[spellBase.ArtPair.Technique][spellBase.ArtPair.Form] = new List<SpellBase>();
            }
            _spellBasesByArts[spellBase.ArtPair.Technique][spellBase.ArtPair.Form].Add(spellBase);
        }

        static void AddByEffect(SpellBase spellBase)
        {
            if (!_spellBasesByEffects.ContainsKey(spellBase.TechniqueEffects))
            {
                _spellBasesByEffects[spellBase.TechniqueEffects] = new Dictionary<FormEffects, SpellBase>();
            }
            if (!_spellBasesByEffects[spellBase.TechniqueEffects].ContainsKey(spellBase.FormEffects))
            {
                _spellBasesByEffects[spellBase.TechniqueEffects][spellBase.FormEffects] = spellBase;
            }
            else
            {
                throw new ArgumentException("Two spell bases with identical effects are not allowed");
            }
        }

        static IOrderedEnumerable<SpellBase> GetSpellBasesByArtPair(ArtPair pair)
        {
            if (!_spellBasesByArts.ContainsKey(pair.Technique) || !_spellBasesByArts[pair.Technique].ContainsKey(pair.Form))
            {
                return null;
            }
            return _spellBasesByArts[pair.Technique][pair.Form].OrderBy(s => s.Level);
        }
    
        static SpellBase GetSpellBaseForEffect(TechniqueEffects technique, FormEffects form)
        {
            if(!_spellBasesByEffects.ContainsKey(technique) || !_spellBasesByEffects[technique].ContainsKey(form))
            {
                return null;
            }
            return _spellBasesByEffects[technique][form];
        }
    }
}
