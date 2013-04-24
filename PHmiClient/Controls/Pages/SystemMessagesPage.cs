using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using PHmiClient.Controls.Input;
using PHmiClient.Loc;
using PHmiClient.PHmiSystem;
using PHmiClient.Utils.Notifications;
using PHmiClient.Utils.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace PHmiClient.Controls.Pages
{
    public class SystemMessagesPage : PaginatorControl, IPage
    {
        private readonly IPaginator<Notification, DateTime> _paginator;
        private PHmiAbstract _pHmi;

        private class PaginationService : IPaginationService<Notification, DateTime>
        {
            private readonly PHmiAbstract _pHmi;

            public PaginationService(PHmiAbstract pHmi)
            {
                _pHmi = pHmi;
            }

            public void GetItems(CriteriaType criteriaType, int maxCount, DateTime criteria, Action<IEnumerable<Notification>> callback)
            {
                IEnumerable<Notification> items;
                switch (criteriaType)
                {
                    case CriteriaType.UpFromInfinity:
                        items = _pHmi.Reporter.Notifications.Take(maxCount).Reverse();
                        break;
                    case CriteriaType.UpFrom:
                        items = _pHmi.Reporter.Notifications.SkipWhile(n => n.StartTime <= criteria).Take(maxCount).Reverse();
                        break;
                    case CriteriaType.UpFromOrEqual:
                        items = _pHmi.Reporter.Notifications.SkipWhile(n => n.StartTime < criteria).Take(maxCount).Reverse();
                        break;
                    case CriteriaType.DownFromOrEqual:
                        items = _pHmi.Reporter.Notifications.Reverse().SkipWhile(n => n.StartTime > criteria).Take(maxCount);
                        break;
                    case CriteriaType.DownFrom:
                        items = _pHmi.Reporter.Notifications.Reverse().SkipWhile(n => n.StartTime >= criteria).Take(maxCount);
                        break;
                    case CriteriaType.DownFromInfinity:
                        items = _pHmi.Reporter.Notifications.Reverse().Take(maxCount);
                        break;
                    default:
                        throw new NotSupportedException("CriteriaType " + criteriaType);
                }
                callback(items);
            }
        }

        static SystemMessagesPage()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SystemMessagesPage), new FrameworkPropertyMetadata(typeof(SystemMessagesPage)));
        }

        public SystemMessagesPage()
        {
            _paginator = new Paginator<Notification, DateTime>(GetCriteria, (item, newItem) => {});
            SetPaginator(_paginator);
        }

        public object PageName { get { return Res.SystemMessages; } }

        #region PHmi
        
        public PHmiAbstract PHmi
        {
            get { return (PHmiAbstract)GetValue(PHmiProperty); }
            set { SetValue(PHmiProperty, value); }
        }

        public static readonly DependencyProperty PHmiProperty =
            DependencyProperty.Register("PHmi", typeof(PHmiAbstract), typeof(SystemMessagesPage),
            new PropertyMetadata(PHmiPropertyChanged));

        private static void PHmiPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var page = (SystemMessagesPage) d;
            page.OnPHmiChanged(e.NewValue as PHmiAbstract);
        }

        private void OnPHmiChanged(PHmiAbstract newValue)
        {
            if (_pHmi != null)
            {
                var collection = (INotifyCollectionChanged)_pHmi.Reporter.Notifications;
                collection.CollectionChanged -= NotificationsCollectionChanged;
            }
            _pHmi = newValue;
            if (_pHmi == null)
            {
                _paginator.PaginationService = null;
            }
            else
            {
                _paginator.PaginationService = new PaginationService(_pHmi);
                var collection = (INotifyCollectionChanged)_pHmi.Reporter.Notifications;
                collection.CollectionChanged += NotificationsCollectionChanged;
                NotificationsCollectionChanged(null, null);
            }
        }

        #endregion

        private void NotificationsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_paginator.RefreshCommand.CanExecute(null))
            {
                _paginator.RefreshCommand.Execute(null);
            }
        }

        private static DateTime GetCriteria(Notification notification)
        {
            return notification.StartTime;
        }

        public IPaginator<Notification, DateTime> Paginator
        {
            get { return _paginator; }
        }

        public IRoot Root { get; set; }

        #region SelectedAlarms

        private ObservableCollection<Notification> _selectedNotifications;

        public ObservableCollection<Notification> SelectedNotifications
        {
            get
            {
                if (_selectedNotifications == null)
                {
                    _selectedNotifications = new ObservableCollection<Notification>();
                    _selectedNotifications.CollectionChanged += OnSelectedNotificationsChanged;
                }
                return _selectedNotifications;
            }
        }

        private void OnSelectedNotificationsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_resetSelectedCommand != null)
            {
                _resetSelectedCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region ResetSelectedCommand

        private DelegateCommand _resetSelectedCommand;

        public ICommand ResetSelectedCommand
        {
            get
            {
                return _resetSelectedCommand
                    ?? (_resetSelectedCommand = new DelegateCommand(ResetSelectedCommandExecuted, ResetSelectedCommandCanExecute));
            }
        }

        private bool ResetSelectedCommandCanExecute(object obj)
        {
            return SelectedNotifications.Any();
        }

        private void ResetSelectedCommandExecuted(object obj)
        {
            foreach (var n in SelectedNotifications.ToArray())
            {
                _pHmi.Reporter.Reset(n);
            }
        }

        #endregion

        #region ResetCommand

        private DelegateCommand _resetCommand;

        public ICommand ResetCommand
        {
            get
            {
                if (_resetCommand == null)
                {
                    _resetCommand = new DelegateCommand(ResetCommandExecuted, ResetCommandCanExecute);
                    ((INotifyCollectionChanged) _paginator.Items).CollectionChanged += OnPaginatorCollectionChanged;
                }
                return _resetCommand;
            }
        }
        
        private void OnPaginatorCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _resetCommand.RaiseCanExecuteChanged();
        }

        private bool ResetCommandCanExecute(object obj)
        {
            return _paginator.Items.Any();
        }

        private void ResetCommandExecuted(object obj)
        {
            foreach (var n in _paginator.Items.ToArray())
            {
                _pHmi.Reporter.Reset(n);
            }
        }

        #endregion

        #region IsTimePopupOpen

        public bool IsTimePopupOpen
        {
            get { return (bool)GetValue(IsTimePopupOpenProperty); }
            set { SetValue(IsTimePopupOpenProperty, value); }
        }

        public static readonly DependencyProperty IsTimePopupOpenProperty =
            DependencyProperty.Register("IsTimePopupOpen", typeof(bool), typeof(SystemMessagesPage),
            new PropertyMetadata(IsTimePopupOpenPropertyChanged));

        private static void IsTimePopupOpenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
                return;
            var p = (SystemMessagesPage) d;
            p.SetTime = p._paginator.Criteria;
            p.SetCriteriaType = p._paginator.CriteriaType;
        }

        #endregion

        #region SetTime
        
        public DateTime SetTime
        {
            get { return (DateTime)GetValue(SetTimeProperty); }
            set { SetValue(SetTimeProperty, value); }
        }

        public static readonly DependencyProperty SetTimeProperty =
            DependencyProperty.Register("SetTime", typeof(DateTime), typeof(SystemMessagesPage));

        #endregion

        #region SetCriteriaType
        
        public CriteriaType SetCriteriaType
        {
            get { return (CriteriaType)GetValue(SetCriteriaTypeProperty); }
            set { SetValue(SetCriteriaTypeProperty, value); }
        }

        public static readonly DependencyProperty SetCriteriaTypeProperty =
            DependencyProperty.Register("SetCriteriaType", typeof(CriteriaType), typeof(SystemMessagesPage));
        
        #endregion

        #region SetTimeCommand

        private DelegateCommand _setTimeCommand;

        public ICommand SetTimeCommand
        {
            get { return _setTimeCommand ?? (_setTimeCommand = new DelegateCommand(SetTimeCommandExecuted)); }
        }

        private void SetTimeCommandExecuted(object obj)
        {
            IsTimePopupOpen = false;
            _paginator.Refresh(SetCriteriaType, SetTime);
        }

        #endregion
    }
}
