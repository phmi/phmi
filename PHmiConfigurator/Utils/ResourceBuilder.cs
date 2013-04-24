using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;

namespace PHmiConfigurator.Utils
{
    public class ResourceBuilder : IResourceBuilder
    {
        private readonly IResourceWriter _resourceWriter;
        private readonly IDictionary<string, object> _resourceEntries = new Dictionary<string, object>();

        public ResourceBuilder(IResourceWriter resourceWriter)
        {
            _resourceWriter = resourceWriter;
        }

        public string Add(string resource)
        {
            if (resource == null)
                resource = string.Empty;
            var initialkey = GetKey(resource);
            var index = 1;
            var key = initialkey;
            while (true)
            {
                object value;
                if (_resourceEntries.TryGetValue(key, out value))
                {
                    if (value as string == resource)
                        return key;
                    key = initialkey + index;
                    index++;
                }
                else
                {
                    _resourceEntries.Add(key, resource);
                    return key;
                }
            }
        }

        private static string GetKey(string resource)
        {
            if (resource == string.Empty)
                return "empty";
            var key = new StringBuilder(resource.Length);
            foreach (var c in resource)
            {
                key.Append(Char.IsLetterOrDigit(c) ? c : '_');
            }
            var result = key.ToString().ToLower();
            return result.Any() && Char.IsDigit(result.First()) ? "n" + result : result;
        }

        public void Build()
        {
            foreach (var i in _resourceEntries)
            {
                _resourceWriter.AddResource(i.Key, i.Value);
            }
            _resourceWriter.Generate();
            _resourceWriter.Close();
        }

        public void Dispose()
        {
            _resourceWriter.Dispose();
        }
    }
}
