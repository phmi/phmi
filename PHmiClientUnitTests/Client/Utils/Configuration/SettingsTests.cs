using System;
using Moq;
using NUnit.Framework;
using PHmiClient.Utils.Configuration;

namespace PHmiClientUnitTests.Client.Utils.Configuration
{
    [TestFixture]
    public class SettingsTests
    {
        private const string Key = "Key";

        [Test]
        public void ReloadTest()
        {
            var keeperMock = new Mock<IStringKeeper>();
            var settings = new Settings(keeperMock.Object);
            settings.Reload();
            keeperMock.Verify(keeper => keeper.Reload());
        }

        [Test]
        public void SaveTest()
        {
            var keeperMock = new Mock<IStringKeeper>();
            var settings = new Settings(keeperMock.Object);
            settings.Save();
            keeperMock.Verify(keeper => keeper.Save());
        }

        [Test]
        public void GetStringTest()
        {
            const string value = "Value";
            var keeperMock = new Mock<IStringKeeper>(MockBehavior.Strict);
            keeperMock.Setup(keeper => keeper.Get(Key)).Returns(value).Verifiable();
            var settings = new Settings(keeperMock.Object);
            var result = settings.GetString(Key);
            keeperMock.Verify();
            Assert.AreEqual(value, result);
        }

        [Test]
        public void SetStringTest()
        {
            const string value = "Value";
            var keeperMock = new Mock<IStringKeeper>();
            var settings = new Settings(keeperMock.Object);
            settings.SetString(Key, value);
            keeperMock.Verify(keeper => keeper.Set(Key, value));
        }

        #region Struct

        [Test]
        public void GetStructReturnsDefaultWhenKeeperReturnsNull()
        {
            var keeperStub = new Mock<IStringKeeper>();
            keeperStub.Setup(keeper => keeper.Get(Key)).Returns((string) null);

            var settings = new Settings(keeperStub.Object);
            var result = settings.Get(Key, bytes => 1);
            Assert.AreEqual(0, result);
        }

        [Test]
        public void GetStructReturnsDefaultWhenKeeperReturnsNonByteString()
        {
            var keeperStub = new Mock<IStringKeeper>();
            keeperStub.Setup(keeper => keeper.Get(Key)).Returns("NonByteString");

            var settings = new Settings(keeperStub.Object);
            var result = settings.Get(Key, bytes => 1);
            Assert.AreEqual(0, result);
        }

        [Test]
        public void GetStructReturnsDefaultWhenConvertFuncThrows()
        {
            var keeperStub = new Mock<IStringKeeper>();
            keeperStub.Setup(keeper => keeper.Get(Key)).Returns("NonByteString");

            var settings = new Settings(keeperStub.Object);
            var result = settings.Get<Int32>(Key, bytes => { throw new Exception(); });
            Assert.AreEqual(0, result);
        }

        [Test]
        public void GetStructReturnsValueWhenKeeperReturnsString()
        {
            const Int32 value = 0x12345678;
            const string keeperString = "12345678";

            var keeperStub = new Mock<IStringKeeper>();
            keeperStub.Setup(keeper => keeper.Get(Key)).Returns(keeperString);

            var settings = new Settings(keeperStub.Object);
            Assert.AreEqual(value, settings.Get(Key, bytes => value));
        }

        [Test]
        public void SetStructSetsBytesToKeeper()
        {
            const Int32 value = 0x01234567;
            var keeperMock = new Mock<IStringKeeper>();
            
            var settings = new Settings(keeperMock.Object);
            settings.Set(Key, value, i => new byte[] { 0x01, 0x23, 0x45, 0x67 });

            keeperMock.Verify(keeper => keeper.Set(Key, "01234567"));
        }

        #endregion

        #region Class

