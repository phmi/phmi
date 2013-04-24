using System;
using Moq;
using NUnit.Framework;
using PHmiClient.Loc;
using PHmiClient.Tags;
using PHmiClient.Utils.Notifications;
using PHmiClient.Wcf;
using PHmiClient.Wcf.ServiceTypes;

namespace PHmiClientUnitTests.Client.Tags
{
    public class WhenUsingTagService : Specification
    {
        internal Mock<IReporter> Reporter;
        internal ITagService TagService;

        protected override void EstablishContext()
        {
            base.EstablishContext();
            Reporter = new Mock<IReporter>();
            TagService = new TagService(Reporter.Object);
        }

        public class AndAddedIoDevices : WhenUsingTagService
        {
            protected Mock<IoDeviceAbstract> IoDevice;
            protected Mock<IoDeviceAbstract> IoDevice2;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                IoDevice = new Mock<IoDeviceAbstract>();
                TagService.Add(IoDevice.Object);
                IoDevice2 = new Mock<IoDeviceAbstract>();
                TagService.Add(IoDevice2.Object);
            }

            public class AndRunInvoked : AndAddedIoDevices
            {
                internal Mock<IService> Service;

                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    Service = new Mock<IService>();
                    TagService.Run(Service.Object);
                }

                public class ThenServiceRemapTagsNotInvoked : AndRunInvoked
                {
                    [Test]
                    public void Test()
                    {
                        Service.Verify(s => s.RemapTags(It.IsAny<RemapTagsParameter[]>()), Times.Never());
                    }
                }
            }

            public class AndIoDeviceReturnsParameter : AndAddedIoDevices
            {
                internal RemapTagsParameter Parameter;

                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    Parameter = new RemapTagsParameter();
                    IoDevice.Setup(d => d.CreateRemapParameter()).Returns(Parameter).Verifiable();
                }

                public class AndWeHaveTheService : AndIoDeviceReturnsParameter
                {
                    internal Mock<IService> Service;
                    internal RemapTagsResult Result;
                    internal WcfNotification Notification;

                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        Service = new Mock<IService>();
                        Notification = new WcfNotification
                        {
                            StartTime = DateTime.UtcNow,
                            Message = "Message",
                            ShortDescription = "ShortDescription",
                            LongDescription = "LongDescription"
                        };
                        Result = new RemapTagsResult
                        {
                            Notifications = new[] { Notification }
                        };
                        Service.Setup(s => s.RemapTags(It.Is<RemapTagsParameter[]>(p => p.Length == 1 && p[0] == Parameter)))
                            .Returns(new[] { Result }).Verifiable();
                    }

                    public class AndRunInvoked1 : AndWeHaveTheService
                    {
                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            TagService.Run(Service.Object);
                        }

                        public class ThenIoDeviceCreateParameterInvoked : AndRunInvoked1
                        {
                            [Test]
                            public void Test()
                            {
                                IoDevice.Verify();
                            }
                        }

                        public class ThenServiceRemapTagsInvoked : AndRunInvoked1
                        {
                            [Test]
                            public void Test()
                            {
                                Service.Verify();
                            }
                        }

                        public class ThenIoDeviceApplyResultInvoked : AndRunInvoked1
                        {
                            [Test]
                            public void Test()
                            {
                                IoDevice.Verify(t => t.ApplyRemapResult(Result), Times.Once());
                            }
                        }

                        public class ThenNotificationIsReported : AndRunInvoked1
                        {
                            [Test]
                            public void Test()
                            {
                                Reporter.Verify(r => r.Report(
                                    Notification.StartTime.ToLocalTime() + " " + Notification.Message,
                                    Notification.ShortDescription,
                                    Notification.LongDescription), Times.Once());
                            }
                        }
                    }
                }
            }

            public class AndCleanInvoked : AndAddedIoDevices
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    TagService.Clean();
                }

                public class ThenIoDeviceApplyNullInvoked : AndCleanInvoked
                {
                    [Test]
                    public void Test()
                    {
                        IoDevice.Verify(t => t.ApplyRemapResult(null), Times.Once());
                        IoDevice2.Verify(t => t.ApplyRemapResult(null), Times.Once());
                    }
                }
            }
        }

        public class ThenNameReturnsTagService : WhenUsingTagService
        {
            [Test]
            public void Test()
            {
                Assert.That(TagService.Name, Is.EqualTo(Res.TagService));
            }
        }
    }
}
