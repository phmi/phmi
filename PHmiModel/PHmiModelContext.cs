using System;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Objects.DataClasses;
using System.IO;
using System.Linq;
using System.Reflection;
using Npgsql;
using PHmiClient.Utils;
using PHmiModel.Entities;
using PHmiModel.Interfaces;

namespace PHmiModel
{
    public class PHmiModelContext : DbContext, IModelContext
    {
        public PHmiModelContext(string connectionString)
            : base(new NpgsqlConnection(connectionString), true)
        {
            Database.SetInitializer<PHmiModelContext>(null);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<AlarmCategory>()
                .HasMany<AlarmTag>(e => e.AlarmTags)
                .WithRequired(e => e.AlarmCategory)
                .HasForeignKey(e => e.RefCategories);

            modelBuilder
                .Entity<DigTag>()
                .HasMany<AlarmTag>(e => e.AlarmTags)
                .WithRequired(e => e.DigTag)
                .HasForeignKey(e => e.RefDigTags);

            modelBuilder
                .Entity<DigTag>()
                .HasMany<TrendTag>(e => e.TrendTags)
                .WithOptional(e => e.Trigger)
                .HasForeignKey(e => e.RefTrigger);

            modelBuilder
                .Entity<IoDevice>()
                .HasMany<DigTag>(e => e.DigTags)
                .WithRequired(e => e.IoDevice)
                .HasForeignKey(e => e.RefIoDevices);

            modelBuilder
                .Entity<IoDevice>()
                .HasMany<NumTag>(e => e.NumTags)
                .WithRequired(e => e.IoDevice)
                .HasForeignKey(e => e.RefIoDevice);

            modelBuilder
                .Entity<NumTagType>()
                .HasMany<NumTag>(e => e.NumTags)
                .WithRequired(e => e.NumTagType)
                .HasForeignKey(e => e.RefTagType);

            modelBuilder
                .Entity<NumTag>()
                .HasMany<TrendTag>(e => e.TrendTags)
                .WithRequired(e => e.NumTag)
                .HasForeignKey(e => e.RefNumTag);

            modelBuilder
                .Entity<TrendCategory>()
                .HasMany<TrendTag>(e => e.TrendTags)
                .WithRequired(e => e.TrendCategory)
                .HasForeignKey(e => e.RefCategory);

            base.OnModelCreating(modelBuilder);
        }
        
        public DbSet<AlarmCategory> AlarmCategories { get; set; }

        public DbSet<AlarmTag> AlarmTags { get; set; }

        public DbSet<DigTag> DigTags { get; set; }

        public DbSet<IoDevice> IoDevices { get; set; }

        public DbSet<Log> Logs { get; set; }

        public DbSet<NumTagType> NumTagTypes { get; set; }

        public DbSet<NumTag> NumTags { get; set; }

        public DbSet<Settings> Settings { get; set; }

        public DbSet<TrendCategory> TrendCategories { get; set; }

        public DbSet<TrendTag> TrendTags { get; set; }

        public DbSet<User> Users { get; set; } 

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
            var manager = ((IObjectContextAdapter)this).ObjectContext.ObjectStateManager;
            manager.ObjectStateManagerChanged += ObjectStateManagerChanged;
        }

        private void ObjectStateManagerChanged(object sender, CollectionChangeEventArgs e)
        {
            var entity = (Entity)e.Element;
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
            if (entity.Id == 0)
            {
                HasChanges = true;
            }
            else if (Entry(entity).State != EntityState.Unchanged)
            {
                HasChanges = true;
            }
        }

        private void ModuleAssociationChanged(object sender, CollectionChangeEventArgs e)
        {
            var entity = (EntityObject) e.Element;
            if (e.Action != CollectionChangeAction.Refresh && entity.EntityState != EntityState.Unchanged)
                HasChanges = true;
        }

        private void EntityPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var entity = (Entity)sender;
            if (entity.Id == 0)
            {
                HasChanges = true;
            }
            else if (Entry(entity).State != EntityState.Unchanged)
            {
                HasChanges = true;
            }
        }

        #region HasChanges

        private bool _hasChanges;

        public bool HasChanges
        {
            get { return _hasChanges; }
            private set
            {
                _hasChanges = value;
                OnPropertyChanged(PropertyHelper.GetPropertyName(this, e => e.HasChanges));
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
            if (typeof(T) == typeof(AlarmCategory))
            {
                AlarmCategories.Add(entity as AlarmCategory);
            }
            else if (typeof(T) == typeof(AlarmTag))
            {
                AlarmTags.Add(entity as AlarmTag);
            }
            else if (typeof(T) == typeof(DigTag))
            {
                DigTags.Add(entity as DigTag);
            }
            else if (typeof(T) == typeof(IoDevice))
            {
                IoDevices.Add(entity as IoDevice);
            }
            else if (typeof(T) == typeof(Log))
            {
                Logs.Add(entity as Log);
            }
            else if (typeof(T) == typeof(NumTag))
            {
                NumTags.Add(entity as NumTag);
            }
            else if (typeof(T) == typeof(TrendCategory))
            {
                TrendCategories.Add(entity as TrendCategory);
            }
            else if (typeof(T) == typeof(TrendTag))
            {
                TrendTags.Add(entity as TrendTag);
            }
            else if (typeof(T) == typeof(User))
            {
                Users.Add(entity as User);
            }
            else throw new NotImplementedException();
        }

        public void DeleteObject(object entity)
        {
            var entry = Entry(entity);
            entry.State = entry.State == EntityState.Added ? EntityState.Detached : EntityState.Deleted;
        }

        public IQueryable<T> Get<T>()
        {
            if (typeof(T) == typeof(AlarmCategory))
            {
                return (IQueryable<T>)AlarmCategories;
            }
            if (typeof(T) == typeof(IoDevice))
            {
                return (IQueryable<T>) IoDevices;
            }
            if (typeof(T) == typeof(DigTag))
            {
                return (IQueryable<T>) DigTags;
            }
            if (typeof(T) == typeof(Log))
            {
                return (IQueryable<T>) Logs;
            }
            if (typeof(T) == typeof(NumTag))
            {
                return (IQueryable<T>) NumTags;
            }
            if (typeof(T) == typeof(NumTagType))
            {
                return (IQueryable<T>) NumTagTypes;
            }
            if (typeof(T) == typeof(Settings))
            {
                return (IQueryable<T>)Settings;
            }
            if (typeof(T) == typeof(TrendCategory))
            {
                return (IQueryable<T>) TrendCategories;
            }
            if (typeof(T) == typeof(User))
            {
                return (IQueryable<T>) Users;
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
