using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using PHmiClient.Controls.Input;
using PHmiClient.Utils;
using PHmiClient.Utils.ViewInterfaces;
using PHmiTools;
using PHmiTools.Utils;
using PHmiTools.ViewModels;

namespace PHmiConfigurator.Dialogs
{
    public class InstallServiceViewModel : ViewModelBase<IWindow>
    {
        private readonly IDialogHelper _dialogHelper;
        private readonly IActionHelper _actionHelper;
        private readonly DelegateCommand _cancelCommand;
        private readonly DelegateCommand _installCommand;
        private string _customConnectionString;
        private bool _useCustomConnectionString = true;
        private bool _busy;
        private string _projectConnectionString;

        public InstallServiceViewModel() : this(new DialogHelper(), new ActionHelper())
        {
        }

        public InstallServiceViewModel(IDialogHelper dialogHelper, IActionHelper actionHelper)
        {
            _actionHelper = actionHelper;
            _dialogHelper = dialogHelper;
            _cancelCommand = new DelegateCommand(CancelExecuted);
            _installCommand = new DelegateCommand(InstallExecuted, InstallCanExecute);
        }

        public ICommand CancelCommand
        {
            get { return _cancelCommand; }
        }

        private void CancelExecuted(object obj)
        {
            View.DialogResult = false;
        }

        public string ProjectConnectionString
        {
            get { return _projectConnectionString; }
            set
            {
                _projectConnectionString = value;
                OnPropertyChanged(this, m => m.ProjectConnectionString);
                if (_projectConnectionString != null)
                {
                    UseCustomConnectionString = false;
                }
            }
        }

        public string CustomConnectionString
        {
            get { return _customConnectionString; }
            set
            {
                _customConnectionString = value;
                OnPropertyChanged(this, m => m.CustomConnectionString);
            }
        }

        public bool UseCustomConnectionString
        {
            get { return _useCustomConnectionString; }
            set
            {
                _useCustomConnectionString = value;
                OnPropertyChanged(this, m => m.UseCustomConnectionString);
            }
        }

        public bool Busy
        {
            get { return _busy; }
            set
            {
                _busy = value;
                OnPropertyChanged(this, m => m.Busy);
            }
        }

        public ICommand InstallCommand
        {
            get { return _installCommand; }
        }

        private bool InstallCanExecute(object obj)
        {
            return !Busy;
        }

        private void InstallExecuted(object obj)
        {
            _actionHelper.Async(Install);
        }

        private void Install()
        {
            Busy = true;
            var connectionString = UseCustomConnectionString ? CustomConnectionString : ProjectConnectionString;
            try
            {
                var proc = new ProcessStartInfo
                {
                    FileName = string.Format("{0}.exe", PHmiConstants.PHmiServiceName),
                    Arguments = "--i " + connectionString
                };
                Process.Start(proc);
                _actionHelper.Dispatch(() => { View.DialogResult = true; });
            }
            catch (Win32Exception)
            {
                _actionHelper.Dispatch(() => { View.DialogResult = false; });
            }
            catch (Exception exception)
            {
                _dialogHelper.Exception(exception, View);
            }
            finally
            {
                Busy = false;
            }
        }

        protected override void OnPropertyChanged(string property)
        {
            base.OnPropertyChanged(property);
            if (property == PropertyHelper.GetPropertyName(this, m => m.Busy))
            {
                _actionHelper.Dispatch(() => _installCommand.RaiseCanExecuteChanged());
            }
        }
    }
}
