using NUnit.Framework;
using PHmiClientUnitTests;
using PHmiIoDevice.Melsec;
using PHmiIoDevice.Melsec.Configuration;

namespace PHmiUnitTests.IoDevices.Mitsubishi.Melsec
{
    public class WhenUsingMelsecOptionsEditorViewModel : Specification
    {
        protected MelsecOptionsEditorViewModel ViewModel;

        protected override void EstablishContext()
        {
            base.EstablishContext();
            ViewModel = new MelsecOptionsEditorViewModel();
        }

        public class AndSettingConfigProperty : WhenUsingMelsecOptionsEditorViewModel
        {
            protected Config Config;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                Config = new FxEnetConfig();
                ViewModel.Config = Config;
            }

            public class ThenConfigTypeChanges : AndSettingConfigProperty
            {
                [Test]
                public void Test()
                {
                    Assert.That(ViewModel.ConfigType, Is.EqualTo(ConfigType.FxEnet));
                }
            }

            public class ThenConfigStays : AndSettingConfigProperty
            {
                [Test]
                public void Test()
                {
                    Assert.That(ViewModel.Config, Is.SameAs(Config));
                }
            }

            public class AndSettingConfigType : AndSettingConfigProperty
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    ViewModel.ConfigType = ConfigType.Q;
                }

                public class ThenConfigChanges : AndSettingConfigType
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(ViewModel.Config, Is.InstanceOf<QConfig>());
                    }
                }

                public class ThenConfigTypeStais : AndSettingConfigType
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(ViewModel.ConfigType, Is.EqualTo(ConfigType.Q));
                    }
                }
            }
        }
    }
}
