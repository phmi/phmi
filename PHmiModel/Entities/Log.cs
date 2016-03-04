using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PHmiClient.Utils;
using PHmiClient.Utils.ValidationAttributes;
using PHmiResources;
using PHmiResources.Loc;

namespace PHmiModel.Entities
{
    [MetadataType(typeof(LogMetadata))]
    [Table("logs", Schema = "public")]
    public class Log : NamedEntity
    {
        public Log()
        {
            TimeToStoreDb = TimeSpan.FromDays(31).Ticks;
        }

        public class LogMetadata : EntityMetadataBase
        {
            private string _name;
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
                    OnPropertyChanged(this, i => i.Name);
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
    }
}
