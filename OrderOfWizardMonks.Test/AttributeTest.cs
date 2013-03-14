using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrderOfHermes;

namespace OrderOfHermes.Test
{
    [TestClass]
    public class AttributeTest
    {
        [TestMethod]
        public void TestDefaultAttributeBehavior()
        {
            Attribute attribute = new Attribute(2);
            Assert.AreEqual(2, attribute.Value);
        }

        [TestMethod]
        public void TestDecrepitude()
        {
            Attribute attribute = new Attribute(2);
            attribute.AddDecrepitude((byte)(2));
            Assert.AreEqual(2, attribute.Value);
        }

        [TestMethod]
        public void TestDecrepitudeLoss()
        {
            Attribute attribute = new Attribute(2);
            attribute.AddDecrepitude((byte)(3));
            Assert.AreEqual(1, attribute.Value);
        }

        [TestMethod]
        public void TestNegativeDecrepitudeLoss()
        {
            Attribute attribute = new Attribute(-2);
            attribute.AddDecrepitude((byte)(3));
            Assert.AreEqual(-3, attribute.Value);
        }

        [TestMethod]
        public void TestDecrepitudeLevelAdd()
        {
            Attribute attribute = new Attribute(3);
            Assert.AreEqual(4, attribute.AddDecrepitudeToNextLevel());
            Assert.AreEqual(2, attribute.Value);
            Assert.AreEqual(3, attribute.AddDecrepitudeToNextLevel());
            Assert.AreEqual(1, attribute.Value);
        }

        [TestMethod]
        public void TestNegativeDecrepitudeLevelAdd()
        {
            Attribute attribute = new Attribute(-1);
            Assert.AreEqual(2, attribute.AddDecrepitudeToNextLevel());
            Assert.AreEqual(-2, attribute.Value);
            Assert.AreEqual(3, attribute.AddDecrepitudeToNextLevel());
            Assert.AreEqual(-3, attribute.Value);
        }
    }
}
