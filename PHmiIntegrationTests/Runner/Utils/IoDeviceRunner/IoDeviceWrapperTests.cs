using NUnit.Framework;
using PHmiClientUnitTests;
using PHmiIoDeviceTools;
using PHmiRunner.Utils.IoDeviceRunner;
using System;
using System.Reflection;

namespace PHmiIntegrationTests.Runner.Utils.IoDeviceRunner
{
    public class WhenUsingIoDeviceWrapper : Specification
    {
        protected IIoDeviceWrapper IoDeviceWrapper;

        protected override void EstablishContext()
        {
            base.EstablishContext();
            IoDeviceWrapper = new IoDeviceWrapper();
        }

        public class AndCallingCreate : WhenUsingIoDeviceWrapper
        {
            protected const string Address = "Address";
            protected const double Value = 0.22;
            protected const string Error = "Error";
            protected const string Options = "Options";
            protected const string DisposeExceptionMessage = "DisposeExceptionMessage";
            protected const string WriteExceptionMessage = "WriteExceptionMessage";
            protected const string NotOpenedExceptionMessage = "NotOpenedExceptionMessage";

            public class IoDevice : IIoDevice
            {
                public IoDevice(string options)
                {
                    if (options != Options)
                        throw new Exception();
                }

                public void Dispose()
                {
                    throw new Exception(DisposeExceptionMessage);
                }

                public void Open()
                {
                    _opened = true;
                }

                private bool _opened;

                public object[] Read(ReadParameter[] readParameters)
                {
                    if (!_opened)
                        throw new Exception(NotOpenedExceptionMessage);
                    if (readParameters.Length == 1
                        && readParameters[0].Address == Address
                        && readParameters[0].ValueType == typeof(int))
                    {
                        return new object[] { Value };
                    }
                    throw new Exception();
                }

                public void Write(WriteParameter[] writeParameters)
                {
                    if (!_opened)
                        throw new Exception(NotOpenedExceptionMessage);
                    if (writeParameters.Length == 1
                        && writeParameters[0].Address == Address
                        && writeParameters[0].Value.Equals(Value))
                    {
                        throw new Exception(WriteExceptionMessage);
                    }
                }
            }

            protected override void EstablishContext()
            {
                base.EstablishContext();
                IoDeviceWrapper.Create(
                    Assembly.GetAssembly(typeof(IoDevice)).Location, Options);
            }

            public class AndCallingWrite : AndCallingCreate
            {
                protected string ExceptionMessage;

                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    ExceptionMessage = null;
                    try
                    {
                        IoDeviceWrapper.Write(new[] { new WriteParameter(Address, Value) });
                    }
                    catch (Exception exception)
                    {
                        ExceptionMessage = exception.Message;
                    }
                    
                }

                public class ThenIoDeviceWriteIsCalled : AndCallingWrite
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(ExceptionMessage, Is.EqualTo(WriteExceptionMessage));
                    }
                }
            }

            public class AndCallingRead : AndCallingCreate
            {
                protected object[] ReadResult;

                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    ReadResult = IoDeviceWrapper.Read(new [] {new ReadParameter(Address, typeof (int))});
                }

                public class ThenResultIsNormal : AndCallingRead
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(ReadResult, Is.EquivalentTo(new [] { Value }));
                    }
                }
            }

            public class AndCallingDispose : AndCallingCreate
            {
                protected string ExceptionMessage;

                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    ExceptionMessage = null;
                    try
                    {
                        IoDeviceWrapper.Dispose();
                    }
                    catch (Exception exception)
                    {
                        ExceptionMessage = exception.Message;
                    }
                }

                public class ThenIoDeviceDisposeInvoked : AndCallingDispose
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(ExceptionMessage, Is.EqualTo(DisposeExceptionMessage));
                    }
                }
            }
        }
    }
}
