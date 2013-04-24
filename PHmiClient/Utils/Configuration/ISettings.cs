using System;

namespace PHmiClient.Utils.Configuration
{
    public interface ISettings
    {
        void Reload();
        void Save();
        string GetString(string key);
        void SetString(string key, string value);
        T Get<T>(string key, Func<byte[], T> convertFunc) where T : new();
        void Set<T>(string key, T value, Func<T, byte[]> convertFunc) where T : new();
        Double? GetDouble(string key);
        void SetDouble(string key, Double? value);
        Boolean? GetBoolean(string key);
        void SetBoolean(string key, Boolean? value);
        Int32? GetInt32(string key);
        void SetInt32(string key, Int32? value);
        Int64? GetInt64(string key);
        void SetInt64(string key, Int64? value);
    }
}
