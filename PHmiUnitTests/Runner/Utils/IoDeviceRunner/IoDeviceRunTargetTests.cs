using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Moq;
using NUnit.Framework;
using PHmiClient.Utils.Notifications;
using PHmiClientUnitTests;
using PHmiIoDeviceTools;
using PHmiModel;
using PHmiModel.Entities;
using PHmiResources.Loc;
using PHmiRunner.Utils.IoDeviceRunner;
using PHmiTools;

namespace PHmiUnitTests.Runner.Utils.IoDeviceRunner
{
    public class WhenUsingIoDeviceRunTarget : Specification
    {
        protected IIoDeviceRunTarget IoDeviceRunTarget;
        protected Mock<IIoDeviceWrapperFactory> IoDeviceWrapperFactory;
        protected Mock<IIoDeviceWrapper> IoDeviceWrapper;
        protected PHmiModel.Entities.IoDevice IoDevice;
        protected DigTag DigitalTag;
        protected DigTag WriteOnlyDigitalTag;
        protected NumTag NumericTag;
        protected NumTag WriteOnlyNumericTag;
        protected bool? DigitalTagValue;
        protected double? NumericTagValue;
        protected const string IoDeviceFolder = "Folder";
        protected const string IoDeviceFile = "File";
        protected const string IoDeviceOptions = "Options";
        protected Mock<INotificationReporter> Reporter;

        protected override void EstablishContext()
        {
            base.EstablishContext();
            IoDeviceWrapper = new Mock<IIoDeviceWrapper>();
            IoDeviceWrapperFactory = new Mock<IIoDeviceWrapperFactory>();
            IoDeviceWrapperFactory.Setup(f => f.Create()).Returns(IoDeviceWrapper.Object);
            IoDevice = new PHmiModel.Entities.IoDevice
                {
                    Name = "IoDeviceRunTargetName",
                    Options = IoDeviceOptions,
                    Type = string.Format("{0}{1}{2}", IoDeviceFolder, PHmiConstants.PHmiIoDeviceSeparator, IoDeviceFile)
                };
            DigitalTag = new DigTag
                {
                    Id = 1,
                    Device = "M0",
                    CanRead = true
                };
            IoDevice.DigTags.Add(DigitalTag);
            WriteOnlyDigitalTag = new DigTag
                {
                    Id = 2,
                    Device = "M1",
                    CanRead = false
                };
            IoDevice.DigTags.Add(WriteOnlyDigitalTag);
            NumericTag = new NumTag
                {
                    Id = 1,
                    Device = "D0",
                    NumTagType = new NumTagType {Name = "Int32"},
                    CanRead = true
                };
            IoDevice.NumTags.Add(NumericTag);
            WriteOnlyNumericTag = new NumTag
                {
                    Id = 2,
                    Device = "D1",
                    NumTagType = new NumTagType {Name = "Int16"},
                    CanRead = false
                };
            IoDevice.NumTags.Add(WriteOnlyNumericTag);
            Reporter = new Mock<INotificationReporter>();
            IoDeviceRunTarget = new IoDeviceRunTarget(IoDevice, IoDeviceWrapperFactory.Object, Reporter.Object);
            DigitalTagValue = true;
            NumericTagValue = new Random().Next();
            IoDeviceWrapper
                .Setup(w => w.Read(It.IsAny<ReadParameter[]>()))
                .Returns(new object[] { DigitalTagValue, NumericTagValue });
        }

        public class ThenNameIsSet : WhenUsingIoDeviceRunTarget
        {
            [Test]
            public void Test()
            {
                Assert.That(IoDeviceRunTarget.Name, Is.EqualTo(string.Format(Res.IoDeviceWithName, IoDevice.Name)));
            }
        }

        protected bool? GetDigitalValue()
        {
            return IoDeviceRunTarget.GetDigitalValue(DigitalTag.Id);
        }

        protected bool? GetWriteOnlyDigitalValue()
        {
            return IoDeviceRunTarget.GetDigitalValue(WriteOnlyDigitalTag.Id);
        }

        public class ThenDigitalValueIsNull : WhenUsingIoDeviceRunTarget
        {
            [Test]
            public void TestDigitalTag()
            {
                Assert.That(GetDigitalValue(), Is.Null);
            }

            [Test]
            public void TestWriteOnlyDigitalTag()
            {
                Assert.That(GetWriteOnlyDigitalValue(), Is.Null);
            }
        }

        protected double? GetNumericValue()
        {
            return IoDeviceRunTarget.GetNumericValue(NumericTag.Id);
        }

        protected double? GetWriteOnlyNumericValue()
        {
            return IoDeviceRunTarget.GetNumericValue(WriteOnlyNumericTag.Id);
        }

