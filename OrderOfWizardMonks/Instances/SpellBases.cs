using System;
using System.Collections.Generic;
using System.Linq;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Spells;

namespace WizardMonks.Instances
{
    // TODO: serialize this class
    public static class SpellBases
    {
        private static Dictionary<Ability, Dictionary<Ability, List<SpellBase>>> _spellBasesByArts;
        private static Dictionary<TechniqueEffects, Dictionary<FormEffects, SpellBase>> _spellBasesByEffects;
        private static Dictionary<SpellTag, List<SpellBase>> _spellBasesByTag;

        static SpellBases()
        {
            _spellBasesByArts = new Dictionary<Ability, Dictionary<Ability, List<SpellBase>>>();
            _spellBasesByEffects = new Dictionary<TechniqueEffects, Dictionary<FormEffects, SpellBase>>();
            _spellBasesByTag = new Dictionary<SpellTag, List<SpellBase>>();

            #region CrAn
            Add(new SpellBase(TechniqueEffects.Create, FormEffects.PlainAnimalProduct, SpellArts.Creo | SpellArts.Animal, MagicArtPairs.CrAn, SpellTag.Creation, 5, "Create Animal Product"));
            Add(new SpellBase(TechniqueEffects.Create, FormEffects.TreatedAnimalProduct, SpellArts.Creo | SpellArts.Animal, MagicArtPairs.CrAn, SpellTag.Creation, 10, "Create Treated Animal Product"));
            Add(new SpellBase(TechniqueEffects.Create, FormEffects.ProcessedAnimalProduct, SpellArts.Creo | SpellArts.Animal, MagicArtPairs.CrAn, SpellTag.Creation, 15, "Create Processed Animal Product"));
            Add(new SpellBase(TechniqueEffects.Create, FormEffects.Insect, SpellArts.Creo | SpellArts.Animal, MagicArtPairs.CrAn, SpellTag.Creation, 5, "Create Insect"));
            #endregion

            #region CrVi
            Add(new SpellBase(TechniqueEffects.Create, FormEffects.Aura, SpellArts.Creo | SpellArts.Vim, MagicArtPairs.CrVi, SpellTag.Utility, 3, "Create Magical Aura"));
            Add(new SpellBase(TechniqueEffects.Create, FormEffects.Vis, SpellArts.Creo | SpellArts.Vim, MagicArtPairs.CrVi, SpellTag.Utility, 5, "Conjure Magical Energy"));
            #endregion

            #region InVi
            Add(new SpellBase(TechniqueEffects.Detect, FormEffects.Aura, SpellArts.Intellego | SpellArts.Vim, MagicArtPairs.InVi, SpellTag.Knowledge, 1, "Detect Aura"));
            Add(new SpellBase(TechniqueEffects.Detect, FormEffects.Vis, SpellArts.Intellego | SpellArts.Vim, MagicArtPairs.InVi, SpellTag.Knowledge, 1, "Detect Vis"));
            Add(new SpellBase(TechniqueEffects.Quantify, FormEffects.Vis, SpellArts.Intellego | SpellArts.Vim, MagicArtPairs.InVi, SpellTag.Knowledge, 4, "Quantify Vis"));
            Add(new SpellBase(TechniqueEffects.Detect, FormEffects.Gift, SpellArts.Intellego | SpellArts.Vim, MagicArtPairs.InVi, SpellTag.Knowledge, 6, "Detect Gift"));
            #endregion

            #region ReAn
            Add(new SpellBase(TechniqueEffects.Ward, FormEffects.Animal, SpellArts.Rego | SpellArts.Animal, MagicArtPairs.ReAn, SpellTag.Protection, 2, "Protect the Target from Animal Attacks"));
            #endregion

            #region ReAq
            Add(new SpellBase(TechniqueEffects.Ward, FormEffects.Water, SpellArts.Rego | SpellArts.Aquam, MagicArtPairs.ReAq, SpellTag.Protection, 5, "Ward Against Mundane Water"));
            #endregion

            #region ReAu
            Add(new SpellBase(TechniqueEffects.Ward, FormEffects.SevereWeather, SpellArts.Rego | SpellArts.Auram, MagicArtPairs.ReAu, SpellTag.Protection, 5, "Ward Against Severe Weather"));
            #endregion

            #region ReCo
            Add(new SpellBase(TechniqueEffects.Ward, FormEffects.Body, SpellArts.Rego | SpellArts.Corpus, MagicArtPairs.ReCo, SpellTag.Protection, 15, "Ward Against Human Beings"));
            #endregion

            #region ReHe
            Add(new SpellBase(TechniqueEffects.Ward, FormEffects.Wood, SpellArts.Rego | SpellArts.Herbam, MagicArtPairs.ReHe, SpellTag.Protection, 4, "Deflect a Single Attack by a Wooden Weapon"));
            Add(new SpellBase(TechniqueEffects.Ward, FormEffects.Plant, SpellArts.Rego | SpellArts.Herbam, MagicArtPairs.ReHe, SpellTag.Protection, 15, "Ward Against Mundane Plant Products"));
            #endregion

            #region ReIg
            Add(new SpellBase(TechniqueEffects.Ward, FormEffects.Fire, SpellArts.Rego | SpellArts.Ignem, MagicArtPairs.ReIg, SpellTag.Protection, 4, "Stop Mundane Fire from Burning a Person"));
            #endregion

            #region ReIm
            Add(new SpellBase(TechniqueEffects.Ward, FormEffects.Image, SpellArts.Rego | SpellArts.Imaginem, MagicArtPairs.ReIm, SpellTag.Protection, 3, "Ward Against Images"));
            #endregion

            #region ReMe
            Add(new SpellBase(TechniqueEffects.Ward, FormEffects.Emotion, SpellArts.Rego | SpellArts.Mentem, MagicArtPairs.ReMe, SpellTag.Protection, 3, "Ward Against Minds"));
            #endregion

            #region ReTe
            Add(new SpellBase(TechniqueEffects.Ward, FormEffects.Dirt,  SpellArts.Rego | SpellArts.Terram, MagicArtPairs.ReTe, SpellTag.Protection,  5, "Ward Against Dirt"));
            Add(new SpellBase(TechniqueEffects.Ward, FormEffects.Stone, SpellArts.Rego | SpellArts.Terram, MagicArtPairs.ReTe, SpellTag.Protection, 10, "Ward Against Stone"));
            Add(new SpellBase(TechniqueEffects.Ward, FormEffects.Metal, SpellArts.Rego | SpellArts.Terram, MagicArtPairs.ReTe, SpellTag.Protection, 15, "Ward Against Metal"));
            #endregion

            #region ReVi
            Add(new SpellBase(TechniqueEffects.Ward, FormEffects.Aura, SpellArts.Rego | SpellArts.Vim, MagicArtPairs.ReVi, SpellTag.Defensive | SpellTag.Protection, 3, "Ward Against Magic"));
            Add(new SpellBase(TechniqueEffects.Manipulate, FormEffects.Vis, SpellArts.Rego | SpellArts.Vim, MagicArtPairs.ReVi, SpellTag.Utility, 3, "Manipulate Magical Essence"));
            Add(new SpellBase(TechniqueEffects.Control, FormEffects.Aura, SpellArts.Rego | SpellArts.Vim, MagicArtPairs.ReVi, SpellTag.Utility, 4, "Control Magical Aura"));
            #endregion
        }

