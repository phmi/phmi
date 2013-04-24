using NUnit.Framework;
using PHmiClient.Utils;

namespace PHmiClientUnitTests.Client.Utils
{
    [TestFixture]
    public class LocDisplayNameAttributeTests
    {
        private class MockResources
        {
            public static string NameInResource
            {
                get { return "Resource property"; }
            }
        }

        private class Mock
        {
            [LocDisplayName("NameInResource", ResourceType = typeof(MockResources))]
            public string Name { get; set; }
        }

        [Test]
        public void Test()
        {
            var mock = new Mock();
            var result = ReflectionHelper.GetDisplayName(mock, "Name");
            Assert.AreEqual("Resource property", result);
        }
    }
}
