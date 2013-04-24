using System;
using System.ComponentModel;
using System.Windows;
using Moq;
using NUnit.Framework;
using PHmiClient.Utils.Configuration;
using PHmiClient.Utils.ViewInterfaces;

namespace PHmiClientUnitTests.Client.Utils.Configuration
{
    [TestFixture]
    public class WindowPositionSettingsTests
    {
        private const string HeightKey = "Height";
        private const string WidthKey = "Width";
        private const string TopKey = "Top";
        private const string LeftKey = "Left";
        private const string MaximizedKey = "Maximized";

        private const double Height = 100;
        private const double Width = 101;
        private const double Top = 102;
        private const double Left = 103;

        [Test]
        public void UseWithSetsValuesWhenValuesPresent()
        {
            var windowMock = new Mock<IWindow>();
            var settignsStub = new Mock<ISettings>();
            settignsStub.Setup(settings => settings.GetDouble(HeightKey)).Returns(Height);
            settignsStub.Setup(settings => settings.GetDouble(WidthKey)).Returns(Width);
            settignsStub.Setup(settings => settings.GetDouble(TopKey)).Returns(Top);
            settignsStub.Setup(settings => settings.GetDouble(LeftKey)).Returns(Left);
            settignsStub.Setup(settings => settings.GetBoolean(MaximizedKey)).Returns(true);
            WindowPositionSettings.UseWith(windowMock.Object, settignsStub.Object);
            windowMock.VerifySet(window => window.WindowStartupLocation = WindowStartupLocation.Manual);
            windowMock.VerifySet(window => window.Height = Height);
            windowMock.VerifySet(window => window.Width = Width);
            windowMock.VerifySet(window => window.Top = Top);
            windowMock.VerifySet(window => window.Left = Left);
            windowMock.VerifySet(window => window.WindowState = WindowState.Maximized);
        }

        [Test]
        public void UseWithSetsNormalWhenNotMinimized()
        {
            var windowMock = new Mock<IWindow>();
            var settignsStub = new Mock<ISettings>();
            settignsStub.Setup(settings => settings.GetDouble(HeightKey)).Returns(Height);
            settignsStub.Setup(settings => settings.GetDouble(WidthKey)).Returns(Width);
            settignsStub.Setup(settings => settings.GetDouble(TopKey)).Returns(Top);
            settignsStub.Setup(settings => settings.GetDouble(LeftKey)).Returns(Left);
            settignsStub.Setup(settings => settings.GetBoolean(MaximizedKey)).Returns(false);
            WindowPositionSettings.UseWith(windowMock.Object, settignsStub.Object);
            windowMock.VerifySet(window => window.WindowState = WindowState.Normal);
        }

        private static void UseWithDoesNotSetValuesWhenAnythingNotPresetAndSetsOtherwise(
            double? height, double? width, double? top, double? left, bool? maximized, bool shouldSet)
        {
            var windowMock = new Mock<IWindow>();
            var settignsStub = new Mock<ISettings>();
            settignsStub.Setup(settings => settings.GetDouble(HeightKey)).Returns(height);
            settignsStub.Setup(settings => settings.GetDouble(WidthKey)).Returns(width);
            settignsStub.Setup(settings => settings.GetDouble(TopKey)).Returns(top);
            settignsStub.Setup(settings => settings.GetDouble(LeftKey)).Returns(left);
            settignsStub.Setup(settings => settings.GetBoolean(MaximizedKey)).Returns(maximized);
            WindowPositionSettings.UseWith(windowMock.Object, settignsStub.Object);
            var times = shouldSet ? Times.Once() : Times.Never();
            windowMock.VerifySet(window => window.WindowStartupLocation = WindowStartupLocation.Manual, times);
            windowMock.VerifySet(window => window.Height = It.Is<Double>(h => h == height), times);
            windowMock.VerifySet(window => window.Width = It.Is<Double>(w => w == width), times);
            windowMock.VerifySet(window => window.Top = It.Is<Double>(t => t == top), times);
            windowMock.VerifySet(window => window.Left = It.Is<Double>(l => l == left), times);
            windowMock.VerifySet(window => window.WindowState = It.IsAny<WindowState>(), times);
        }

