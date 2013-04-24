using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using PHmiClient.Controls.Input;

namespace PHmiClient.Utils.Pagination
{
    public class Paginator<T, TCriteria> : IPaginator<T, TCriteria>
    {
        public delegate void UpdateOldItemDelegate(T oldItem, T newItem);
        private readonly Func<T, TCriteria> _getCriteriaFunc;
        private readonly UpdateOldItemDelegate _updateAction;
        private readonly IDispatcherService _dispatcher = new DispatcherService();
        private readonly ObservableCollection<T> _items = new ObservableCollection<T>();
        private readonly ReadOnlyObservableCollection<T> _readOnlyItems;
        private readonly DelegateCommand _upUpCommand;
        private readonly DelegateCommand _upCommand;
        private readonly DelegateCommand _downCommand;
        private readonly DelegateCommand _downDownCommand;
        private readonly DelegateCommand _refreshCommand;
        private readonly DelegateCommand _cancelCommand;
        private bool _busy;
        private IPaginationService<T, TCriteria> _paginationService;
        private Guid _refreshGuid;
        private TCriteria _criteria;
        private CriteriaType _criteriaType;
        private int _pageSize;

        public Paginator(Func<T, TCriteria> getCriteriaFunc, UpdateOldItemDelegate updateAction)
        {
            _getCriteriaFunc = getCriteriaFunc;
            _updateAction = updateAction;
            _readOnlyItems = new ReadOnlyObservableCollection<T>(_items);
            _upUpCommand = new DelegateCommand(UpUpCommandExecuted, UpUpCommandCanExecute);
            _upCommand = new DelegateCommand(UpCommandExecuted, UpCommandCanExecute);
            _downCommand = new DelegateCommand(DownCommandExecuted, DownCommandCanExecute);
            _downDownCommand = new DelegateCommand(DownDownCommandExecuted, DownDownCommandCanExecute);
            _refreshCommand = new DelegateCommand(RefreshCommandExecuted, RefreshCommandCanExecute);
            _cancelCommand = new DelegateCommand(CancelCommandExecuted, CancelCommandCanExecute);
        }

        public ReadOnlyObservableCollection<T> Items { get { return _readOnlyItems; } }

        public int PageSize
        {
            get { return _pageSize; }
            set
            {
                _pageSize = value;
                OnPropertyChanged("PageSize");
                UpdatePageSize();
            }
        }

        private void UpdatePageSize()
        {
            switch (CriteriaType)
            {
                case CriteriaType.UpFromInfinity:
                case CriteriaType.UpFrom:
                case CriteriaType.UpFromOrEqual:
                    while (Items.Count > PageSize)
                    {
                        _items.RemoveAt(0);
                    }
                    break;
                case CriteriaType.DownFromInfinity:
                case CriteriaType.DownFrom:
                case CriteriaType.DownFromOrEqual:
                    while (Items.Count > PageSize)
                    {
                        _items.RemoveAt(_items.Count - 1);
                    }
                    break;
            }
        }

        #region UpCommand

        private bool UpCommandCanExecute(object obj)
        {
            if (!RefreshCommandCanExecute(obj))
                return false;
            switch (CriteriaType)
            {
                case CriteriaType.DownFromInfinity:
                    return false;
                case CriteriaType.DownFrom:
                case CriteriaType.DownFromOrEqual:
                    return true;
                case CriteriaType.UpFrom:
                case CriteriaType.UpFromOrEqual:
                case CriteriaType.UpFromInfinity:
                    return _items.Count >= PageSize;
                default:
                    return false;
            }
        }

        private void UpCommandExecuted(object obj)
        {
            if (_items.Any())
            {
                Refresh(CriteriaType.UpFrom, _getCriteriaFunc(_items.First()));
            }
            else
            {
                switch (CriteriaType)
                {
                    case CriteriaType.DownFrom:
                        Refresh(CriteriaType.UpFromOrEqual, Criteria);
                        break;
                    case CriteriaType.DownFromOrEqual:
                        Refresh(CriteriaType.UpFrom, Criteria);
                        break;
                }
            }
        }
        
        public ICommand UpCommand { get { return _upCommand; } }

        #endregion

        #region UpUpCommand

        private bool UpUpCommandCanExecute(object obj)
        {
            return RefreshCommandCanExecute(obj);
        }

        private void UpUpCommandExecuted(object obj)
        {
            Refresh(CriteriaType.DownFromInfinity, Criteria);
        }

        public ICommand UpUpCommand { get { return _upUpCommand; } }

        #endregion

        #region DownCommand

        public ICommand DownCommand { get { return _downCommand; } }

