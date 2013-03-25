using Microsoft.VisualStudio.TestTools.UnitTesting;
using WizardMonks;

namespace WizardMonks.Test
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
    }
}
