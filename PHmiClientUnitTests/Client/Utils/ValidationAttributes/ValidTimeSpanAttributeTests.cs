using System;
using System.ComponentModel.DataAnnotations;
using NUnit.Framework;
using PHmiClient.Utils.ValidationAttributes;

namespace PHmiClientUnitTests.Client.Utils.ValidationAttributes
{
    [TestFixture]
    public class ValidTimeSpanAttributeTests
    {
        private ValidationContext _context;
        private ValidTimeSpanAttribute _attribute;

        [SetUp]
        public void SetUp()
        {
            _context = new ValidationContext(new object(), null, null);
            _attribute = new ValidTimeSpanAttribute();
        }

        [Test]
        public void AllowNullFalseTest()
        {
            _attribute.AllowNull = false;
            Assert.AreNotEqual(ValidationResult.Success, _attribute.GetValidationResult(null, _context));
            Assert.AreNotEqual(ValidationResult.Success, _attribute.GetValidationResult(string.Empty, _context));
        }

        [Test]
        public void AllowNullTrueTest()
        {
            _attribute.AllowNull = true;
            Assert.AreEqual(ValidationResult.Success, _attribute.GetValidationResult(null, _context));
            Assert.AreEqual(ValidationResult.Success, _attribute.GetValidationResult(string.Empty, _context));
        }

        [Test]
        public void ValidTest()
        {
            Assert.AreEqual(ValidationResult.Success, _attribute.GetValidationResult(new TimeSpan(1, 1, 1, 1).ToString(), _context));
        }

        [Test]
        public void InvalidTest()
        {
            Assert.AreNotEqual(ValidationResult.Success, _attribute.GetValidationResult("31.01:444:55.55555", _context));
        }
    }
}