        [Test]
        public void UseWithDoesNotSetValuesWhenHeightNotPresentAndSetsOtherwise()
        {
            for (var h = 0; h <= 1; h++)
            {
                var height = h == 0 ? null : (double?) Height;
                for (var w = 0; w <= 1; w++)
                {
                    var width = w == 0 ? null : (double?) Width;
                    for (var t = 0; t <= 1; t++)
                    {
                        var top = h == 0 ? null : (double?) Top;
                        for (var l = 0; l <= 1; l++)
                        {
                            var left = l == 0 ? null : (double?) Left;
                            for (var m = 0; m <= 1; m++)
                            {
                                var maximized = m == 0 ? null : (bool?) false;
                                UseWithDoesNotSetValuesWhenAnythingNotPresetAndSetsOtherwise(
                                    height, width, top, left, maximized,
                                    height.HasValue && width.HasValue && top.HasValue && left.HasValue && maximized.HasValue);
                            }
                        }
                    }
                }
            }
        }

        [Test]
        public void ReloadsAndSavesSettingsOnClosing()
        {
            var windowStub = new Mock<IWindow>();
            var settingsMock = new Mock<ISettings>();
            WindowPositionSettings.UseWith(windowStub.Object, settingsMock.Object);
            windowStub.Raise(w => w.Closing += null, new CancelEventArgs());
            settingsMock.Verify(settings => settings.Reload());
            settingsMock.Verify(settings => settings.Save());
        }

        [Test]
        public void SetsBoundsOnClosing()
        {
            var windowStub = new Mock<IWindow>();
            var settingsMock = new Mock<ISettings>();
            WindowPositionSettings.UseWith(windowStub.Object, settingsMock.Object);
            windowStub.Setup(w => w.RestoreBounds).Returns(new Rect(Left, Top, Width, Height));
            windowStub.Raise(w => w.Closing += null, new CancelEventArgs());
            settingsMock.Verify(settings => settings.SetDouble(LeftKey, Left));
            settingsMock.Verify(settings => settings.SetDouble(TopKey, Top));
            settingsMock.Verify(settings => settings.SetDouble(WidthKey, Width));
            settingsMock.Verify(settings => settings.SetDouble(HeightKey, Height));
        }

        [Test]
        public void SetsMaximizedStateOnClosing()
        {
            var windowStub = new Mock<IWindow>();
            var settingsMock = new Mock<ISettings>();
            WindowPositionSettings.UseWith(windowStub.Object, settingsMock.Object);
            windowStub.Setup(w => w.WindowState).Returns(WindowState.Maximized);
            windowStub.Raise(w => w.Closing += null, new CancelEventArgs());
            settingsMock.Verify(settings => settings.SetBoolean(MaximizedKey, true));
        }

        [Test]
        public void SetsNotMaximizedOnClosingNormalState()
        {
            var windowStub = new Mock<IWindow>();
            var settingsMock = new Mock<ISettings>();
            WindowPositionSettings.UseWith(windowStub.Object, settingsMock.Object);
            windowStub.Setup(w => w.WindowState).Returns(WindowState.Normal);
            windowStub.Raise(w => w.Closing += null, new CancelEventArgs());
            settingsMock.Verify(settings => settings.SetBoolean(MaximizedKey, false));
        }

        [Test]
        public void SetsNotMaximizedOnClosingMinimizedState()
        {
            var windowStub = new Mock<IWindow>();
            var settingsMock = new Mock<ISettings>();
            WindowPositionSettings.UseWith(windowStub.Object, settingsMock.Object);
            windowStub.Setup(w => w.WindowState).Returns(WindowState.Minimized);
            windowStub.Raise(w => w.Closing += null, new CancelEventArgs());
            settingsMock.Verify(settings => settings.SetBoolean(MaximizedKey, false));
        }
    }
}