        public class ThenNumericValueIsNull : WhenUsingIoDeviceRunTarget
        {
            [Test]
            public void TestNumericTag()
            {
                Assert.That(GetNumericValue(), Is.Null);
            }

            [Test]
            public void TestWriteOnlyNumericTag()
            {
                Assert.That(GetWriteOnlyNumericValue(), Is.Null);
            }
        }

        public class AndCallingRun : WhenUsingIoDeviceRunTarget
        {
            protected override void EstablishContext()
            {
                base.EstablishContext();
                IoDeviceRunTarget.Run();
            }

            public class ThenWrapperCreated : AndCallingRun
            {
                [Test]
                public void Test()
                {
                    IoDeviceWrapperFactory.Verify(f => f.Create(), Times.Once());
                }
            }

            public class ThenWrapperCreateExecuted : AndCallingRun
            {
                [Test]
                public void Test()
                {
                    var expectedFilePath =
                        Path.GetDirectoryName(Assembly.GetAssembly(typeof (IoDeviceRunTarget)).Location)
                        + Path.DirectorySeparatorChar + PHmiConstants.PHmiIoDevicesDirectoryName
                        + Path.DirectorySeparatorChar + PHmiConstants.PHmiIoDevicePrefix + IoDeviceFolder
                        + Path.DirectorySeparatorChar + PHmiConstants.PHmiIoDevicePrefix + IoDeviceFile
                        + PHmiConstants.PHmiIoDeviceExt;
                    IoDeviceWrapper.Verify(w => w.Create(expectedFilePath, IoDeviceOptions), Times.Once());
                }
            }

            public class AndCallingRunAgain : AndCallingRun
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    IoDeviceRunTarget.Run();
                }

                public class ThenWrapperNotCreated : AndCallingRunAgain
                {
                    [Test]
                    public void Test()
                    {
                        IoDeviceWrapperFactory.Verify(f => f.Create(), Times.Once());
                    }
                }

                public class ThenWrapperCreateNotExecuted : AndCallingRunAgain
                {
                    [Test]
                    public void Test()
                    {
                        IoDeviceWrapper.Verify(w => w.Create(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
                    }
                }
            }

            public class AndCallingClean : AndCallingRun
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    IoDeviceRunTarget.Clean();
                }

                public class ThenWrapperDisposeExecuted : AndCallingClean
                {
                    [Test]
                    public void Test()
                    {
                        IoDeviceWrapper.Verify(w => w.Dispose(), Times.Once());
                    }
                }

                public class ThenIoDeviceWrapperFactoryUnloadDomainExecuted : AndCallingClean
                {
                    [Test]
                    public void Test()
                    {
                        IoDeviceWrapperFactory.Verify(f => f.UnloadDomain(), Times.Once());
                    }
                }

                public class AndThenCallingRun : AndCallingClean
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        IoDeviceRunTarget.Run();
                    }

                    public class ThenWrapperCreatedAgain : AndThenCallingRun
                    {
                        [Test]
                        public void Test()
                        {
                            IoDeviceWrapperFactory.Verify(f => f.Create(), Times.Exactly(2));
                        }
                    }

                    public class ThenWrapperCreateExecutedAgain : AndThenCallingRun
                    {
                        [Test]
                        public void Test()
                        {
                            IoDeviceWrapper.Verify(w => w.Create(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
                        }
                    }
                }
            }

            public class AndIoDeviceWrapperDisposeThrows : AndCallingRun
            {
                protected Exception IoDeviceWrapperException;

                protected override void EstablishContext()
                {
                    CatchExceptionInEstablishContext = true;
                    base.EstablishContext();
                    IoDeviceWrapperException = new Exception();
                    IoDeviceWrapper.Setup(w => w.Dispose()).Throws(IoDeviceWrapperException);
                }

                public class AndCallingClean1 : AndIoDeviceWrapperDisposeThrows
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        IoDeviceRunTarget.Clean();
                    }

                    public class ThenIoDeviceWrapperDisposeExecuted : AndCallingClean1
                    {
                        [Test]
                        public void Test()
                        {
                            IoDeviceWrapper.Verify(w => w.Dispose(), Times.Once());
                        }
                    }

                    public class ThenIoDeviceWrapperFactoryUnloadDomainExecuted : AndCallingClean1
                    {
                        [Test]
                        public void Test()
                        {
                            IoDeviceWrapperFactory.Verify(f => f.UnloadDomain(), Times.Once());
                        }
                    }

