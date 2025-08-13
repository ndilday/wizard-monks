using System.Collections.Generic;
using System.Linq;
using WizardMonks.Instances;
using WizardMonks.Models.Characters;
using WizardMonks.Models.Spells;

namespace WizardMonks.Economy
{

    // in the future, we should probably have ad hoc micro-economies, 
    // and the possibility of someone profiting by trading among unconnected micro-economies
    public static class GlobalEconomy
    {
        private static Dictionary<Ability, List<BookDesire>> _desiredBooksByTopic;
        // needs to know about all books available for trade
        public static Dictionary<Ability, List<BookForTrade>> BooksForTradeByTopicMap = [];
        public static Dictionary<SpellBase, List<LabTextDesire>> LabTextDesiresBySpellBase = [];
        // needs to know about all topics people have expressed wanting a book for
        public static Dictionary<Ability, List<BookDesire>> DesiredBooksByTopic
        {
            get
            {
                return _desiredBooksByTopic;
            }
            set
            {
                _desiredBooksByTopic = value;
                MostDesiredBookTopics = _desiredBooksByTopic.OrderByDescending(kvp => kvp.Value.Sum(bd => bd.Desire)).Select(kvp => kvp.Key);
            }
        }
        public static IEnumerable<Ability> MostDesiredBookTopics { get; private set; }
        // needs to know about all vis desires
        public static double[] GlobalVisDemandMap = new double[MagicArts.Count];
        // needs to have some sense of the average value of a tractatus
        public static double GlobalTractatusValue = 2;

        public static IEnumerable<Ability> DesiredBookTopics
        {
            get
            {
                return DesiredBooksByTopic.Keys;
            }
        }
    }
}
