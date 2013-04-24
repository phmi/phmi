using System;
using NUnit.Framework;
using PHmiClient.Utils;

namespace PHmiClientUnitTests.Client.Utils
{
    [TestFixture]
    public class ByteConverterTests
    {
        #region BytesToString

        [Test]
        public void BytesToStringReturnsNullIfNullIsPassed()
        {
            Assert.IsNull(ByteConverter.BytesToString(null));
        }

        [Test]
        public void BytesToStringReturnsEmptyIfEmptyPassed()
        {
            Assert.IsEmpty(ByteConverter.BytesToString(new byte[0]));
        }

        [Test]
        public void BytesToStringSingleByteTest()
        {
            for (var i = 0; i <= 0xf; i++)
            {
                for (var j = 0; j <= 0xf; j++)
                {
                    var b = (byte) (i*0x10 + j);
                    var str = i.ToString("x") + j.ToString("x");
                    Assert.AreEqual(str, ByteConverter.BytesToString(new []{b}), str);
                }
            }
        }

        [Test]
        public void BytesToStringTwoBytesTest()
        {
            Assert.AreEqual("abcd", ByteConverter.BytesToString(new byte[]{0xab, 0xcd}));
        }

        #endregion

        #region StringToBytes

        [Test]
        public void StringToBytesReturnsNullIfNullIsPassed()
        {
            Assert.IsNull(ByteConverter.StringToBytes(null));
        }

        [Test]
        public void StringToBytesReturnsEmptyIfEmptyPassed()
        {
            Assert.IsEmpty(ByteConverter.StringToBytes(string.Empty));
        }

        [Test]
        public void StringToBytesSingleByteTest()
        {
            for (var i = 0; i <= 0xf; i++)
            {
                for (var j = 0; j <= 0xf; j++)
                {
                    var b = (byte)(i * 0x10 + j);
                    var str = i.ToString("x") + j.ToString("x");
                    var result = ByteConverter.StringToBytes(str);
                    Assert.AreEqual(1, result.Length);
                    Assert.AreEqual(b, result[0]);
                }
            }
        }

        [Test]
        public void StringToBytesTwoBytesTest()
        {
            var result = ByteConverter.StringToBytes("abcd");
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(0xab, result[0]);
            Assert.AreEqual(0xcd, result[1]);
        }

        [Test]
        public void StringToBytesFormatExceptionThrownOnNonHexString()
        {
            Assert.Catch<FormatException>(() => ByteConverter.StringToBytes("NonHex"));
        }

        #endregion
    }
}
