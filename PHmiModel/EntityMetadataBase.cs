using System;
using System.ComponentModel;
using System.Linq.Expressions;
using PHmiClient.Utils;

namespace PHmiModel
{
    public abstract class EntityMetadataBase : IDataErrorInfo, INotifyPropertyChanged
    {
        public string this[string columnName]
        {
            get { return this.GetError(columnName); }
        }

        public string Error { get { return this.GetError(); } }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged<TDescender>(TDescender obj, Expression<Func<TDescender, object>> expression)
            where TDescender : EntityMetadataBase
        {
            var property = PropertyHelper.GetPropertyName(expression);
            EventHelper.Raise(ref PropertyChanged, this, new PropertyChangedEventArgs(property));
            var errorPropertyName = PropertyHelper.GetPropertyName(this, entity => entity.Error);
            if (property != errorPropertyName)
                EventHelper.Raise(ref PropertyChanged, this, new PropertyChangedEventArgs(errorPropertyName));
        }
    }
}
