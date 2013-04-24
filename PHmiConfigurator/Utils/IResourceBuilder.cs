using System;

namespace PHmiConfigurator.Utils
{
    public interface IResourceBuilder : IDisposable
    {
        string Add(string resource);
        void Build();
    }
}
