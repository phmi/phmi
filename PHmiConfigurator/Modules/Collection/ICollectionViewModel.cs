using System.Windows.Input;

namespace PHmiConfigurator.Modules.Collection
{
    public interface ICollectionViewModel : IModuleViewModel
    {
        ICommand AddCommand { get; }
        ICommand EditCommand { get; }
        ICommand DeleteCommand { get; }
        ICommand CopyCommand { get; }
        ICommand PasteCommand { get; }
        ICommand UnselectCommand { get; }
    }
}
