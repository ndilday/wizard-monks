using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WizardMonks.Test
{
    [TestClass]
    public class TracingTest
    {
        private Character _character;

        [TestMethod]
        public void TestAging()
        {
            uint runningTally = 0;
            IAction action = new Exposure(new Ability(), 0);
                                
            for (int i = 0; i < 100; i++)
            {
                _character = new Character(null, null, null, null);

                while (_character.Decrepitude < 75)
                {
                    _character.CommitAction(action);
                }
                Trace.WriteLine("Age at Death: " + _character.SeasonalAge);
                runningTally += _character.SeasonalAge;
            }
            Trace.WriteLine("Average Age at Death: " + runningTally/100);
        }
    }
}
