using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PHmiClient.Utils;
using PHmiClient.Utils.ValidationAttributes;
using PHmiModel.Interfaces;
using PHmiResources;
using PHmiResources.Loc;

namespace PHmiModel.Entities
{
    [MetadataType(typeof(AlarmCategoryMetadata))]
    [Table("alarm_categories", Schema = "public")]
    public class AlarmCategory : NamedEntity, IRepository
    {
        public AlarmCategory()
        {
            TimeToStoreDb = TimeSpan.FromDays(31).Ticks;
        }

        public class AlarmCategoryMetadata : EntityMetadataBase
        {
            private string _name;
            private string _description;
            private string _timeToStore;

            [LocDisplayName("Name", ResourceType = typeof(Res))]
            [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Res))]
            [RegularExpression(RegexPatterns.VariableName, ErrorMessageResourceName = "DotNetNameMessage", ErrorMessageResourceType = typeof(Res))]
            public string Name
            {
                get { return _name; }
                set
                {
                    _name = value;
                    OnPropertyChanged(this, m => m.Name);
                }
            }

            [LocDisplayName("Description", ResourceType = typeof(Res))]
            public string Description
            {
                get { return _description; }
                set
                {
                    _description = value;
                    OnPropertyChanged(this, m => m.Description);
                }
            }

            [LocDisplayName("TimeToStore", ResourceType = typeof(Res))]
            [ValidTimeSpan(AllowNull = true, ErrorMessageResourceName = "InvalidTimeSpanMessage", ErrorMessageResourceType = typeof(Res))]
            public string TimeToStore
            {
                get { return _timeToStore; }
                set
                {
                    _timeToStore = value;
                    OnPropertyChanged(this, m => m.TimeToStore);
                }
            }
        }

        #region TimeToStore

        private string _timeToStore;

        [NotMapped]
        public string TimeToStore
        {
            get { return _timeToStore; }
            set
            {
                _timeToStore = value;
                if (string.IsNullOrEmpty(_timeToStore))
                {
                    TimeToStoreDb = null;
                    return;
                }
                TimeSpan d;
                if (TimeSpan.TryParse(_timeToStore, out d))
                {
                    TimeToStoreDb = d.Ticks;
                }
                else
                {
                    OnPropertyChanged(this, e => e.TimeToStore);
                }
            }
        }

        #endregion

        public ICollection<T> GetRepository<T>()
        {
            if (typeof(T) == typeof(AlarmTag))
            {
                return AlarmTags as ICollection<T>;
            }
            throw new NotSupportedException();
        }

        #region Description

        private string _description;

        [Column("description")]
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChanged(this, e => e.Description);
            }
        }

        #endregion

        #region TimeToStoreDb

        private long? _timeToStoreDb;

        [Column("time_to_store")]
        public long? TimeToStoreDb
        {
            get { return _timeToStoreDb; }
            set
            {
                _timeToStoreDb = value;
                _timeToStore = TimeToStoreDb.HasValue ? new TimeSpan(TimeToStoreDb.Value).ToString() : null;
                OnPropertyChanged(this, e => e.TimeToStore);
                OnPropertyChanged(this, e => e.TimeToStoreDb);
            }
        }

        #endregion

        #region AlarmTags

        private ICollection<AlarmTag> _alarmTags;

        public virtual ICollection<AlarmTag> AlarmTags
        {
            get { return _alarmTags ?? (_alarmTags = new ObservableCollection<AlarmTag>()); }
            set { _alarmTags = value; }
        }

        #endregion
    }
}
