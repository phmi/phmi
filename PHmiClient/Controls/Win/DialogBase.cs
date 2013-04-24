using System;
using PHmiClient.Controls.Input;
using System.Windows;
using System.Windows.Input;

namespace PHmiClient.Controls.Win
{
    public abstract class DialogBase : Window
    {
        private readonly DelegateCommand _okCommand;
        private readonly DelegateCommand _cancelCommand;

        protected DialogBase()
        {
            ShowInTaskbar = false;
            _okCommand = new DelegateCommand(OkCommandExecuted, OkCommandCanExecute);
            _cancelCommand = new DelegateCommand(CancelCommandExecuted);
            InputBindings.Add(new KeyBinding(OkCommand, Key.Enter, ModifierKeys.None));
            InputBindings.Add(new KeyBinding(CancelCommand, Key.Escape, ModifierKeys.None));
        }
        
        #region Busy

        public bool Busy
        {
            get { return (bool)GetValue(BusyProperty); }
            set { SetValue(BusyProperty, value); }
        }

        public static readonly DependencyProperty BusyProperty =
            DependencyProperty.Register("Busy", typeof(bool), typeof(DialogBase));

        #endregion

        #region Failed

        public bool Failed
        {
            get { return (bool)GetValue(FailedProperty); }
            set { SetValue(FailedProperty, value); }
        }

        public static readonly DependencyProperty FailedProperty =
            DependencyProperty.Register("Failed", typeof(bool), typeof(DialogBase));

        #endregion

        #region OkCommand

        public ICommand OkCommand { get { return _okCommand; } }

        protected virtual void OkCommandExecuted(object obj)
        {
            DialogResult = true;
        }

        protected virtual bool OkCommandCanExecute(object obj)
        {
            return true;
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
            DialogResult = false;
        }

        #endregion

        protected void StartLoading()
        {
            Busy = true;
            Failed = false;
        }

        protected void EndLoading(bool result)
        {
            Busy = false;
            if (result)
            {
                try
                {
                    DialogResult = true;
                }
                catch (InvalidOperationException)
                {
                }
            }
            else
            {
                Failed = true;
            }
        }
    }
}
