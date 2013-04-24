using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using NUnit.Framework;
using Validator = PHmiClient.Utils.Validator;

namespace PHmiClientUnitTests.Client.Utils
{
    [TestFixture]
    public class ValidatorTests
    {
        private class MockValidationAttribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var mock = (Mock) value;
                return mock.Property != null
                    ? new ValidationResult("Property is NOT required of class attribute")
                    : ValidationResult.Success;
            }

            public override bool IsValid(object value)
            {
                var mock = (Mock)value;
                return mock.Property == null;
            }
        }

        [MockValidation]
        private class Mock : IDataErrorInfo
        {
            [Required(ErrorMessage = "Property is required")]
            public string Property { get; set; }

            [Required(ErrorMessage = "{0} is required")]
            [DisplayName("Property With Display Name")]
            public string PropertyWithDisplayName { get; set; }

            public string this[string columnName]
            {
                get { throw new NotImplementedException(); }
            }

            public string Error
            {
                get { throw new NotImplementedException(); }
            }
        }

        [Test]
        public void GetErrorOfPropertyReturnsError()
        {
            var mock = new Mock();
            var result = Validator.GetError(mock, "Property");
            Assert.AreEqual("Property is required", result);
        }

        [Test]
        public void GetErrorOfPropertyWithDisplayNameAttrReturnsError()
        {
            var mock = new Mock();
            var result = Validator.GetError(mock, "PropertyWithDisplayName");
            Assert.AreEqual("Property With Display Name is required", result);
        }

        [Test]
        public void GetErrorReturnsError()
        {
            var mock = new Mock
                           {
                               Property = "Property",
                               PropertyWithDisplayName = "PropertyWithDisplayName"
                           };
            var result = Validator.GetError(mock);
            Assert.AreEqual("Property is NOT required of class attribute", result);
        }

        [Test]
        public void GetErrorReturnsAllPropertyErrors()
        {
            var mock = new Mock();
            var result = Validator.GetError(mock);
            Assert.IsTrue(result.Contains("Property is required"));
            Assert.IsTrue(result.Contains("Property With Display Name is required"));
        }

        [Test]
        public void GetErrorReturnsAllErrors()
        {
            var mock = new Mock { Property = "Property" };
            var result = Validator.GetError(mock);
            Assert.AreEqual(
                string.Join(Environment.NewLine,
                "Property With Display Name is required",
                "Property is NOT required of class attribute"),
                result);
        }
    }
}
