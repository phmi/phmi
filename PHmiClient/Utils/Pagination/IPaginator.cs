using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace PHmiClient.Utils.Pagination
{
    public interface IPaginator : INotifyPropertyChanged
    {
        int PageSize { get; set; }
        ICommand UpUpCommand { get; }
        ICommand UpCommand { get; }
        ICommand DownCommand { get; }
        ICommand DownDownCommand { get; }
        ICommand RefreshCommand { get; }
        ICommand CancelCommand { get; }
        bool Busy { get; }
        CriteriaType CriteriaType { get; }
    }

    public interface IPaginator<T, TCriteria> : IPaginator
    {
        TCriteria Criteria { get; }
        ReadOnlyObservableCollection<T> Items { get; }
        IPaginationService<T, TCriteria> PaginationService { get; set; }
        void Refresh(CriteriaType criteriaType, TCriteria criteria);
    }
}
