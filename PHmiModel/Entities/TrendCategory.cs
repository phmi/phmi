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
    [MetadataType(typeof(TrendCategoryMetadata))]
    [Table("trend_categories", Schema = "public")]
    public class TrendCategory : NamedEntity, IRepository
    {
        public TrendCategory()
        {
            TimeToStoreDb = TimeSpan.FromDays(31).Ticks;
            PeriodDb = TimeSpan.FromSeconds(5).Ticks;
        }

        public class TrendCategoryMetadata : EntityMetadataBase
        {
            private string _name;
            private string _timeToStore;
            private string _period;

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

            [LocDisplayName("Period", ResourceType = typeof(Res))]
            [ValidTimeSpan(AllowNull = false, ErrorMessageResourceName = "InvalidTimeSpanMessage", ErrorMessageResourceType = typeof(Res))]
            public string Period
            {
                get { return _period; }
                set
                {
                    _period = value;
                    OnPropertyChanged(this, m => m.Period);
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

        #region Period

        private string _periodStr;

        [NotMapped]
        public string Period
        {
            get { return _periodStr; }
            set
            {
                _periodStr = value;
                TimeSpan d;
                if (TimeSpan.TryParse(_periodStr, out d))
                {
                    PeriodDb = d.Ticks;
                }
                else
                {
                    OnPropertyChanged(this, e => e.Period);
                }
            }
        }

        #endregion

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
            if (typeof(T) == typeof(TrendTag))
            {
                return TrendTags as ICollection<T>;
            }
            throw new NotSupportedException();
        }

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

        #region PeriodDb

        private long _periodDb;

        [Column("period")]
        public long PeriodDb
        {
            get { return _periodDb; }
            set
            {
                _periodDb = value;
                _periodStr = new TimeSpan(PeriodDb).ToString();
                OnPropertyChanged(this, e => e.Period);
                OnPropertyChanged(this, e => e.PeriodDb);
            }
        }

        #endregion

        #region TrendTags

        private ICollection<TrendTag> _trendTags;

        public virtual ICollection<TrendTag> TrendTags
        {
            get { return _trendTags ?? (_trendTags = new ObservableCollection<TrendTag>()); }
            set { _trendTags = value; }
        }

        #endregion
    }
}
