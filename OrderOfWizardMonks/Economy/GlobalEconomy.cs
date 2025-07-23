using System.Collections.Generic;
using System.Linq;
using WizardMonks.Instances;

namespace WizardMonks.Economy
{

    // in the future, we should probably have ad hoc micro-economies, 
    // and the possibility of someone profiting by trading among unconnected micro-economies
    public static class GlobalEconomy
    {
        // needs to know about all books available for trade
        public static Dictionary<Ability, List<BookForTrade>> BooksForTradeByTopicMap = new();
        // needs to know about all topics people have expressed wanting a book for
        public static List<BookDesire> DesiredBooksList = new();
        // needs to know about all vis desires
        public static double[] GlobalVisDemandMap = new double[MagicArts.Count];
        // needs to have some sense of the average value of a tractatus
        public static double GlobalTractatusValue = 2;

        public static IEnumerable<Ability> DesiredBookTopics
        {
            get
            {
                return DesiredBooksList.Select(b => b.Ability).Distinct();
            }
        }
    }
}
