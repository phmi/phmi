using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using PHmiClient.Controls.Input;
using PHmiClient.Controls.Win;
using PHmiClient.Loc;
using PHmiClient.PHmiSystem;
using PHmiClient.Users;
using PHmiClient.Utils;
using PHmiClient.Utils.Pagination;

namespace PHmiClient.Controls.Pages
{
    public class UsersPage : PaginatorControl, IPage
    {
        private readonly IPaginator<User, string> _paginator;
        private readonly DelegateCommand _setPasswordCommand;
        private readonly DelegateCommand _editUserCommand;
        private readonly DelegateCommand _addUserCommand;

        private class PaginationService : IPaginationService<User, string>
        {
            private readonly IUsers _users;

            public PaginationService(IUsers users)
            {
                _users = users;
            }

            public void GetItems(CriteriaType criteriaType, int maxCount, string criteria, Action<IEnumerable<User>> callback)
            {
                _users.GetUsers(criteriaType, criteria, maxCount, callback);
            }
        }

        public IRoot Root { get; set; }

        public object PageName { get { return Res.Users; } }

        static UsersPage()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(UsersPage), new FrameworkPropertyMetadata(typeof(UsersPage)));
        }

        public UsersPage()
        {
            _paginator = new Paginator<User, string>(
                user => user.Name, (user, newUser) => user.UpdateFrom(newUser));
            SetPaginator(_paginator);
            _setPasswordCommand = new DelegateCommand(SetPasswordCommandExecuted, SetPasswordCommandCanExecute);
            _editUserCommand = new DelegateCommand(EditUserCommandExecuted, EditUserCommandCanExecute);
            _addUserCommand = new DelegateCommand(AddUserCommandExecuted);
        }

        public IPaginator<User, string> Paginator
        {
            get { return _paginator; }
        }

        #region PHmi

        public PHmiAbstract PHmi
        {
            get { return (PHmiAbstract)GetValue(PHmiProperty); }
            set { SetValue(PHmiProperty, value); }
        }

        public static readonly DependencyProperty PHmiProperty =
            DependencyProperty.Register("PHmi", typeof(PHmiAbstract), typeof(UsersPage),
            new PropertyMetadata(PHmiPropertyChanged));

        private static void PHmiPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var page = (UsersPage) d;
            var pHmi = e.NewValue as PHmiAbstract;
            page._paginator.PaginationService = pHmi == null ? null : new PaginationService(pHmi.Users);
        }

        #endregion

        #region SelectedUser
        
        public User SelectedUser
        {
            get { return (User)GetValue(SelectedUserProperty); }
            set { SetValue(SelectedUserProperty, value); }
        }

        public static readonly DependencyProperty SelectedUserProperty =
            DependencyProperty.Register("SelectedUser", typeof(User), typeof(UsersPage),
            new PropertyMetadata(SelectedUserPropertyChanged));

        private static void SelectedUserPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var p = (UsersPage)d;
            p._editUserCommand.RaiseCanExecuteChanged();
            p._setPasswordCommand.RaiseCanExecuteChanged();
        }

        #endregion

        #region SetPasswordCommand

        public ICommand SetPasswordCommand
        {
            get { return _setPasswordCommand; }
        }

        private bool SetPasswordCommandCanExecute(object obj)
        {
            return SelectedUser != null;
        }

        private void SetPasswordCommandExecuted(object obj)
        {
            var dialog = new SetPasswordDialog(PHmi.Users, SelectedUser);
            if (SetPasswordDialogStyle != null)
            {
                dialog.Style = SetPasswordDialogStyle;
            }
            dialog.ShowDialog(this);
        }

        #region SetPasswordDialogStyle
        
        public Style SetPasswordDialogStyle
        {
            get { return (Style)GetValue(SetPasswordDialogStyleProperty); }
            set { SetValue(SetPasswordDialogStyleProperty, value); }
        }

        public static readonly DependencyProperty SetPasswordDialogStyleProperty =
            DependencyProperty.Register("SetPasswordDialogStyle", typeof(Style), typeof(UsersPage));

        #endregion

        #endregion

        #region EditUserCommand

        public ICommand EditUserCommand
        {
            get { return _editUserCommand; }
        }

        private bool EditUserCommandCanExecute(object obj)
        {
            return SelectedUser != null;
        }

        private void EditUserCommandExecuted(object obj)
        {
            var dialog = new EditUserDialog(PHmi.Users, SelectedUser);
            if (EditUserDialogStyle != null)
            {
                dialog.Style = EditUserDialogStyle;
            }
            var result = dialog.ShowDialog(this);
            if (result == true && Paginator.RefreshCommand.CanExecute(null))
            {
                Paginator.RefreshCommand.Execute(null);
            }
        }

        #region EditUserDialogStyle

        public Style EditUserDialogStyle
        {
            get { return (Style)GetValue(EditUserDialogStyleProperty); }
            set { SetValue(EditUserDialogStyleProperty, value); }
        }

        public static readonly DependencyProperty EditUserDialogStyleProperty =
            DependencyProperty.Register("EditUserDialogStyle", typeof(Style), typeof(UsersPage));

        #endregion

        #endregion

        #region AddUserCommand

        public ICommand AddUserCommand
        {
            get { return _addUserCommand; }
        }

        private void AddUserCommandExecuted(object obj)
        {
            var dialog = new AddUserDialog(PHmi.Users, PHmi.Time.Ticks);
            if (AddUserDialogStyle != null)
            {
                dialog.Style = AddUserDialogStyle;
            }
            var result = dialog.ShowDialog(this);
            if (result == true && Paginator.RefreshCommand.CanExecute(null))
            {
                Paginator.RefreshCommand.Execute(null);
            }
        }

        #region AddUserDialogStyle

        public Style AddUserDialogStyle
        {
            get { return (Style)GetValue(AddUserDialogStyleProperty); }
            set { SetValue(AddUserDialogStyleProperty, value); }
        }

        public static readonly DependencyProperty AddUserDialogStyleProperty =
            DependencyProperty.Register("AddUserDialogStyle", typeof(Style), typeof(UsersPage));
        
        #endregion

        #endregion
    }
}
