using PHmiClient.Controls.Input;
using PHmiClient.Controls.Pages;
using PHmiClient.PHmiSystem;
using PHmiClient.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PHmiClient.Controls
{
    public class Root : ContentControl, IRoot
    {
        static Root()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Root), new FrameworkPropertyMetadata(typeof(Root)));
        }

        private Type _currentType;

        public Root()
        {
            _backCommand = new DelegateCommand(BackCommandExecuted, BackCommandCanExecute);
            _forwardCommand = new DelegateCommand(ForwardCommandExecuted, ForwardCommandCanExecute);
            Loaded += RootLoaded;
            Unloaded += RootUnloaded;
        }

        private PHmiAbstract PHmi
        {
            get
            {
                var phmi = DataContext as PHmiAbstract;
                return phmi;
            }
        }

        private void RootLoaded(object sender, RoutedEventArgs e)
        {
            if (InDesignModeHelper.IsInDesignMode)
                return;
            if (PHmi != null)
                PHmi.Start();
        }

        private void RootUnloaded(object sender, RoutedEventArgs e)
        {
            var phmi = DataContext as PHmiAbstract;
            if (phmi != null)
                phmi.Stop();
        }

        public void Show(object objOrType)
        {
            var type = objOrType as Type;
            if (type == null)
            {
                var page = objOrType as IPage;
                if (page != null)
                {
                    page.Root = this;
                }
                Content = objOrType;
                return;
            }
            Show(type);
        }

        public void Show<T>()
        {
            Show(typeof(T));
        }

        private void Show(Type type)
        {
            if (_currentType != null && _currentType != type)
            {
                _backStack.Push(_currentType);
            }
            if (_currentType != type)
            {
                _forwardStack.Clear();
            }
            if (type.Equals(HomePage))
            {
                _backStack.Clear();
            }
            var instance = Activator.CreateInstance(type);
            Show(instance);
            _currentType = type;
            _backCommand.RaiseCanExecuteChanged();
            _forwardCommand.RaiseCanExecuteChanged();
        }

        #region ShowCommand

        private ICommand _showCommand;

        public ICommand ShowCommand
        {
            get { return _showCommand ?? (_showCommand = new DelegateCommand(ShowCommandExecuted)); }
        }

        private void ShowCommandExecuted(object obj)
        {
            Show(obj);
        }

        #endregion

        #region Home

        public object HomePage
        {
            get { return GetValue(HomePageProperty); }
            set { SetValue(HomePageProperty, value); }
        }

        public static readonly DependencyProperty HomePageProperty =
            DependencyProperty.Register("HomePage", typeof(object), typeof(Root), new PropertyMetadata(HomePagePropertyChanged));

        private static void HomePagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var root = (Root)d;
            if (root.Content == null)
            {
                root.Show(e.NewValue);
            }
            if (root._homeCommand != null)
            {
                root._homeCommand.RaiseCanExecuteChanged();
            }
        }

        private DelegateCommand _homeCommand;
        
        public ICommand HomeCommand
        {
            get { return _homeCommand ?? (_homeCommand = new DelegateCommand(HomeCommandExecuted, HomeCommandCanExecute)); }
        }

        private bool HomeCommandCanExecute(object obj)
        {
            return HomePage != null;
        }

        private void HomeCommandExecuted(object obj)
        {
            Show(HomePage);
        }

        #endregion

        #region Navigation

        private readonly Stack<Type> _backStack = new Stack<Type>();  

        private readonly DelegateCommand _backCommand;

        public ICommand BackCommand
        {
            get { return _backCommand; }
        }

        private bool BackCommandCanExecute(object obj)
        {
            return _backStack.Any();
        }

        private void BackCommandExecuted(object obj)
        {
            _forwardStack.Push(_currentType);
            _currentType = _backStack.Pop();
            Show(_currentType);
        }

        private readonly Stack<Type> _forwardStack = new Stack<Type>(); 

        private readonly DelegateCommand _forwardCommand;

        public ICommand ForwardCommand
        {
            get
            {
                return _forwardCommand;
            }
        }

        private bool ForwardCommandCanExecute(object obj)
        {
            return _forwardStack.Any();
        }

        private void ForwardCommandExecuted(object obj)
        {
            _backStack.Push(_currentType);
            _currentType = _forwardStack.Pop();
            Show(_currentType);
        }

        #endregion

        #region LogOn

        private ICommand _logOnCommand;

        public ICommand LogOnCommand
        {
            get { return _logOnCommand ?? (_logOnCommand = new DelegateCommand(LogOnCommandExecuted)); }
        }

        private void LogOnCommandExecuted(object obj)
        {
            if (PHmi == null)
                return;
            var d = new Win.LogOnDialog(PHmi);
            d.ShowDialog(this);
        }

        #endregion

        #region LogOff

        private ICommand _logOffCommand;

        public ICommand LogOffCommand
        {
            get { return _logOffCommand ?? (_logOffCommand = new DelegateCommand(LogOffCommandExecuted)); }
        }
        
        private void LogOffCommandExecuted(object obj)
        {
            if (PHmi == null)
                return;
            PHmi.Users.LogOff();
        }

        #endregion

        #region ChangePassword

        private ICommand _changePasswordCommand;

        public ICommand ChangePasswordCommand
        {
            get
            {
                return _changePasswordCommand ??
                   (_changePasswordCommand = new DelegateCommand(ChangePasswordCommandExecuted));
            }
        }

        private void ChangePasswordCommandExecuted(object obj)
        {
            if (PHmi == null)
                return;
            var d = new Win.ChangePasswordDialog(PHmi);
            d.ShowDialog(this);
        }

        #endregion
    }
}
