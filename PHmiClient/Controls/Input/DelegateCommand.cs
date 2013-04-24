using System;
using System.Windows.Input;
using PHmiClient.Utils;

namespace PHmiClient.Controls.Input
{
    public class DelegateCommand : ICommand
    {
        private readonly Predicate<object> _canExecute;
        private readonly Action<object> _method;
        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action<object> method, Predicate<object> canExecute = null)
        {
            _method = method;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _method.Invoke(parameter);
        }

        protected virtual void OnCanExecuteChanged(EventArgs e)
        {
            EventHelper.Raise(ref CanExecuteChanged, this, e);
        }

        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged(EventArgs.Empty);
        }
    }
}
