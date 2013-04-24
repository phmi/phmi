using System.IO.Ports;
using NUnit.Framework;
using PHmiIoDevice.Melsec.Configuration;

namespace PHmiUnitTests.IoDevices.Mitsubishi.Melsec.Configuration
{
    [TestFixture]
    public class FxComConfigTests
    {
        [Test]
        public void GetSetXmlTest()
        {
            var config = new FxComConfig()
                {
                    PortName = "PortName",
                    BaudRate = 10,
                    DataBits = 11,
                    Parity = Parity.None,
                    StopBits = StopBits.None,
                    TryCount = 100
                };
            var config2 = new FxComConfig();
            config2.SetXml(config.GetXml());
            Assert.That(config.GetXml(), Is.EqualTo(config2.GetXml()));
        }
    }
}
