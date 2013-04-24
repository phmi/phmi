using NUnit.Framework;
using PHmiClient.Utils;

namespace PHmiClientUnitTests.Client.Utils
{
    [TestFixture]
    public class PropertyHelperTests
    {
        #region GetPropertyName
        
        private class GetDisplayNameMock
        {
            public string Property { get; set; }
        }

        [Test]
        public void GetPropertyNameReturnsDisplayNameLinqExpressionVersion()
        {
            var result = PropertyHelper.GetPropertyName<GetDisplayNameMock>(m => m.Property);
            Assert.AreEqual("Property", result);
        }

        [Test]
        public void GetPropertyNameReturnsDisplayNameLinqExpressionVersionWithObject()
        {
            var obj = new GetDisplayNameMock();
            var result = PropertyHelper.GetPropertyName(obj, m => m.Property);
            Assert.AreEqual("Property", result);
        }

        #endregion
    }
}
