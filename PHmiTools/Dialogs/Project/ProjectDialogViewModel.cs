using System;
using System.ComponentModel;
using System.Windows.Input;
using PHmiClient.Controls.Input;
using PHmiClient.Utils;
using PHmiClient.Utils.ViewInterfaces;
using PHmiResources.Loc;
using PHmiTools.Utils.Npg;
using PHmiTools.ViewModels;

namespace PHmiTools.Dialogs.Project
{
    public class ProjectDialogViewModel : ViewModelBase<IWindow>
    {
        public ProjectDialogViewModel(IProjectDialogService service)
        {
            _service = service ?? new ProjectDialogService();
            var configString = _service.ConnectionStringHelper.Get(PHmiConstants.PHmiConnectionStringName);
            if (configString == null)
            {
                _service.ConnectionParameters.Server = "localhost";
                _service.ConnectionParameters.Port = "5432";
                _service.ConnectionParameters.UserId = "postgres";
            }
            else
            {
                _service.ConnectionParameters.Update(configString);
            }
            _service.ConnectionParameters.PropertyChanged += ConnectionParametersPropertyChanged;
            _remember = configString != null;
            _okCommand = new DelegateCommand(OkCommandExecuted, OkCommandCanExecute);
            _cancelCommand = new DelegateCommand(CancelCommandExecuted);
        }

        private readonly IProjectDialogService _service;
        private readonly DelegateCommand _okCommand;
        private readonly ICommand _cancelCommand;
        private bool _remember;
        private bool _inProgress;
        private bool _progressIsIndeterminate;
        private int _progressMax;
        private int _progress;
        private bool _closed;

        private void ConnectionParametersPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaiseOkCommandCanExecuteChanged();
        }
        
        #region OkCommand

        public ICommand OkCommand { get { return _okCommand; } }

        protected virtual void OkCommandExecuted(object obj)
        {
            if (_closed)
                return;
            _closed = true;
            _service.ConnectionStringHelper.Set(PHmiConstants.PHmiConnectionStringName, Remember ? ConnectionParameters.ConnectionString : null);
            try
            {
                View.DialogResult = true;
            }
            catch (InvalidOperationException)
            {
            }
        }

        protected virtual bool OkCommandCanExecute(object obj)
        {
            return string.IsNullOrEmpty(ConnectionParameters.Error) && !_inProgress;
        }

        protected void RaiseOkCommandCanExecuteChanged()
        {
            _okCommand.RaiseCanExecuteChanged();
        }

        #endregion

        #region CancelCommand

        public ICommand CancelCommand { get { return _cancelCommand; } }

        protected virtual void CancelCommandExecuted(object obj)
        {
            if (_closed)
                return;
            _closed = true;
            if (!Remember)
                _service.ConnectionStringHelper.Set(PHmiConstants.PHmiConnectionStringName, null);
            View.DialogResult = false;
        }

        #endregion

        public INpgConnectionParameters ConnectionParameters
        {
            get { return _service.ConnectionParameters; }
        }

        [LocDisplayName("Remember", ResourceType = typeof(Res))]
        public bool Remember
        {
            get { return _remember; }
            set
            {
                _remember = value;
                OnPropertyChanged(this, d => d.Remember);
            }
        }

        #region Progress

        public bool InProgress
        {
            get { return _inProgress; }
            protected set
            {
                _inProgress = value;
                OnPropertyChanged(this, d => d.InProgress);
                RaiseOkCommandCanExecuteChanged();
            }
        }

        public int ProgressMax
        {
            get { return _progressMax; }
            protected set
            {
                _progressMax = value;
                OnPropertyChanged(this, d => d.ProgressMax);
            }
        }

        public int Progress
        {
            get { return _progress; }
            protected set
            {
                _progress = value;
                OnPropertyChanged(this, d => d.Progress);
            }
        }

        public bool ProgressIsIndeterminate
        {
            get { return _progressIsIndeterminate; }
            protected set
            {
                _progressIsIndeterminate = value;
                OnPropertyChanged(this, d => d.ProgressIsIndeterminate);
            }
        }

        #endregion
    }
}
