using System;
using PHmiTools.Views;

namespace PHmiConfigurator.Modules
{
    public interface IModule : IView
    {
        string ConnectionString { get; set; }
        event EventHandler Closed;
        bool HasChanges { get; }
        bool IsValid { get; }
        void Save();
    }
}
