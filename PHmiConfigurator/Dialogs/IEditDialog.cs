using PHmiClient.Utils.ViewInterfaces;

namespace PHmiConfigurator.Dialogs
{
    public interface IEditDialog<T> : IWindow
    {
        T Entity { get; set; }
    }
}
