using System.ComponentModel.DataAnnotations;
using System.Globalization;
using NUnit.Framework;
using PHmiClient.Utils.ValidationAttributes;

namespace PHmiClientUnitTests.Client.Utils.ValidationAttributes
{
    [TestFixture]
    public class ValidDoubleAttributeTests
    {
        private ValidDoubleAttribute _attribute;
        private ValidationContext _context;

        [SetUp]
        public void SetUp()
        {
            _attribute = new ValidDoubleAttribute();
            _context = new ValidationContext(new object(), null, null);
        }

        [Test]
        public void AllowInfinityTrueTest()
        {
            _attribute.AllowInfinity = true;
            Assert.AreEqual(ValidationResult.Success, _attribute.GetValidationResult(
                double.PositiveInfinity.ToString(CultureInfo.CurrentCulture), _context));
            Assert.AreEqual(ValidationResult.Success, _attribute.GetValidationResult(
                double.NegativeInfinity.ToString(CultureInfo.CurrentCulture), _context));
        }

        [Test]
        public void AllowInfinityFalseTest()
        {
            _attribute.AllowInfinity = false;
            Assert.AreNotEqual(ValidationResult.Success, _attribute.GetValidationResult(
                double.PositiveInfinity.ToString(CultureInfo.CurrentCulture), _context));
            Assert.AreNotEqual(ValidationResult.Success, _attribute.GetValidationResult(
                double.NegativeInfinity.ToString(CultureInfo.CurrentCulture), _context));
        }

        [Test]
        public void AllowNaNTrueTest()
        {
            _attribute.AllowNaN = true;
            Assert.AreEqual(ValidationResult.Success, _attribute.GetValidationResult(
                double.NaN.ToString(CultureInfo.CurrentCulture), _context));
        }

        [Test]
        public void AllowNaNFalseTest()
        {
            _attribute.AllowNaN = false;
            Assert.AreNotEqual(ValidationResult.Success, _attribute.GetValidationResult(
                double.NaN.ToString(CultureInfo.CurrentCulture), _context));
        }

        [Test]
        public void AllowNullTrueTest()
        {
            _attribute.AllowNull = true;
            Assert.AreEqual(ValidationResult.Success, _attribute.GetValidationResult(null, _context));
            Assert.AreEqual(ValidationResult.Success, _attribute.GetValidationResult(string.Empty, _context));
        }

        [Test]
        public void AllowNullFalseTest()
        {
            _attribute.AllowNull = false;
            Assert.AreNotEqual(ValidationResult.Success, _attribute.GetValidationResult(null, _context));
            Assert.AreNotEqual(ValidationResult.Success, _attribute.GetValidationResult(string.Empty, _context));
        }

        [Test]
        public void VaidTest()
        {
            Assert.AreEqual(ValidationResult.Success, _attribute.GetValidationResult(
                double.Epsilon.ToString(CultureInfo.CurrentCulture), _context));
        }

        [Test]
        public void InvalidTest()
        {
            Assert.AreNotEqual(ValidationResult.Success, _attribute.GetValidationResult("NotDoubleAtAll", _context));
        }
    }
}
