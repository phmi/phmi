using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using PHmiClient.Utils;
using PHmiModel.Interfaces;

namespace PHmiModel.Entities
{
    public class Entity : IEntity, INotifyPropertyChanged, IDataErrorInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        #region IDataErrorInfo and INotifyPropertyChanged

        public string this[string columnName]
        {
            get { return this.GetError(columnName); }
        }

        public string Error { get { return this.GetError(); } }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string property)
        {
            var errorPropertyName = PropertyHelper.GetPropertyName(this, e => e.Error);
            if (property != errorPropertyName)
                EventHelper.Raise(ref PropertyChanged, this, new PropertyChangedEventArgs(property));
        }

        protected virtual void OnPropertyChanged<T>(T obj, Expression<Func<T, object>> getPropertyExpression)
        {
            OnPropertyChanged(PropertyHelper.GetPropertyName(obj, getPropertyExpression));
        }

        #endregion
    }
}
