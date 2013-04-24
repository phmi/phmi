using NUnit.Framework;
using PHmiIoDevice.Melsec.Configuration;

namespace PHmiUnitTests.IoDevices.Mitsubishi.Melsec.Configuration
{
    [TestFixture]
    public class ConfigTests
    {
        [Test]
        public void GetXmlTest()
        {
            var config = new Config("Name")
                {
                    Timeout = 2000,
                    MessageEndTimeout = 100
                };
            var xml = config.GetXml();
            Assert.That(
                xml.Replace("\r\n", string.Empty).Replace("    ", string.Empty).Replace("   ", string.Empty).Replace("  ", string.Empty),
                Is.EqualTo("<?xml version=\"1.0\" encoding=\"utf-16\"?><Name><Timeout>2000</Timeout><MessageEndTimeout>100</MessageEndTimeout></Name>"));
        }

        [Test]
        public void GetSetXmlEqualityTest()
        {
            var config = new Config("Name") {MessageEndTimeout = 130, Timeout = 2121};
            var config2 = new Config("Name");
            config2.SetXml(config.GetXml());
            Assert.That(config.GetXml(), Is.EqualTo(config2.GetXml()));
        }
    }
}
