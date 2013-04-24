using NUnit.Framework;
using PHmiIoDevice.Melsec.Configuration;

namespace PHmiUnitTests.IoDevices.Mitsubishi.Melsec.Configuration
{
    [TestFixture]
    public class EnetConfigTests
    {
        [Test]
        public void GetSetXmlTest()
        {
            var config = new EnetConfig("Name") {Address = "Address"};
            var config2 = new EnetConfig("Name"); 
            config2.SetXml(config.GetXml());
            Assert.That(config.GetXml(), Is.EqualTo(config2.GetXml()));
        }
    }
}
