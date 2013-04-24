using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using PHmiClient.Alarms;
using PHmiClient.Controls.Input;
using PHmiClient.Loc;
using PHmiClient.Utils.Pagination;
using System;
using System.Collections.Generic;
using System.Windows;

namespace PHmiClient.Controls.Pages
{
    public class AlarmsPage : PaginatorControl, IPage
    {
        private readonly IPaginator<Alarm, AlarmSampleId> _paginator;

        private class PaginationService : IPaginationService<Alarm, AlarmSampleId>
        {
            private readonly AlarmCategoryAbstract _alarmCategory;

            public PaginationService(AlarmCategoryAbstract alarmCategory)
            {
                _alarmCategory = alarmCategory;
            }

            public void GetItems(CriteriaType criteriaType, int maxCount, AlarmSampleId criteria, Action<IEnumerable<Alarm>> callback)
            {
                _alarmCategory.GetCurrent(criteriaType, criteria, maxCount, callback);
            }
        }

        static AlarmsPage()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AlarmsPage), new FrameworkPropertyMetadata(typeof(AlarmsPage)));
        }

        public AlarmsPage()
        {
            _paginator = new Paginator<Alarm, AlarmSampleId>(
                alarm => new AlarmSampleId(alarm), (item, newItem) => item.UpdateFrom(newItem));
            SetPaginator(_paginator);
            AutoRefresh = true;
        }

        public virtual object PageName { get { return Res.Alarms; } }

        #region AlarmCategory
        
        public AlarmCategoryAbstract AlarmCategory
        {
            get { return (AlarmCategoryAbstract)GetValue(AlarmCategoryProperty); }
            set { SetValue(AlarmCategoryProperty, value); }
        }

        public static readonly DependencyProperty AlarmCategoryProperty =
            DependencyProperty.Register("AlarmCategory", typeof(AlarmCategoryAbstract), typeof(AlarmsPage),
            new PropertyMetadata(AlarmCategoryPropertyChanged));

        private static void AlarmCategoryPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var p = (AlarmsPage) d;
            var alarmCategory = e.NewValue as AlarmCategoryBase;
            p._paginator.PaginationService = alarmCategory != null ? p.CreatePaginationService(alarmCategory) : null;
        }

        #endregion

        protected virtual IPaginationService<Alarm, AlarmSampleId> CreatePaginationService(
            AlarmCategoryAbstract alarmCategory)
        {
            return new PaginationService(alarmCategory);
        }

        public IPaginator<Alarm, AlarmSampleId> Paginator
        {
            get { return _paginator; }
        }

        public IRoot Root { get; set; }

        #region SelectedAlarms

        private ObservableCollection<Alarm> _selectedAlarms;

        public ObservableCollection<Alarm> SelectedAlarms
        {
            get
            {
                if (_selectedAlarms == null)
                {
                    _selectedAlarms = new ObservableCollection<Alarm>();
                    _selectedAlarms.CollectionChanged += OnSelectedAlarmsChanged;
                }
                return _selectedAlarms;
            }
        }

        private void OnSelectedAlarmsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_acknowledgeSelectedCommand != null)
            {
                _acknowledgeSelectedCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region AcknowledgeSelectedCommand

        private DelegateCommand _acknowledgeSelectedCommand;

        public ICommand AcknowledgeSelectedCommand
        {
            get
            {
                return _acknowledgeSelectedCommand
                    ?? (_acknowledgeSelectedCommand = new DelegateCommand(AcknowledgeSelectedCommandExecuted, AcknowledgeSelectedCommandCanExecute));
            }
        }

        private bool AcknowledgeSelectedCommandCanExecute(object obj)
        {
            return SelectedAlarms.Any();
        }

        private void AcknowledgeSelectedCommandExecuted(object obj)
        {
            AlarmCategory.Acknowledge(SelectedAlarms.Where(a => !a.AcknowledgeTime.HasValue).ToArray());
        }

        #endregion

        #region AcknowledgeCommand

        private DelegateCommand _acknowledgeCommand;

        public ICommand AcknowledgeCommand
        {
            get
            {
                if (_acknowledgeCommand == null)
                {
                    _acknowledgeCommand = new DelegateCommand(AcknowledgeCommandExecuted, AcknowledgeCommandCanExecute);
                    ((INotifyCollectionChanged)_paginator.Items).CollectionChanged += OnPaginatorCollectionChanged;
                }
                return _acknowledgeCommand;
            }
        }

        private void OnPaginatorCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _acknowledgeCommand.RaiseCanExecuteChanged();
        }

        private bool AcknowledgeCommandCanExecute(object obj)
        {
            return _paginator.Items.Any();
        }

        private void AcknowledgeCommandExecuted(object obj)
        {
            AlarmCategory.Acknowledge(_paginator.Items.Where(a => !a.AcknowledgeTime.HasValue).ToArray());
        }

        #endregion

        #region IsTimePopupOpen

        public bool IsTimePopupOpen
        {
            get { return (bool)GetValue(IsTimePopupOpenProperty); }
            set { SetValue(IsTimePopupOpenProperty, value); }
        }

        public static readonly DependencyProperty IsTimePopupOpenProperty =
            DependencyProperty.Register("IsTimePopupOpen", typeof(bool), typeof(AlarmsPage),
            new PropertyMetadata(IsTimePopupOpenPropertyChanged));

        private static void IsTimePopupOpenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
                return;
            var p = (AlarmsPage)d;
            if (p._paginator.Criteria != null)
            {
                p.SetTime = p._paginator.Criteria.StartTime;
            }
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
            DependencyProperty.Register("SetTime", typeof(DateTime), typeof(AlarmsPage));

        #endregion

        #region SetCriteriaType

        public CriteriaType SetCriteriaType
        {
            get { return (CriteriaType)GetValue(SetCriteriaTypeProperty); }
            set { SetValue(SetCriteriaTypeProperty, value); }
        }

        public static readonly DependencyProperty SetCriteriaTypeProperty =
            DependencyProperty.Register("SetCriteriaType", typeof(CriteriaType), typeof(AlarmsPage));

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
            var id = _paginator.Criteria == null ? 0 : _paginator.Criteria.AlarmId;
            _paginator.Refresh(SetCriteriaType, new AlarmSampleId(SetTime, id));
        }

        #endregion
    }
}
