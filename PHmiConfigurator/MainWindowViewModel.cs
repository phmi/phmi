using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using PHmiClient.Controls.Input;
using PHmiClient.Utils.ViewInterfaces;
using PHmiConfigurator.Dialogs;
using PHmiConfigurator.Modules;
using PHmiConfigurator.Modules.Collection;
using PHmiResources.Loc;
using PHmiTools;
using PHmiTools.Dialogs;
using PHmiTools.Dialogs.Project;
using PHmiTools.Utils.Npg;
using PHmiTools.ViewModels;

namespace PHmiConfigurator
{
    public class MainWindowViewModel : ViewModelBase<IWindow>
    {
        private readonly IMainWindowService _service;
        private readonly ObservableCollection<object> _modules = new ObservableCollection<object>();
        private readonly ReadOnlyObservableCollection<object> _readOnlyModules;
        private readonly DelegateCommand _openModuleCommand;
        private readonly ICommand _exitCommand;
        private readonly ICommand _newProjectCommand;
        private readonly ICommand _openCommand;
        private readonly DelegateCommand _closeCommand;
        private readonly DelegateCommand _exportCommand;
        private readonly ICommand _importCommand;
        private readonly DelegateCommand _buildClientCommand;
        private readonly DelegateCommand _runProjectCommand;
        private readonly ICommand _installServiceCommand;
        private readonly DelegateCommand _uninstallServiceCommand;
        private INpgConnectionParameters _connectionParameters;
        private IModuleViewModel _selectedModuleViewModel;

        public MainWindowViewModel() : this(new MainWindowService()) { }

        internal MainWindowViewModel(IMainWindowService service)
        {
            _service = service;
            _readOnlyModules = new ReadOnlyObservableCollection<object>(_modules);
            _openModuleCommand = new DelegateCommand(OpenModuleCommandExecuted, OpenModuleCommandCanExecute);
            _exitCommand = new DelegateCommand(ExitCommandExecuted);
            _newProjectCommand = new DelegateCommand(NewProjectCommandExecuted);
            _openCommand = new DelegateCommand(OpenCommandExecuted);
            _closeCommand = new DelegateCommand(CloseCommandExecuted, CloseCommandCanExecute);
            _exportCommand = new DelegateCommand(ExportCommandExecuted, ExportCommandCanExecute);
            _importCommand = new DelegateCommand(ImportCommandExecuted);
            _quickStartCommand = new DelegateCommand(QuickStartExecuted);
            _aboutCommand = new DelegateCommand(AboutCommandExecuted);
            _buildClientCommand = new DelegateCommand(BuildClientCommandExecuted, BuildClientCommandCanExecute);
            _runProjectCommand = new DelegateCommand(RunProjectCommandExecuted, RunProjectCommandCanExecute);
            _installServiceCommand = new DelegateCommand(InstallServiceCommandExecuted);
            _uninstallServiceCommand = new DelegateCommand(UninstallServiceCommandExecuted);

            _reloadModuleCommand = new DelegateCommand(ReloadModuleCommandExecuted);
            _saveModuleCommand = new DelegateCommand(SaveModuleCommandExecuted);
            _closeModuleCommand = new DelegateCommand(CloseModuleCommandExecuted);
            _addModuleCommand = new DelegateCommand(AddModuleCommandExecuted);
            _editModuleCommand = new DelegateCommand(EditModuleCommandExecuted);
            _deleteModuleCommand = new DelegateCommand(DeleteModuleCommandExecuted);
            _copyModuleCommand = new DelegateCommand(CopyModuleCommandExecuted);
            _pasteModuleCommand = new DelegateCommand(PasteModuleCommandExecuted);
            _unselectModuleCommand = new DelegateCommand(UnselectModuleCommandExecuted);
        }

        #region Modules

        public ReadOnlyObservableCollection<object> Modules
        {
            get { return _readOnlyModules; }
        }

        public void OpenModule<T>()
        {
            var type = typeof(T);
            OpenModule(type);
        }

        private void OpenModule(Type type)
        {
            var present = _modules.FirstOrDefault(m => m.GetType() == type);
            if (present != null)
            {
                TryFocus(present);
                return;
            }

            var constructor = type.GetConstructor(new Type[0]);
            if (constructor == null)
            {
                throw new ArgumentException("type must have a parameterless constructor");
            }
            var item = constructor.Invoke(new object[0]);
            _modules.Add(item);
            var module = item as IModule;
            if (module != null)
            {
                module.ConnectionString = ConnectionParameters.ConnectionString;
                module.Closed += ModuleClosed;
            }
            TryFocus(item);
        }