        static void Add(SpellBase spellBase)
        {
            AddByArt(spellBase);
            AddByEffect(spellBase);
            AddByTag(spellBase);
        }

        static void AddByTag(SpellBase spellBase)
        {
            foreach (SpellTag tag in Enum.GetValues(typeof(SpellTag)))
            {
                if (tag == SpellTag.None) continue;
                if ((spellBase.Tags & tag) != 0)
                {
                    if (!_spellBasesByTag.ContainsKey(tag))
                        _spellBasesByTag[tag] = new List<SpellBase>();
                    _spellBasesByTag[tag].Add(spellBase);
                }
            }
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

        public static IOrderedEnumerable<SpellBase> GetSpellBasesByArtPair(ArtPair pair)
        {
            if (!_spellBasesByArts.ContainsKey(pair.Technique) || !_spellBasesByArts[pair.Technique].ContainsKey(pair.Form))
            {
                return null;
            }
            return _spellBasesByArts[pair.Technique][pair.Form].OrderBy(s => s.Magnitude);
        }
    
        public static IEnumerable<SpellBase> GetSpellBasesByTag(SpellTag tag)
        {
            if (!_spellBasesByTag.TryGetValue(tag, out var list))
                return Enumerable.Empty<SpellBase>();
            return list;
        }

        public static SpellBase GetSpellBaseForEffect(TechniqueEffects technique, FormEffects form)
        {
            if(!_spellBasesByEffects.ContainsKey(technique) || !_spellBasesByEffects[technique].ContainsKey(form))
            {
                return null;
            }
            return _spellBasesByEffects[technique][form];
        }
    }
}
