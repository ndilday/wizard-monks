using System;
using System.Collections.Generic;
using System.Linq;

using WizardMonks.Characters;

namespace WizardMonks.Instances
{
    // TODO: serialize this class
    static class SpellBases
    {
        private readonly static Dictionary<Ability, Dictionary<Ability, List<SpellBase>>> _spellBasesByArts;
        private readonly static Dictionary<TechniqueEffects, Dictionary<FormEffects, SpellBase>> _spellBasesByEffects;

        static SpellBases()
        {
            _spellBasesByArts = [];
            _spellBasesByEffects = [];

            #region CrAn
            Add(new SpellBase(TechniqueEffects.Create, FormEffects.PlainAnimalProduct, SpellArts.Creo | SpellArts.Animal, MagicArtPairs.CrAn, 5, "Create Animal Product"));
            Add(new SpellBase(TechniqueEffects.Create, FormEffects.TreatedAnimalProduct, SpellArts.Creo | SpellArts.Animal, MagicArtPairs.CrAn, 10, "Create Treated Animal Product"));
            Add(new SpellBase(TechniqueEffects.Create, FormEffects.ProcessedAnimalProduct, SpellArts.Creo | SpellArts.Animal, MagicArtPairs.CrAn, 15, "Create Processed Animal Product"));
            Add(new SpellBase(TechniqueEffects.Create, FormEffects.Insect, SpellArts.Creo | SpellArts.Animal, MagicArtPairs.CrAn, 5, "Create Insect"));
            #endregion

            #region InVi
            Add(new SpellBase(TechniqueEffects.Detect, FormEffects.Aura, SpellArts.Intellego | SpellArts.Vim, MagicArtPairs.InVi, 1, "Detect Aura"));
            Add(new SpellBase(TechniqueEffects.Detect, FormEffects.Vis, SpellArts.Intellego | SpellArts.Vim, MagicArtPairs.InVi, 1, "Detect Vis"));
            Add(new SpellBase(TechniqueEffects.Quantify, FormEffects.Vis, SpellArts.Intellego | SpellArts.Vim, MagicArtPairs.InVi, 4, "Quantify Vis"));
            Add(new SpellBase(TechniqueEffects.Detect, FormEffects.Gift, SpellArts.Intellego | SpellArts.Vim, MagicArtPairs.InVi, 10, "Detect Gift"));
            #endregion
        }

        static void Add(SpellBase spellBase)
        {
            AddByArt(spellBase);
            AddByEffect(spellBase);
        }

        static void AddByArt(SpellBase spellBase)
        {
            if (!_spellBasesByArts.TryGetValue(spellBase.ArtPair.Technique, out Dictionary<Ability, List<SpellBase>> spellBaseMap))
            {
                spellBaseMap = [];
                _spellBasesByArts[spellBase.ArtPair.Technique] = spellBaseMap;
            }
            if (!spellBaseMap.TryGetValue(spellBase.ArtPair.Form, out List<SpellBase> spellBaseList))
            {
                spellBaseList = [];
                spellBaseMap[spellBase.ArtPair.Form] = spellBaseList;
            }

            spellBaseList.Add(spellBase);
        }

        static void AddByEffect(SpellBase spellBase)
        {
            if (!_spellBasesByEffects.TryGetValue(spellBase.TechniqueEffects, out Dictionary<FormEffects, SpellBase> value))
            {
                value = ([]);
                _spellBasesByEffects[spellBase.TechniqueEffects] = value;
            }
            if (!value.ContainsKey(spellBase.FormEffects))
            {
                value[spellBase.FormEffects] = spellBase;
            }
            else
            {
                throw new ArgumentException("Two spell bases with identical effects are not allowed");
            }
        }

        static IOrderedEnumerable<SpellBase> GetSpellBasesByArtPair(ArtPair pair)
        {
            if (!_spellBasesByArts.TryGetValue(pair.Technique, out Dictionary<Ability, List<SpellBase>> value) || !value.TryGetValue(pair.Form, out List<SpellBase> spellBaseList))
            {
                return null;
            }
            return spellBaseList.OrderBy(s => s.Level);
        }
    
        public static SpellBase GetSpellBaseForEffect(TechniqueEffects technique, FormEffects form)
        {
            if(!_spellBasesByEffects.TryGetValue(technique, out Dictionary<FormEffects, SpellBase> value) || !value.TryGetValue(form, out SpellBase spellBase))
            {
                return null;
            }
            return spellBase;
        }
    }
}