        private static void TryFocus(object module)
        {
            var inputElement = module as IInputElement;
            if (inputElement != null)
                inputElement.Focus();
        }

        private void ModuleClosed(object sender, EventArgs e)
        {
            var module = (IModule)sender;
            module.Closed -= ModuleClosed;
            _modules.Remove(sender);
        }

        #region OpenModuleCommand

        public ICommand OpenModuleCommand
        {
            get { return _openModuleCommand; }
        }

        private bool OpenModuleCommandCanExecute(object obj)
        {
            return ConnectionParameters != null;
        }

        private void OpenModuleCommandExecuted(object parameter)
        {
            if (parameter == null)
                throw new ArgumentNullException("parameter");
            var type = parameter as Type;
            if (type == null)
                throw new ArgumentException("parameter must be a Type");
            OpenModule(type);
        }

        #endregion
        
        public bool HasChanges
        {
            get
            {
                return _modules.OfType<IModule>().Any(m => m.HasChanges);
            }
        }

        public bool IsValid
        {
            get
            {
                return _modules.OfType<IModule>().All(m => m.IsValid);
            }
        }

        public void Save()
        {
            foreach (var module in _modules.OfType<IModule>())
            {
                module.Save();
            }
        }

        #endregion

        public INpgConnectionParameters ConnectionParameters
        {
            get { return _connectionParameters; }
            set
            {
                var modules = _modules.OfType<IModule>().Where(m => m.HasChanges).ToArray();
                if (modules.Any())
                {
                    var result = _service.DialogHelper.Message(
                            string.Format(
                            "{0}{1}{2}", Res.ThereAreChangesMessage, Environment.NewLine, Res.SaveChangesQuestion),
                            Title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question, View);
                    if (result == true)
                    {
                        if (modules.Any(m => !m.IsValid))
                        {
                            _service.DialogHelper.Message(
                                Res.ValidationErrorMessage, Res.ValidationError, MessageBoxButton.OK, MessageBoxImage.Error, View);
                            return;
                        }
                        foreach (var module in modules)
                        {
                            module.Save();
                        }
                    }
                    if (result == null)
                    {
                        return;
                    }
                }
                _connectionParameters = value;
                _modules.Clear();
                OnPropertyChanged(this, v => v.ConnectionParameters);
                _openModuleCommand.RaiseCanExecuteChanged();
                _closeCommand.RaiseCanExecuteChanged();
                _exportCommand.RaiseCanExecuteChanged();
                _buildClientCommand.RaiseCanExecuteChanged();
                _runProjectCommand.RaiseCanExecuteChanged();
                OnPropertyChanged(this, v => v.Title);
            }
        }

        #region ExitCommand

        public ICommand ExitCommand
        {
            get { return _exitCommand; }
        }

        private void ExitCommandExecuted(object parameter)
        {
            View.Close();
        }

        #endregion

        #region NewProjectCommand

        public ICommand NewProjectCommand
        {
            get { return _newProjectCommand; }
        }

        private void NewProjectCommandExecuted(object obj)
        {
            var dialog = new NewProjectDialog();
            if (dialog.ShowDialog() == true)
            {
                ConnectionParameters = dialog.ViewModel.ConnectionParameters;
            }
        }

        #endregion

        #region OpenCommand

        public ICommand OpenCommand
        {
            get { return _openCommand; }
        }
        
        private void OpenCommandExecuted(object obj)
        {
            var dialog = new OpenProjectDialog();
            if (dialog.ShowDialog() == true)
            {
                ConnectionParameters = dialog.ViewModel.ConnectionParameters;
            }
        }

        #endregion

        #region CloseCommand

        public ICommand CloseCommand
        {
            get { return _closeCommand; }
        }

        private bool CloseCommandCanExecute(object obj)
        {
            return ConnectionParameters != null;
        }

        private void CloseCommandExecuted(object obj)
        {
            ConnectionParameters = null;
        }

        #endregion

        #region ExportCommand

        public ICommand ExportCommand
        {
            get { return _exportCommand; }
        }

