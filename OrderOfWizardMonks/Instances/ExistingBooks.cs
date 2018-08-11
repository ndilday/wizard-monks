using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardMonks.Instances
{
    public static class ExistingBooks
    {
        public static Summa ArsGrammatica;

        static ExistingBooks()
        {
            ArsGrammatica = new Summa("Ars Grammatica - Ars Minor", "Donatus", Abilities.ArtesLiberales, 15, 4, Abilities.Latin);
        }
    }
}
