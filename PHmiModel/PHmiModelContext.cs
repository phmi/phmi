using System;
using System.ComponentModel;
using System.Data;
using System.Data.Objects.DataClasses;
using System.IO;
using System.Linq;
using System.Reflection;
using PHmiClient.Utils;
using PHmiModel.Interfaces;

namespace PHmiModel
{
    public partial class PHmiModelContext : IModelContext
    {
        private static Stream GetEmbeddedResource(string resource)
        {
            var assembly = Assembly.GetAssembly(typeof(PHmiModelContext));
            return assembly.GetManifestResourceStream(resource);
        }

        public static Stream GetPHmiScriptStream()
        {
            return GetEmbeddedResource("PHmiModel.Schemes.PHmi.sql");
        }

        public static Stream GetPHmiScriptRowsStream()
        {
            return GetEmbeddedResource("PHmiModel.Schemes.PHmiRows.sql");
        }

        public void StartTrackingChanges()
        {
            ObjectStateManager.ObjectStateManagerChanged += ObjectStateManagerChanged;
        }

        private void ObjectStateManagerChanged(object sender, CollectionChangeEventArgs e)
        {
            var entity = (EntityObject)e.Element;
            switch (e.Action)
            {
                case CollectionChangeAction.Add:
                    entity.PropertyChanged += EntityPropertyChanged;
                    foreach (var propertyInfo in entity.GetType().GetProperties())
                    {
                        if (!propertyInfo.PropertyType.Name.Contains("EntityCollection"))
                            continue;
                        var property = propertyInfo.GetValue(entity, null);
                        var relatedEnd = property as RelatedEnd;
                        if (relatedEnd != null)
                            relatedEnd.AssociationChanged += ModuleAssociationChanged;
                    }
                    break;
                case CollectionChangeAction.Remove:
                    entity.PropertyChanged -= EntityPropertyChanged;
                    foreach (var propertyInfo in entity.GetType().GetProperties())
                    {
                        if (!propertyInfo.PropertyType.Name.Contains("EntityCollection"))
                            continue;
                        var property = propertyInfo.GetValue(entity, null);
                        var relatedEnd = property as RelatedEnd;
                        if (relatedEnd != null)
                            relatedEnd.AssociationChanged -= ModuleAssociationChanged;
                    }
                    HasChanges = true;
                    break;
            }
            if (entity.EntityState != EntityState.Unchanged)
                HasChanges = true;
        }

        private void ModuleAssociationChanged(object sender, CollectionChangeEventArgs e)
        {
            var entity = (EntityObject) e.Element;
            if (e.Action != CollectionChangeAction.Refresh && entity.EntityState != EntityState.Unchanged)
                HasChanges = true;
        }

        private void EntityPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var entity = (EntityObject)sender;
            if (entity.EntityState != EntityState.Unchanged)
                HasChanges = true;
        }

        #region HasChanges

        private bool _hasChanges;

        public bool HasChanges
        {
            get { return _hasChanges; }
            private set
            {
                _hasChanges = value;
                OnPropertyChanged("HasChanges");
            }
        }

        #endregion

        public void Save()
        {
            SaveChanges();
            HasChanges = false;
        }

        public void AddTo<T>(T entity)
        {
            if (typeof(T) == typeof(alarm_categories))
            {
                AddToalarm_categories(entity as alarm_categories);
            }
            else if (typeof(T) == typeof(alarm_tags))
            {
                AddToalarm_tags(entity as alarm_tags);
            }
            else if (typeof(T) == typeof(dig_tags))
            {
                AddTodig_tags(entity as dig_tags);
            }
            else if (typeof(T) == typeof(io_devices))
            {
                AddToio_devices(entity as io_devices);
            }
            else if (typeof(T) == typeof(logs))
            {
                AddTologs(entity as logs);
            }
            else if (typeof(T) == typeof(num_tags))
            {
                AddTonum_tags(entity as num_tags);
            }
            else if (typeof(T) == typeof(trend_categories))
            {
                AddTotrend_categories(entity as trend_categories);
            }
            else if (typeof(T) == typeof(trend_tags))
            {
                AddTotrend_tags(entity as trend_tags);
            }
            else if (typeof(T) == typeof(users))
            {
                AddTousers(entity as users);
            }
            else throw new NotImplementedException();
        }

        public IQueryable<T> Get<T>()
        {
            if (typeof(T) == typeof(alarm_categories))
            {
                return (IQueryable<T>)alarm_categories;
            }
            if (typeof(T) == typeof(io_devices))
            {
                return (IQueryable<T>) io_devices;
            }
            if (typeof(T) == typeof(dig_tags))
            {
                return (IQueryable<T>) dig_tags;
            }
            if (typeof(T) == typeof(logs))
            {
                return (IQueryable<T>) logs;
            }
            if (typeof(T) == typeof(num_tags))
            {
                return (IQueryable<T>) num_tags;
            }
            if (typeof(T) == typeof(num_tag_types))
            {
                return (IQueryable<T>) num_tag_types;
            }
            if (typeof(T) == typeof(settings))
            {
                return (IQueryable<T>)settings;
            }
            if (typeof(T) == typeof(trend_categories))
            {
                return (IQueryable<T>) trend_categories;
            }
            if (typeof(T) == typeof(users))
            {
                return (IQueryable<T>) users;
            }
            throw new NotSupportedException();
        }

        #region PropertyChanged
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            EventHelper.Raise(ref PropertyChanged, this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