        private bool ExportCommandCanExecute(object obj)
        {
            return ConnectionParameters != null;
        }

        private void ExportCommandExecuted(object obj)
        {
            var w = new ExportProjectDialog(ConnectionParameters);
            w.ShowDialog();
        }

        #endregion

        #region ImportCommand

        public ICommand ImportCommand
        {
            get { return _importCommand; }
        }

        private void ImportCommandExecuted(object obj)
        {
            var dialog = new ImportProjectDialog();
            if (dialog.ShowDialog() == true)
            {
                ConnectionParameters = dialog.ViewModel.ConnectionParameters;
            }
        }

        #endregion

        #region QuickStartCommand

        private readonly ICommand _quickStartCommand;

        public ICommand QuickStartCommand { get { return _quickStartCommand; } }

        private void QuickStartExecuted(object obj)
        {
            try
            {
                var assemblyLocation = Assembly.GetAssembly(typeof(MainWindowViewModel)).Location;
                var dirPath = Path.GetDirectoryName(assemblyLocation);
                if (dirPath == null)
                    return;
                var filePath = Path.Combine(dirPath, "PHmi Quick start guide.pdf");
                Process.Start(filePath);
            }
            catch (Exception exception)
            {
                ExceptionDialog.Show(exception, View);
            }
        }

        #endregion

        #region AboutCommand

        private readonly ICommand _aboutCommand;

        public ICommand AboutCommand { get { return _aboutCommand; } }
        
        private void AboutCommandExecuted(object obj)
        {
            var w = new AboutDialog { Owner = View as Window };
            w.ShowDialog();
        }

        #endregion

        #region BuildClientCommand

        public ICommand BuildClientCommand
        {
            get { return _buildClientCommand; }
        }

        private void BuildClientCommandExecuted(object obj)
        {
            var w = new BuildClient
                {
                    ConnectionString = ConnectionParameters.ConnectionString,
                    Owner = View as Window
                };
            w.ShowDialog();
        }

        private bool BuildClientCommandCanExecute(object obj)
        {
            return ConnectionParameters != null;
        }

        #endregion

        #region RunProjectCommand

        public ICommand RunProjectCommand
        {
            get { return _runProjectCommand; }
        }

        private bool RunProjectCommandCanExecute(object obj)
        {
            return ConnectionParameters != null;
        }

        private void RunProjectCommandExecuted(object obj)
        {
            try
            {
                var proc = new ProcessStartInfo
                    {
                        FileName = string.Format("{0}.exe", PHmiConstants.PHmiRunnerName),
                        Arguments = ConnectionParameters.ConnectionString
                    };
                Process.Start(proc);
            }
            catch (Win32Exception)
            {
            }
            catch (Exception exception)
            {
                _service.DialogHelper.Exception(exception, View);
            }
        }

        #endregion

        #region InstallServiceCommand

        public ICommand InstallServiceCommand
        {
            get { return _installServiceCommand; }
        }
        
        private void InstallServiceCommandExecuted(object obj)
        {
            var w = new InstallService
            {
                ProjectConnectionString = ConnectionParameters == null ? null : ConnectionParameters.ConnectionString,
                Owner = View as Window
            };
            w.ShowDialog();
        }

        #endregion

        #region UninstallServiceCommand

        public ICommand UninstallServiceCommand
        {
            get { return _uninstallServiceCommand; }
        }

        private void UninstallServiceCommandExecuted(object obj)
        {
            try
            {
                var proc = new ProcessStartInfo
                {
                    FileName = string.Format("{0}.exe", PHmiConstants.PHmiServiceName),
                    Arguments = "--u "
                };
                Process.Start(proc);
            }
            catch (Win32Exception)
            {
            }
            catch (Exception exception)
            {
                _service.DialogHelper.Exception(exception, View);
            }
        }

        #endregion

        public string Title
        {
            get
            {
                return string.Format(
                    "{0}{1}",
                    ConnectionParameters == null ? string.Empty : ConnectionParameters.Database + " - ",
                    PHmiConstants.PHmiConfiguratorName);
            }
        }
        
        #region SelectedModuleViewModel

        public IModuleViewModel SelectedModuleViewModel
        {
            get { return _selectedModuleViewModel; }
            set
            {
                _selectedModuleViewModel = value;
                OnPropertyChanged(this, v => v.SelectedModuleViewModel);
            }
        }

