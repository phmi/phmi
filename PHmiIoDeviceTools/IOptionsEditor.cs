using System;

namespace PHmiIoDeviceTools
{
    /// <summary>
    /// IoDevice options editor to be used in configurator instead of TextBox
    /// </summary>
    public interface IOptionsEditor
    {
        void SetOptions(string options);
        string GetOptions();
        event EventHandler OptionsChanged;
    }
}