        private bool DownCommandCanExecute(object obj)
        {
            if (!RefreshCommandCanExecute(obj))
                return false;
            switch (CriteriaType)
            {
                case CriteriaType.UpFromInfinity:
                    return false;
                case CriteriaType.UpFrom:
                case CriteriaType.UpFromOrEqual:
                    return true;
                case CriteriaType.DownFrom:
                case CriteriaType.DownFromOrEqual:
                case CriteriaType.DownFromInfinity:
                    return _items.Count >= PageSize;
                default:
                    return false;
            }
        }

        private void DownCommandExecuted(object obj)
        {
            if (_items.Any())
            {
                Refresh(CriteriaType.DownFrom, _getCriteriaFunc(_items.Last()));
            }
            else
            {
                switch (CriteriaType)
                {
                    case CriteriaType.UpFrom:
                        Refresh(CriteriaType.DownFromOrEqual, Criteria);
                        break;
                    case CriteriaType.UpFromOrEqual:
                        Refresh(CriteriaType.DownFrom, Criteria);
                        break;
                }
            }
        }

        #endregion

        #region DownDownCommand

        private bool DownDownCommandCanExecute(object obj)
        {
            return RefreshCommandCanExecute(obj);
        }

        private void DownDownCommandExecuted(object obj)
        {
            Refresh(CriteriaType.UpFromInfinity, Criteria);
        }

        public ICommand DownDownCommand { get { return _downDownCommand; } }

        #endregion

        #region RefreshCommand

        public ICommand RefreshCommand { get { return _refreshCommand; } }

        private bool RefreshCommandCanExecute(object obj)
        {
            return PaginationService != null;
        }

        private void RefreshCommandExecuted(object obj)
        {
            Refresh(CriteriaType, Criteria);
        }

        #endregion

        #region CancelCommand

        public ICommand CancelCommand { get { return _cancelCommand; } }

        private void CancelCommandExecuted(object obj)
        {
            _refreshGuid = Guid.NewGuid();
            Busy = false;
        }

        private bool CancelCommandCanExecute(object obj)
        {
            return Busy;
        }

        #endregion

        public void Refresh(CriteriaType criteriaType, TCriteria criteria)
        {
            Busy = true;
            var refreshGuid = Guid.NewGuid();
            _refreshGuid = refreshGuid;
            PaginationService.GetItems(criteriaType, PageSize, criteria, items =>
                {
                    if (_refreshGuid != refreshGuid)
                        return;
                    _dispatcher.Invoke(() =>
                        {
                            CriteriaType = criteriaType;
                            if (criteriaType == CriteriaType.DownFromInfinity && items.Any())
                            {
                                Criteria = _getCriteriaFunc(items.First());
                            }
                            else if (criteriaType == CriteriaType.UpFromInfinity && items.Any())
                            {
                                Criteria = _getCriteriaFunc(items.Last());
                            }
                            else
                            {
                                Criteria = criteria;
                            }
                            UpdateItems(items);
                            Busy = false;
                        });
                });
        }
        
        private void UpdateItems(IEnumerable<T> items)
        {
            foreach (var item in _items.ToArray().Where(item => !items.Any(i => AreSameItems(i, item))))
            {
                _items.Remove(item);
            }
            var index = 0;
            foreach (var item in items)
            {
                if (_items.Count <= index)
                {
                    _items.Add(item);
                }
                else if (AreSameItems(_items[index], item))
                {
                    _updateAction(_items[index], item);
                }
                else
                {
                    _items.Insert(index, item);
                }
                index++;
            }
            _upCommand.RaiseCanExecuteChanged();
            _downCommand.RaiseCanExecuteChanged();
        }

        private bool AreSameItems(T item1, T item2)
        {
            return Equals(_getCriteriaFunc(item1), _getCriteriaFunc(item2));
        }

        public bool Busy
        {
            get { return _busy; }
            private set
            {
                _busy = value;
                OnPropertyChanged("Busy");
            }
        }

        public TCriteria Criteria
        {
            get { return _criteria; }
            private set
            {
                _criteria = value;
                OnPropertyChanged("Criteria");
            }
        }

        public CriteriaType CriteriaType
        {
            get { return _criteriaType; }
            private set
            {
                _criteriaType = value;
                OnPropertyChanged("CriteriaType");
            }
        }

        public IPaginationService<T, TCriteria> PaginationService
        {
            get { return _paginationService; }
            set
            {
                _paginationService = value;
                OnPropertyChanged("PaginationService");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string property)
        {
            EventHelper.Raise(ref PropertyChanged, this, new PropertyChangedEventArgs(property));
            if (property == "PaginationService" || property == "PageSize")
            {
                _refreshCommand.RaiseCanExecuteChanged();
                _upUpCommand.RaiseCanExecuteChanged();
                _upCommand.RaiseCanExecuteChanged();
                _downCommand.RaiseCanExecuteChanged();
                _downDownCommand.RaiseCanExecuteChanged();
            }
        }
    }
}
