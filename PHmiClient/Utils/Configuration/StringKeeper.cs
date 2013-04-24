using System.Configuration;

namespace PHmiClient.Utils.Configuration
{
    public class StringKeeper : IStringKeeper
    {
        public StringKeeper(string settingsPrefix)
        {
            _prefix = string.Format("{0}_", settingsPrefix);
            Reload();
        }

        private readonly string _prefix;
        private System.Configuration.Configuration _config;

        public void Reload()
        {
            _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }

        public string Get(string key)
        {
            var kValue = _config.AppSettings.Settings[_prefix + key];
            return kValue == null ? null : kValue.Value;
        }

        public void Set(string key, string value)
        {
            var k = _prefix + key;
            var kValue = _config.AppSettings.Settings[k];
            if (value == null)
            {
                if (kValue != null)
                    _config.AppSettings.Settings.Remove(k);
                return;
            }
            if (kValue == null)
                _config.AppSettings.Settings.Add(_prefix + key, value);
            else
                kValue.Value = value;
        }

        public void Save()
        {
            _config.Save();
        }
    }
}
