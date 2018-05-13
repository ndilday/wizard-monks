using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks.Instances
{
    public static class World
    {
        public static Area StonehengeTribunal;
        public static Area NormandyTribunal;
        public static Area LochLegeanTribunal;
        public static Area RhineTribunal;
        public static Area RomanTribunal;
        public static Area TransylvanianTribunal;
        public static Area ThebianTribunal;
        public static Area ProvencalTribunal;
        public static Area IberianTribunal;
        public static Area AlpineTribunal;
        public static Area HibernianTribunal;
        public static Area NovgorodTribunal;
        public static Area LevantineTribunal;
        public static Area EverywhereElse;

        static World()
        {
            StonehengeTribunal = new Area("Stonehenge Tribunal", Abilities.StonehengeLore);
            NormandyTribunal = new Area("Normandy Tribunal", Abilities.NormandyLore);
            LochLegeanTribunal = new Area("Loch Legean Tribunal", Abilities.LochLegeanLore);
            RhineTribunal = new Area("Rhine Tribunal", Abilities.RhineLore);
            RomanTribunal = new Area("Roman Tribunal", Abilities.RomeLore);
            TransylvanianTribunal = new Area("Transylvanian Tribunal", Abilities.TransylvanianLore);
            ThebianTribunal = new Area("Thebian Tribunal", Abilities.ThebianLore);
            ProvencalTribunal = new Area("Provencal Tribunal", Abilities.ProvencalLore);
            IberianTribunal = new Area("Iberian Tribunal", Abilities.IberianLore);
            AlpineTribunal = new Area("Alpine Tribunal", Abilities.AlpineLore);
            HibernianTribunal = new Area("Hibernian Tribunal", Abilities.HibernianLore);
            NovgorodTribunal = new Area("Novgorod Tribunal", Abilities.NovgorodLore);
            LevantineTribunal = new Area("Levantine Tribunal", Abilities.LevantineLore);
            EverywhereElse = new Area("", null);
        }

        public static IEnumerable<Area> GetEnumerator()
        {
            yield return HibernianTribunal;
            yield return LochLegeanTribunal;
            yield return StonehengeTribunal;
            yield return NormandyTribunal;
            yield return ProvencalTribunal;
            yield return IberianTribunal;
            yield return RhineTribunal;
            yield return AlpineTribunal;
            yield return RomanTribunal;
            yield return NovgorodTribunal;
            yield return TransylvanianTribunal;
            yield return ThebianTribunal;
            yield return LevantineTribunal;
            yield return EverywhereElse;
        }
    }
}
