using System;

namespace PHmiClient.Utils.Configuration
{
    public class Settings : ISettings
    {
        private readonly IStringKeeper _stringKeeper;

        public Settings(IStringKeeper stringKeeper)
        {
            _stringKeeper = stringKeeper;
        }

        public Settings(string settingsPrefix)
        {
            _stringKeeper = new StringKeeper(settingsPrefix);
        }

        public void Reload()
        {
            _stringKeeper.Reload();
        }

        public void Save()
        {
            _stringKeeper.Save();
        }

        public string GetString(string key)
        {
            return _stringKeeper.Get(key);
        }

        public void SetString(string key, string value)
        {
            _stringKeeper.Set(key, value);
        }
        
        public void Set<T>(string key, T value, Func<T, byte[]> convertFunc) where T : new()
        {
            var bytes = convertFunc.Invoke(value);
            var str = ByteConverter.BytesToString(bytes);
            SetString(key, str);
        }

        public T Get<T>(string key, Func<byte[], T> convertFunc) where T : new()
        {
            var str = GetString(key);
            if (str == null)
            {
                return default(T);
            }
            byte[] bytes;
            try
            {
                bytes = ByteConverter.StringToBytes(str);
            }
            catch (FormatException)
            {
                return default(T);
            }
            try
            {
                return convertFunc.Invoke(bytes);
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public void SetDouble(string key, Double? value)
        {
            Set(key, value, 
                val => val.HasValue ? BitConverter.GetBytes(val.Value) : null);
        }

        public Double? GetDouble(string key)
        {
            return Get<Double?>(key, bytes => BitConverter.ToDouble(bytes, 0));
        }

        public void SetBoolean(string key, Boolean? value)
        {
            Set(key, value, 
                boolean => boolean.HasValue ? BitConverter.GetBytes(boolean.Value) : null);
        }

        public Boolean? GetBoolean(string key)
        {
            return Get<Boolean?>(key, bytes => BitConverter.ToBoolean(bytes, 0));
        }

        public void SetInt32(string key, Int32? value)
        {
            Set(key, value,
                i => i.HasValue ? BitConverter.GetBytes(i.Value) : null);
        }

        public Int32? GetInt32(string key)
        {
            return Get<Int32?>(key, bytes => BitConverter.ToInt32(bytes, 0));
        }

        public void SetInt64(string key, Int64? value)
        {
            Set(key, value,
                i => i.HasValue ? BitConverter.GetBytes(i.Value) : null);
        }

        public Int64? GetInt64(string key)
        {
            return Get<Int64?>(key, bytes => BitConverter.ToInt64(bytes, 0));
        }
    }
}
