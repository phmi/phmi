using System;
using System.Windows;
using System.Windows.Input;
using PHmiClient.Controls.Input;
using PHmiClient.Utils;
using PHmiClient.Utils.Notifications;
using PHmiClient.Utils.ViewInterfaces;
using PHmiModel;
using PHmiRunner.Utils;
using PHmiTools;
using PHmiTools.Dialogs;
using PHmiTools.Dialogs.Project;
using PHmiTools.Utils.Npg;
using PHmiTools.ViewModels;

namespace PHmiRunner
{
    public class MainWindowViewModel : ViewModelBase<IWindow>
    {
        private INpgConnectionParameters _connectionParameters;
        private readonly IActionHelper _actionHelper;
        private readonly IProjectRunnerFactory _runnerFactory;
        private readonly INotificationReporter _reporter;
        private IProjectRunner _runner;
        private bool _busy;
        private ICommand _exitCommand;
        private readonly DelegateCommand _closeCommand;
        private ICommand _openCommand;
        private ICommand _importCommand;
        private ICommand _aboutCommand;

        internal MainWindowViewModel(
            IActionHelper actionHelper,
            ITimeService timeService,
            INotificationReporter reporter,
            IProjectRunnerFactory runnerFactory)
        {
            _actionHelper = actionHelper;
            _reporter = reporter ?? new NotificationReporter(timeService);
            _reporter.ExpirationTime = TimeSpan.FromSeconds(1);
            _runnerFactory = runnerFactory ??
                new ProjectRunnerFactory(timeService, _reporter, new PHmiModelContextFactory(), new NpgHelper());
            _closeCommand = new DelegateCommand(CloseCommandExecuted, CloseCommandCanExecute);
        }

        public MainWindowViewModel() : this(new ActionHelper(), new TimeService(), null, null) { }

        public INpgConnectionParameters ConnectionParameters
        {
            get { return _connectionParameters; }
            set
            {
                _connectionParameters = value;
                RestartRunner();
                OnPropertyChanged(this, v => v.ConnectionParameters);
                OnPropertyChanged(this, v => v.Title);
                _closeCommand.RaiseCanExecuteChanged();
            }
        }

        public INotificationReporter Reporter { get { return _reporter; } }

        public string Title
        {
            get
            {
                return string.Format("{0}{1}",
                    ConnectionParameters != null ? ConnectionParameters.Database + " - " : string.Empty,
                    PHmiConstants.PHmiRunnerName);
            }
        }

        public bool Busy
        {
            get { return _busy; }
            set
            {
                _busy = value;
                OnPropertyChanged(this, v => v.Busy);
            }
        }

        private void RestartRunner()
        {
            if (_runner == null)
            {
                if (_connectionParameters != null)
                {
                    _runner = _runnerFactory.Create(_connectionParameters.Database, _connectionParameters.ConnectionString);
                    Busy = true;
                    _actionHelper.Async(StartRunner);
                }
            }
            else
            {
                Busy = true;
                _actionHelper.Async(StopRunner);
            }
        }

        private void StartRunner()
        {
            _runner.Start();
            Busy = false;
        }

        private void StopRunner()
        {
            _runner.Stop();
            Busy = false;
            _runner = null;
            RestartRunner();
        }

        public ICommand ExitCommand
        {
            get { return _exitCommand ?? (_exitCommand = new DelegateCommand(ExitCommandExecuted)); }
        }

        private void ExitCommandExecuted(object parameter)
        {
            var view = View;
            if (view != null)
                view.Close();
        }

        public ICommand CloseCommand
        {
            get { return _closeCommand; }
        }

        private void CloseCommandExecuted(object parameter)
        {
            ConnectionParameters = null;
        }

        private bool CloseCommandCanExecute(object obj)
        {
            return ConnectionParameters != null;
        }

        public ICommand OpenCommand
        {
            get { return _openCommand ?? (_openCommand = new DelegateCommand(OpenCommandExecuted)); }
        }

        private void OpenCommandExecuted(object parameter)
        {
            var dialog = new OpenProjectDialog();
            if (dialog.ShowDialog() == true)
            {
                ConnectionParameters = dialog.ViewModel.ConnectionParameters;
            }
        }

        public ICommand ImportCommand
        {
            get { return _importCommand ?? (_importCommand = new DelegateCommand(ImportCommandExecuted)); }
        }

        private void ImportCommandExecuted(object obj)
        {
            var dialog = new ImportProjectDialog();
            if (dialog.ShowDialog() == true)
            {
                ConnectionParameters = dialog.ViewModel.ConnectionParameters;
            }
        }

        public ICommand AboutCommand
        {
            get { return _aboutCommand ?? (_aboutCommand = new DelegateCommand(AboutCommandExecuted)); }
        }

        private void AboutCommandExecuted(object obj)
        {
            var w = new AboutDialog { Owner = View as Window };
            w.ShowDialog();
        }
    }
}