        #region ReloadModuleCommand

        private readonly ICommand _reloadModuleCommand;

        public ICommand ReloadModuleCommand { get { return _reloadModuleCommand; } }

        private void ReloadModuleCommandExecuted(object obj)
        {
            if (SelectedModuleViewModel != null && SelectedModuleViewModel.ReloadCommand.CanExecute(null))
                SelectedModuleViewModel.ReloadCommand.Execute(null);
        }

        #endregion

        #region SaveModuleCommand

        private readonly ICommand _saveModuleCommand;

        public ICommand SaveModuleCommand { get { return _saveModuleCommand; } }

        private void SaveModuleCommandExecuted(object obj)
        {
            if (SelectedModuleViewModel != null && SelectedModuleViewModel.SaveCommand.CanExecute(null))
                SelectedModuleViewModel.SaveCommand.Execute(null);
        }

        #endregion

        #region CloseModuleCommand

        private readonly ICommand _closeModuleCommand;

        public ICommand CloseModuleCommand { get { return _closeModuleCommand; } }

        private void CloseModuleCommandExecuted(object obj)
        {
            if (SelectedModuleViewModel != null && SelectedModuleViewModel.CloseCommand.CanExecute(null))
                SelectedModuleViewModel.CloseCommand.Execute(null);
        }

        #endregion

        #region AddModuleCommand

        private readonly ICommand _addModuleCommand;

        public ICommand AddModuleCommand { get { return _addModuleCommand; } }

        private void AddModuleCommandExecuted(object obj)
        {
            var collectionViewModel = SelectedModuleViewModel as ICollectionViewModel;
            if (collectionViewModel != null && collectionViewModel.AddCommand.CanExecute(null))
                collectionViewModel.AddCommand.Execute(null);
        }

        #endregion

        #region EditModuleCommand

        private readonly ICommand _editModuleCommand;

        public ICommand EditModuleCommand { get { return _editModuleCommand; } }

        private void EditModuleCommandExecuted(object obj)
        {
            var collectionViewModel = SelectedModuleViewModel as ICollectionViewModel;
            if (collectionViewModel != null && collectionViewModel.EditCommand.CanExecute(null))
                collectionViewModel.EditCommand.Execute(null);
        }

        #endregion

        #region DeleteModuleCommand

        private readonly ICommand _deleteModuleCommand;

        public ICommand DeleteModuleCommand { get { return _deleteModuleCommand; } }

        private void DeleteModuleCommandExecuted(object obj)
        {
            var collectionViewModel = SelectedModuleViewModel as ICollectionViewModel;
            if (collectionViewModel != null && collectionViewModel.DeleteCommand.CanExecute(null))
                collectionViewModel.DeleteCommand.Execute(null);
        }

        #endregion

        #region CopyModuleCommand

        private readonly ICommand _copyModuleCommand;

        public ICommand CopyModuleCommand { get { return _copyModuleCommand; } }

        private void CopyModuleCommandExecuted(object obj)
        {
            var collectionViewModel = SelectedModuleViewModel as ICollectionViewModel;
            if (collectionViewModel != null && collectionViewModel.CopyCommand.CanExecute(null))
                collectionViewModel.CopyCommand.Execute(null);
        }

        #endregion

        #region PasteModuleCommand

        private readonly ICommand _pasteModuleCommand;

        public ICommand PasteModuleCommand { get { return _pasteModuleCommand; } }

        private void PasteModuleCommandExecuted(object obj)
        {
            var collectionViewModel = SelectedModuleViewModel as ICollectionViewModel;
            if (collectionViewModel != null && collectionViewModel.PasteCommand.CanExecute(null))
                collectionViewModel.PasteCommand.Execute(null);
        }

        #endregion

        #region UnselectModuleCommand

        private readonly ICommand _unselectModuleCommand;

        public ICommand UnselectModuleCommand { get { return _unselectModuleCommand; } }

        private void UnselectModuleCommandExecuted(object obj)
        {
            var collectionViewModel = SelectedModuleViewModel as ICollectionViewModel;
            if (collectionViewModel != null && collectionViewModel.UnselectCommand.CanExecute(null))
                collectionViewModel.UnselectCommand.Execute(null);
        }

        #endregion

        #endregion
    }
}