                    public class ThenExceptionIsThrown : AndCallingClean1
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(ThrownException, Is.SameAs(IoDeviceWrapperException));
                        }
                    }
                }
            }

            public class AndIoDeviceWrapperFactoryUnloadDomainThrows : AndCallingRun
            {
                protected Exception IoDeviceWrapperFactoryUnloadDomainException;

                protected override void EstablishContext()
                {
                    CatchExceptionInEstablishContext = true;
                    base.EstablishContext();
                    IoDeviceWrapperFactoryUnloadDomainException = new Exception();
                    IoDeviceWrapperFactory.Setup(w => w.UnloadDomain()).Throws(IoDeviceWrapperFactoryUnloadDomainException);
                }

                public class AndCallingClean1 : AndIoDeviceWrapperFactoryUnloadDomainThrows
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        IoDeviceRunTarget.Clean();
                    }

                    public class ThenIoDeviceWrapperDisposeExecuted : AndCallingClean1
                    {
                        [Test]
                        public void Test()
                        {
                            IoDeviceWrapper.Verify(w => w.Dispose(), Times.Once());
                        }
                    }

                    public class ThenIoDeviceWrapperFactoryUnloadDomainExecuted : AndCallingClean1
                    {
                        [Test]
                        public void Test()
                        {
                            IoDeviceWrapperFactory.Verify(f => f.UnloadDomain(), Times.Once());
                        }
                    }

                    public class ThenExceptionIsThrown : AndCallingClean1
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(ThrownException, Is.SameAs(IoDeviceWrapperFactoryUnloadDomainException));
                        }
                    }
                }
            }

            public class ThenDigitalValueIsReturned : AndCallingRun
            {
                [Test]
                public void Test()
                {
                    Assert.That(GetDigitalValue(), Is.EqualTo(DigitalTagValue));
                }
            }

            public class ThenNumericTagValueIsReturned : AndCallingRun
            {
                [Test]
                public void Test()
                {
                    Assert.That(GetNumericValue(), Is.EqualTo(NumericTagValue));
                }
            }

            public class AndSettedValue1 : AndCallingRun
            {
                protected bool SettedDigitalValue;
                protected bool SettedWriteOnlyDigitalValue;
                protected double SettedNumericValue;
                protected double SettedWriteOnlyNumericValue;

                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    SettedDigitalValue = false;
                    IoDeviceRunTarget.SetDigitalValue(DigitalTag.Id, SettedDigitalValue);
                    SettedWriteOnlyDigitalValue = false;
                    IoDeviceRunTarget.SetDigitalValue(WriteOnlyDigitalTag.Id, SettedWriteOnlyDigitalValue);
                    SettedNumericValue = new Random().Next();
                    IoDeviceRunTarget.SetNumericValue(NumericTag.Id, SettedNumericValue);
                    SettedWriteOnlyNumericValue = new Random().Next();
                    IoDeviceRunTarget.SetNumericValue(WriteOnlyNumericTag.Id, SettedWriteOnlyNumericValue);
                }

                public class ThenSettedDigitalValueIsReturned : AndSettedValue1
                {
                    [Test]
                    public void TestDigitalTag()
                    {
                        Assert.That(GetDigitalValue(), Is.EqualTo(SettedDigitalValue));
                    }

                    [Test]
                    public void TestWriteOnlyDigitalTag()
                    {
                        Assert.That(GetWriteOnlyDigitalValue(), Is.EqualTo(SettedWriteOnlyDigitalValue));
                    }
                }

                public class ThenSettedNumericValueIsReturned : AndSettedValue1
                {
                    [Test]
                    public void TestNumericTag()
                    {
                        Assert.That(GetNumericValue(), Is.EqualTo(SettedNumericValue));
                    }

                    [Test]
                    public void TestWriteOnlyNumericTag()
                    {
                        Assert.That(GetWriteOnlyNumericValue(), Is.EqualTo(SettedWriteOnlyNumericValue));
                    }
                }
            }

            public class ThenWriteOnlyValuesAreNull : AndCallingRun
            {
                [Test]
                public void WriteOnlyDigitalTagTest()
                {
                    Assert.That(GetWriteOnlyDigitalValue(), Is.Null);
                }

                [Test]
                public void WriteOnlyNumericTagTest()
                {
                    Assert.That(GetWriteOnlyNumericValue(), Is.Null);
                }
            }
        }

        public class AndSettedValue : WhenUsingIoDeviceRunTarget
        {
            protected bool SettedDigitalValue;
            protected bool SettedWriteOnlyDigitalValue;
            protected double SettedNumericValue;
            protected double SettedWriteOnlyNumericValue;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                SettedDigitalValue = false;
                IoDeviceRunTarget.SetDigitalValue(DigitalTag.Id, SettedDigitalValue);
                SettedWriteOnlyDigitalValue = false;
                IoDeviceRunTarget.SetDigitalValue(WriteOnlyDigitalTag.Id, SettedWriteOnlyDigitalValue);
                var random = new Random();
                SettedNumericValue = random.Next();
                IoDeviceRunTarget.SetNumericValue(NumericTag.Id, SettedNumericValue);
                SettedWriteOnlyNumericValue = random.Next(Int16.MinValue, Int16.MaxValue);
                IoDeviceRunTarget.SetNumericValue(WriteOnlyNumericTag.Id, SettedWriteOnlyNumericValue);
            }

            public class ThenSettedDigitalValueIsReturned : AndSettedValue
            {
                [Test]
                public void TestDigitalTag()
                {
                    Assert.That(GetDigitalValue(), Is.EqualTo(SettedDigitalValue));
                }

                [Test]
                public void TestWriteOnlyDigitalTag()
                {
                    Assert.That(GetWriteOnlyDigitalValue(), Is.EqualTo(SettedWriteOnlyDigitalValue));
                }
            }

            public class ThenSettedNumericValueIsReturned : AndSettedValue
            {
                [Test]
                public void TestNumericTag()
                {
                    Assert.That(GetNumericValue(), Is.EqualTo(SettedNumericValue));
                }

                [Test]
                public void TestWriteOnlyNumericTag()
                {
                    Assert.That(GetWriteOnlyNumericValue(), Is.EqualTo(SettedWriteOnlyNumericValue));
                }
            }

            public class AndCallingRun1 : AndSettedValue
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    IoDeviceRunTarget.Run();
                }

                public class ThenWrapperWriteExecuted : AndCallingRun1
                {
                    [Test]
                    public void Test()
                    {
                        Func<WriteParameter[], bool> f = parameters =>
                            {
                                if (parameters.Length != 4)
                                    return false;
                                if (!parameters.Any(
                                        p => p.Address == DigitalTag.Device && p.Value.Equals(SettedDigitalValue)))
                                    return false;
                                if (!parameters.Any(
                                        p => p.Address == WriteOnlyDigitalTag.Device && p.Value.Equals(SettedWriteOnlyDigitalValue)))
                                    return false;
                                if (!parameters.Any(
                                        p => p.Address == NumericTag.Device && p.Value.Equals((Int32)SettedNumericValue)))
                                    return false;
                                if (!parameters.Any(
                                        p => p.Address == WriteOnlyNumericTag.Device && p.Value.Equals((Int16)SettedWriteOnlyNumericValue)))
                                    return false;
                                return true;
                            };
                        IoDeviceWrapper
                            .Verify(w => w.Write(It.Is<WriteParameter[]>(p => f.Invoke(p))), Times.Once());
                    }
                }

                public class AndCallingRunAgain : AndCallingRun1
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        IoDeviceRunTarget.Run();
                    }

                    public class ThenWrapperWriteIsNotExecuted : AndCallingRunAgain
                    {
                        [Test]
                        public void Test()
                        {
                            IoDeviceWrapper.Verify(w => w.Write(It.IsAny<WriteParameter[]>()), Times.Once());
                        }
                    }
                }

                public class ThenDigitalValueIsReturned : AndCallingRun1
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(GetDigitalValue(), Is.EqualTo(DigitalTagValue));
                    }
                }

                public class ThenNumericTagValueIsReturned : AndCallingRun1
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(GetNumericValue(), Is.EqualTo(NumericTagValue));
                    }
                }
            }

            public class AndCallingClean : AndSettedValue
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    CatchExceptionInEstablishContext = true;
                    IoDeviceRunTarget.Clean();
                }

                public class ThenDigitalValueIsNull1 : AndCallingClean
                {
                    [Test]
                    public void TestDigitalTag()
                    {
                        Assert.That(GetDigitalValue(), Is.Null);
                    }

                    [Test]
                    public void TestWriteOnlyDigitalTag()
                    {
                        Assert.That(GetWriteOnlyDigitalValue(), Is.Null);
                    }
                }

                public class ThenNumericValueIsNull1 : AndCallingClean
                {
                    [Test]
                    public void TestNumericTag()
                    {
                        Assert.That(GetNumericValue(), Is.Null);
                    }

                    [Test]
                    public void TestWriteOnlyNumericTag()
                    {
                        Assert.That(GetWriteOnlyNumericValue(), Is.Null);
                    }
                }
            }
        }

        public class ThenReporterIsConstructorArgumentReporter : WhenUsingIoDeviceRunTarget
        {
            [Test]
            public void Test()
            {
                Assert.That(IoDeviceRunTarget.Reporter, Is.SameAs(Reporter.Object));
            }
        }
    }
}