        [Test]
        public void GetClassReturnsNullWhenKeeperReturnsNull()
        {
            var keeperStub = new Mock<IStringKeeper>();
            keeperStub.Setup(keeper => keeper.Get(Key)).Returns((string) null);
            var settings = new Settings(keeperStub.Object);
            var result = settings.Get(Key, bytes => new object());
            Assert.IsNull(result);
        }

        [Test]
        public void GetClassReturnsNullWhenKeeperReturnsNonByteString()
        {
            var keeperStub = new Mock<IStringKeeper>();
            keeperStub.Setup(keeper => keeper.Get(Key)).Returns("NonByteString");
            var settings = new Settings(keeperStub.Object);
            var result = settings.Get(Key, bytes => new object());
            Assert.IsNull(result);
        }

        [Test]
        public void GetClassReturnsNullWhenKeeperConvertFuncThrows()
        {
            var keeperStub = new Mock<IStringKeeper>();
            keeperStub.Setup(keeper => keeper.Get(Key)).Returns("NonByteString");
            var settings = new Settings(keeperStub.Object);
            var result = settings.Get<object>(Key, bytes => { throw new Exception(); });
            Assert.IsNull(result);
        }

        [Test]
        public void GetClassReturnsValueWhenKeeperReturnsString()
        {
            var keeperStub = new Mock<IStringKeeper>();
            keeperStub.Setup(keeper => keeper.Get(Key)).Returns("abcd");
            var value = new object();
            var settings = new Settings(keeperStub.Object);
            var result = settings.Get(Key, bytes => value);
            Assert.AreSame(value, result);
        }

        [Test]
        public void SetClassSetsBytesToKeeper()
        {
            var value = new object();
            var keeperMock = new Mock<IStringKeeper>();

            var settings = new Settings(keeperMock.Object);
            settings.Set(Key, value, obj => new byte[] { 0x01, 0x23, 0x45, 0x67 });

            keeperMock.Verify(keeper => keeper.Set(Key, "01234567"));
        }

        #endregion

        private static void SetTest<T>(T value, Action<Settings, string, T> setAction, string expectedKeeperParam)
        {
            var keeperMock = new Mock<IStringKeeper>();
            var settings = new Settings(keeperMock.Object);
            setAction.Invoke(settings, Key, value);
            keeperMock.Verify(keeper => keeper.Set(Key, expectedKeeperParam));
        }

        private static void GetTest<T>(string keeperValue, Func<Settings, string, T> getFunc, T expectedResult)
        {
            var keeperStub = new Mock<IStringKeeper>();
            keeperStub.Setup(keeper => keeper.Get(Key)).Returns(keeperValue);
            var settings = new Settings(keeperStub.Object);
            var result = getFunc.Invoke(settings, Key);
            Assert.AreEqual(expectedResult, result);
        }

        #region Double

        readonly Action<Settings, string, Double?> _setDoubleAction = (settings, key, value) => settings.SetDouble(key, value);

        [Test]
        public void SetDoubleTest()
        {
            SetTest(Math.E, _setDoubleAction, "6957148b0abf0540");
        }

        [Test]
        public void SetDoubleNullTest()
        {
            SetTest(null, _setDoubleAction, null);
        }

        [Test]
        public void SetDoubleMinTest()
        {
            SetTest(Double.MinValue, _setDoubleAction, "ffffffffffffefff");
        }

        [Test]
        public void SetDoubleMaxTest()
        {
            SetTest(Double.MaxValue, _setDoubleAction, "ffffffffffffef7f");
        }

        private readonly Func<Settings, string, Double?> _getDoubleFunc = (settings, key) => settings.GetDouble(key);

        [Test]
        public void GetDoubleTest()
        {
            GetTest("6957148b0abf0540", _getDoubleFunc, Math.E);
        }

        [Test]
        public void GetDoubleNullTest()
        {
            GetTest(null, _getDoubleFunc, null);
        }

        [Test]
        public void GetDoubleMinTest()
        {
            GetTest("ffffffffffffefff", _getDoubleFunc, Double.MinValue);
        }

