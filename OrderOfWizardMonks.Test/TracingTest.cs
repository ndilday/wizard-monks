using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OrderOfHermes.Test
{
    [TestClass]
    public class TracingTest
    {
        private Character _character;

        [TestMethod]
        public void TestAging()
        {
            uint runningTally = 0;
            ISeason season = new SundrySeason();
                                
            for (int i = 0; i < 100; i++)
            {
                _character = new Character();

                while (_character.Decrepitude < 75)
                {
                    _character.AddSeason(season);
                }
                Trace.WriteLine("Age at Death: " + _character.SeasonalAge);
                runningTally += _character.SeasonalAge;
            }
            Trace.WriteLine("Average Age at Death: " + runningTally/100);
        }
    }
}
