using System;
using System.ComponentModel;
using System.Linq;

namespace PHmiModel.Interfaces
{
    public interface IModelContext : INotifyPropertyChanged, IDisposable
    {
        bool HasChanges { get; }
        void Save();
        void DeleteObject(object entity);
        void AddTo<T>(T entity);
        IQueryable<T> Get<T>();
    }
}