        [Test]
        public void GetDoubleMaxTest()
        {
            GetTest("ffffffffffffef7f", _getDoubleFunc, Double.MaxValue);
        }

        #endregion

        #region Boolean

        readonly Action<Settings, string, Boolean?> _setBooleanAction = (settings, key, value) => settings.SetBoolean(key, value); 

        [Test]
        public void SetBooleanTrueTest()
        {
            SetTest(true, _setBooleanAction, "01");
        }

        [Test]
        public void SetBooleanFalseTest()
        {
            SetTest(false, _setBooleanAction, "00");
        }

        [Test]
        public void SetBooleanNullTest()
        {
            SetTest(null, _setBooleanAction, null);
        }

        private readonly Func<Settings, string, Boolean?> _getBooleanFunc = (settings, key) => settings.GetBoolean(key);

        [Test]
        public void GetBooleanTrueTest()
        {
            GetTest("01", _getBooleanFunc, true);
        }

        [Test]
        public void GetBooleanFalseTest()
        {
            GetTest("00", _getBooleanFunc, false);
        }

        [Test]
        public void GetBooleanNullTest()
        {
            GetTest(null, _getBooleanFunc, null);
        }

        #endregion

        #region Int32

        readonly Action<Settings, string, Int32?> _setInt32Action = (settings, key, value) => settings.SetInt32(key, value);

        [Test]
        public void SetInt32Test()
        {
            SetTest(0, _setInt32Action, "00000000");
        }

        [Test]
        public void SetInt32NullTest()
        {
            SetTest(null, _setInt32Action, null);
        }

        [Test]
        public void SetInt32MinTest()
        {
            SetTest(Int32.MinValue, _setInt32Action, "00000080");
        }

        [Test]
        public void SetInt32MaxTest()
        {
            SetTest(Int32.MaxValue, _setInt32Action, "ffffff7f");
        }

        private readonly Func<Settings, string, Int32?> _getInt32Func = (settings, key) => settings.GetInt32(key);

        [Test]
        public void GetInt32Test()
        {
            GetTest("00000000", _getInt32Func, 0);
        }

        [Test]
        public void GetInt32NullTest()
        {
            GetTest(null, _getInt32Func, null);
        }

        [Test]
        public void GetInt32MinTest()
        {
            GetTest("00000080", _getInt32Func, Int32.MinValue);
        }

        [Test]
        public void GetInt32MaxTest()
        {
            GetTest("ffffff7f", _getInt32Func, Int32.MaxValue);
        }

        #endregion

        #region Int64

        readonly Action<Settings, string, Int64?> _setInt64Action = (settings, key, value) => settings.SetInt64(key, value);

        [Test]
        public void SetInt64Test()
        {
            SetTest(0, _setInt64Action, "0000000000000000");
        }

        [Test]
        public void SetInt64NullTest()
        {
            SetTest(null, _setInt64Action, null);
        }

        [Test]
        public void SetInt64MinTest()
        {
            SetTest(Int64.MinValue, _setInt64Action, "0000000000000080");
        }

        [Test]
        public void SetInt64MaxTest()
        {
            SetTest(Int64.MaxValue, _setInt64Action, "ffffffffffffff7f");
        }

        private readonly Func<Settings, string, Int64?> _getInt64Func = (settings, key) => settings.GetInt64(key);

        [Test]
        public void GetInt64Test()
        {
            GetTest("0000000000000000", _getInt64Func, 0);
        }

        [Test]
        public void GetInt64NullTest()
        {
            GetTest(null, _getInt64Func, null);
        }

        [Test]
        public void GetInt64MinTest()
        {
            GetTest("0000000000000080", _getInt64Func, Int64.MinValue);
        }

        [Test]
        public void GetInt64MaxTest()
        {
            GetTest("ffffffffffffff7f", _getInt64Func, Int64.MaxValue);
        }

        #endregion
    }
}