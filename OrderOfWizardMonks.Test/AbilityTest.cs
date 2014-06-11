using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using WizardMonks;

namespace WizardMonks.Test
{
    [TestClass]
    public sealed class AbilityTest
    {
        [TestMethod]
        public void BasicAbilityTest()
        {
            Ability ability = new Ability(0, AbilityType.General, "TestAbility");
            Assert.AreEqual("TestAbility", ability.AbilityName);
            Assert.AreEqual(AbilityType.General, ability.AbilityType);
        }

        [TestMethod]
        public void CharacterAbilityZeroTest()
        {
            Ability ability = new Ability(0, AbilityType.General, "TestAbility");
            CharacterAbility characterAbility = new CharacterAbility(ability);
            characterAbility.Experience = 0;

            Assert.AreEqual(0, characterAbility.Value);
        }

        [TestMethod]
        public void CharacterAbilityExactTest()
        {
            Ability ability = new Ability(0, AbilityType.General, "TestAbility");
            CharacterAbility characterAbility = new CharacterAbility(ability);
            characterAbility.Experience = 15;

            Assert.AreEqual(2, characterAbility.Value);
        }

        [TestMethod]
        public void CharacterAbilityInexactTest()
        {
            Ability ability = new Ability(0, AbilityType.General, "TestAbility");
            CharacterAbility characterAbility = new CharacterAbility(ability);
            characterAbility.Experience = 37;

            Assert.AreEqual(3, characterAbility.Value);
        }

        [TestMethod]
        public void DeserializationTest()
        {
            Assembly thisExe = Assembly.GetExecutingAssembly();
            XmlSerializer serializer = new XmlSerializer(typeof(Ability));
            Stream reader = thisExe.GetManifestResourceStream("OrderOfHermes.Test.TestFiles.skills.xml");
            Assert.IsNotNull(reader);
            var abilities = (List<Ability>) serializer.Deserialize(reader);
            Assert.IsNotNull(abilities);
            Assert.AreNotEqual(0, abilities.Count);
        }

        [TestMethod]
        public void LimitedGainTest()
        {
            Ability ability = new Ability(0, AbilityType.General, "TestAbility");
            CharacterAbility charAbility = new CharacterAbility(ability);
            charAbility.AddExperience(10, 1);
            Assert.AreEqual(1, charAbility.Value);
        }
    }
}
