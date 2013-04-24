using System;
using System.Windows;
using PHmiClient.Utils.ViewInterfaces;

namespace PHmiClient.Utils.Configuration
{
    public class WindowPositionSettings
    {
        private readonly ISettings _settings;
        private readonly IWindow _window;

        private WindowPositionSettings(IWindow window, ISettings settings)
        {
            _settings = settings;
            _window = window;
            _window.Closing += WindowClosing;
            Load();
        }

        private WindowPositionSettings(IWindow window, string settingsPrefix)
            : this(window, new Settings(settingsPrefix)) { }

        public static WindowPositionSettings UseWith(IWindow window, string settingsPrefix)
        {
            return new WindowPositionSettings(window, settingsPrefix);
        }

        internal static WindowPositionSettings UseWith(IWindow window, ISettings settings)
        {
            return new WindowPositionSettings(window, settings);
        }

        private void WindowClosing(object sender, EventArgs e)
        {
            Save();
        }

        private void Load()
        {
            var top = _settings.GetDouble("Top");
            var left = _settings.GetDouble("Left");
            var width = _settings.GetDouble("Width");
            var height = _settings.GetDouble("Height");
            var maximized = _settings.GetBoolean("Maximized");
            if (top.HasValue && left.HasValue && width.HasValue && height.HasValue && maximized.HasValue)
            {
                _window.WindowStartupLocation = WindowStartupLocation.Manual;
                _window.Top = top.Value;
                _window.Left = left.Value;
                _window.Width = width.Value;
                _window.Height = height.Value;
                _window.WindowState = maximized.Value ? WindowState.Maximized : WindowState.Normal;
            }
        }

        private void Save()
        {
            _settings.Reload();
            var position = _window.RestoreBounds;
            _settings.SetDouble("Top", position.Top);
            _settings.SetDouble("Left", position.Left);
            _settings.SetDouble("Width", position.Width);
            _settings.SetDouble("Height", position.Height);
            _settings.SetBoolean("Maximized", _window.WindowState == WindowState.Maximized);
            _settings.Save();
        }
    }
}
