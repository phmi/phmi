using System.Configuration;
using System.Linq;
using NUnit.Framework;
using PHmiClient.Utils.Configuration;

namespace PHmiClientUnitTests.Client.Utils.Configuration
{
    [TestFixture]
    public class StringKeeperTests
    {
        private const string Prefix = "Settings";
        private const string Key = "Key";
        private const string ConfigKey = Prefix + "_" + Key;
        private const string Value = "Value";

        private static System.Configuration.Configuration CreateConfig()
        {
            return ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }

        private static StringKeeper CreateStringKeeper()
        {
            return new StringKeeper(Prefix);
        }

        [SetUp]
        [TearDown]
        public void CleanUp()
        {
            var config = CreateConfig();
            config.AppSettings.Settings.Clear();
            config.Save();
        }

        [Test]
        public void GetReturnsNullWhenConfigEmpty()
        {
            var keeper = CreateStringKeeper();
            Assert.IsNull(keeper.Get(Key));
        }

        [Test]
        public void GetReturnsValueWhenConfigStoresValue()
        {
            var config = CreateConfig();
            config.AppSettings.Settings.Add(ConfigKey, Value);
            config.Save();

            var settings = CreateStringKeeper();
            Assert.AreEqual(Value, settings.Get(Key));
        }

        [Test]
        public void SetAndSaveSavesStringToConfig()
        {
            var keeper = CreateStringKeeper();
            keeper.Set(Key, Value);
            keeper.Save();

            var config = CreateConfig();
            Assert.IsTrue(config.AppSettings.Settings.AllKeys.Count() == 1);
            Assert.AreEqual(Value, config.AppSettings.Settings[ConfigKey].Value);
        }

        [Test]
        public void SetAndSaveReplacesEntryInConfig()
        {
            const string value = "Value";
            const string anotherValue = "Another value";

            var config = CreateConfig();
            config.AppSettings.Settings.Add(ConfigKey, value);
            config.Save();

            var keeper = CreateStringKeeper();
            keeper.Set(Key, anotherValue);
            keeper.Save();

            config = CreateConfig();
            Assert.IsTrue(config.AppSettings.Settings.AllKeys.Count() == 1);
            Assert.AreEqual(anotherValue, config.AppSettings.Settings[ConfigKey].Value);
        }

        [Test]
        public void SetNullAndSaveRemovesEntryFromConfig()
        {
            var config = CreateConfig();
            config.AppSettings.Settings.Add(ConfigKey, Value);
            config.Save();

            var keeper = CreateStringKeeper();
            keeper.Set(Key, null);
            keeper.Save();

            config = CreateConfig();
            Assert.IsFalse(config.AppSettings.Settings.AllKeys.Any());
        }

        [Test]
        public void SetNullAndSaveDoesNothingWhenConfigEmpty()
        {
            var keeper = CreateStringKeeper();
            keeper.Set(Key, null);
            keeper.Save();

            var config = CreateConfig();
            Assert.IsFalse(config.AppSettings.Settings.AllKeys.Any());
        }
    }
}
