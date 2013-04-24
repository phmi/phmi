using System;
using System.Windows.Input;

namespace PHmiConfigurator.Modules
{
    public interface IModuleViewModel
    {
        Module View { get; set; }
        event EventHandler Closed;
        string ConnectionString { get; set; }
        bool Reload();
        bool HasChanges { get; }
        bool IsValid { get; }
        bool Save();
        ICommand ReloadCommand { get; }
        ICommand SaveCommand { get; }
        ICommand CloseCommand { get; }
    }
}
