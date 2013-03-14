using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OrderOfHermes.Test
{
    [TestClass]
    public class CharacterTest
    {
        public class FakeDie : IAMDie
        {
            public byte Value { get; set; }

            public ushort RollStressDie(byte botchDiceCount, out byte botchesRolled)
            {
                botchesRolled = 0;
                return Value;
            }

            public ushort RollExplodingDie()
            {
                return Value;
            }

            public ushort RollSimpleDie()
            {
                return Value;
            }
        }

        private static FakeDie _fakeDie;

        [ClassInitialize]
        public static void TestSetUp(TestContext context)
        {
            _fakeDie = new FakeDie();
            Die.Instance = _fakeDie;
        }

        [ClassCleanup]
        public static void TestTearDown()
        {
            Die.Instance = null;
        }
        
        [TestMethod]
        public void TestEarlyLifeAging()
        {
            _fakeDie.Value = 5;

            Character character = new Character();
            ISeason season = new SundrySeason();
            
            for (uint i = 21; i < 141; i++)
            {
                character.AddSeason(season);
                Assert.AreEqual(i, character.SeasonalAge);
                Assert.AreEqual(i, character.ApparentAge);
            }
        }

        public Character CreateAge35Character()
        {
            Character character = new Character();
            ISeason season = new SundrySeason();

            for (uint i = 21; i < 141; i++)
            {
                character.AddSeason(season);
                Assert.AreEqual(i, character.SeasonalAge);
                Assert.AreEqual(i, character.ApparentAge);
            }

            return character;
        }
    }
}
