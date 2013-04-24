using NUnit.Framework;
using System;
using PHmiIoDevice.Generic;
using PHmiIoDeviceTools;

namespace PHmiUnitTests.IoDevices
{
    [TestFixture]
    public class GenericTests
    {
        #region Stub

        private class ParameterLessConstructorObject
        {
            public ParameterLessConstructorObject(string arg)
            {

            }
        }

        private class PrivateConstructorObject
        {
            private PrivateConstructorObject() { }
        }

        #endregion

        private GenericIoDevice _ioDevice;

        [SetUp]
        public void SetUp()
        {
            _ioDevice = new GenericIoDevice(null);
        }

        [Test]
        public void ReadReturnsNewObject()
        {
            var parameters = new []
                {
                    new ReadParameter("Addr0", typeof (object))
                };
            var result = _ioDevice.Read(parameters);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(typeof(object), result[0].GetType());
        }

        [Test]
        public void ReadDoesNotReturnParameterlessConstructorObject()
        {
            var parameters = new []
                {
                    new ReadParameter("Addr0", typeof (ParameterLessConstructorObject))
                };
            Assert.Throws<Exception>(() => _ioDevice.Read(parameters));
        }

        [Test]
        public void ReadDoesNotReturnPrivateConstructorObject()
        {
            var parameters = new[]
                {
                    new ReadParameter("Addr0", typeof (PrivateConstructorObject))
                };
            Assert.Throws<Exception>(() => _ioDevice.Read(parameters));
        }

        [Test]
        public void ReadReturnsNewStruct()
        {
            var parameters = new[]
                {
                    new ReadParameter("Addr0", typeof (Int16))
                };
            var result = _ioDevice.Read(parameters);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(typeof(Int16), result[0].GetType());
        }

        [Test]
        public void ReadTwoValues()
        {
            var parameters = new[]
                {
                    new ReadParameter("Addr0", typeof (Int32)),
                    new ReadParameter("Addr1", typeof (object))
                };
            var result = _ioDevice.Read(parameters);
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(typeof(Int32), result[0].GetType());
            Assert.AreEqual(typeof(object), result[1].GetType());
        }

        [Test]
        public void WriteTest()
        {
            var writeParameters = new[]
                {
                    new WriteParameter("Addr0", 16)
                };
            _ioDevice.Write(writeParameters);

            var readParameters = new[]
                {
                    new ReadParameter("Addr0", typeof (Int32))
                };
            var readResult = _ioDevice.Read(readParameters);
            Assert.AreEqual(1, readResult.Length);
            Assert.AreEqual(16, readResult[0]);

            writeParameters = new[]
                {
                    new WriteParameter("Addr0", -1)
                };
            _ioDevice.Write(writeParameters);

            readParameters = new[]
                {
                    new ReadParameter("Addr0", typeof (Int32))
                };
            readResult = _ioDevice.Read(readParameters);
            Assert.AreEqual(1, readResult.Length);
            Assert.AreEqual(-1, readResult[0]);
        }

        [Test]
        public void ReadTypeMismatchTest()
        {
            var writeParameters = new[]
                {
                    new WriteParameter("Addr0", 16)
                };
            _ioDevice.Write(writeParameters);

            var readParameters = new[]
                {
                    new ReadParameter("Addr0", typeof (Int16))
                };
            Assert.Throws<Exception>(() => _ioDevice.Read(readParameters));
        }
    }
}
