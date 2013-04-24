using System.Windows;
using NUnit.Framework;
using PHmiClient.Converters;

namespace PHmiClientUnitTests.Client.Converters
{
    [TestFixture]
    public class FalseToCollapsedConverterTests
    {
        [Test]
        public void ConvertReturnsCollapsedIfValueIsFalse()
        {
            var conv = new FalseToCollapsedConverter();
            Assert.AreEqual(Visibility.Collapsed, conv.Convert(false, null, null, null));
        }

        [Test]
        public void ConvertReturnsVisibleIfValueIsTrue()
        {
            var conv = new FalseToCollapsedConverter();
            Assert.AreEqual(Visibility.Visible, conv.Convert(true, null, null, null));
        }

        [Test]
        public void ConvertReturnsCollapsedIfValueIsNull()
        {
            var conv = new FalseToCollapsedConverter();
            Assert.AreEqual(Visibility.Collapsed, conv.Convert(null, null, null, null));
        }

        [Test]
        public void ConvertBackReturnsTrueIfValueIsVisible()
        {
            var conv = new FalseToCollapsedConverter();
            Assert.AreEqual(true, conv.ConvertBack(Visibility.Visible, null, null, null));
        }

        [Test]
        public void ConvertBackReturnsFalseIfValueIsCollapsed()
        {
            var conv = new FalseToCollapsedConverter();
            Assert.AreEqual(false, conv.ConvertBack(Visibility.Collapsed, null, null, null));
        }

        [Test]
        public void ConvertBackReturnsFalseIfValueIsHidden()
        {
            var conv = new FalseToCollapsedConverter();
            Assert.AreEqual(false, conv.ConvertBack(Visibility.Hidden, null, null, null));
        }

    }
}
