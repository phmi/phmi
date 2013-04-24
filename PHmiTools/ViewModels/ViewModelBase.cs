using System;
using System.ComponentModel;
using System.Linq.Expressions;
using PHmiClient.Utils;

namespace PHmiTools.ViewModels
{
    public abstract class ViewModelBase<T> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged<TDescender>(TDescender obj, Expression<Func<TDescender, object>> expression)
            where TDescender : ViewModelBase<T>
        {
            var property = PropertyHelper.GetPropertyName(expression);
            OnPropertyChanged(property);
        }

        protected virtual void OnPropertyChanged(string property)
        {
            EventHelper.Raise(ref PropertyChanged, this, new PropertyChangedEventArgs(property));
        }

        public T View { get; set; }
    }
}
