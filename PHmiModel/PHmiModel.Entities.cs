using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using PHmiClient.Utils;
using PHmiClient.Utils.ValidationAttributes;
using PHmiModel.Interfaces;
using PHmiResources;
using PHmiResources.Loc;

namespace PHmiModel
{
    [MetadataType(typeof(AlarmCategoriesMetadata))]
    public partial class alarm_categories : IDataErrorInfo, INamedEntity, IRepository
    {
        public alarm_categories()
        {
            time_to_store = TimeSpan.FromDays(31).Ticks;
        }

        public class AlarmCategoriesMetadata : EntityMetadataBase
        {
            private string _name;
            private string _description;
            private string _timeToStore;

            [LocDisplayName("Name", ResourceType = typeof(Res))]
            [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Res))]
            [RegularExpression(RegexPatterns.VariableName, ErrorMessageResourceName = "DotNetNameMessage", ErrorMessageResourceType = typeof(Res))]
            public string name
            {
                get { return _name; }
                set
                {
                    _name = value;
                    OnPropertyChanged(this, m => m.name);
                }
            }

            [LocDisplayName("Description", ResourceType = typeof(Res))]
            public string description
            {
                get { return _description; }
                set
                {
                    _description = value;
                    OnPropertyChanged(this, m => m.description);
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

        public string TimeToStore
        {
            get { return _timeToStore; }
            set
            {
                _timeToStore = value;
                if (string.IsNullOrEmpty(_timeToStore))
                {
                    time_to_store = null;
                    return;
                }
                TimeSpan d;
                if (TimeSpan.TryParse(_timeToStore, out d))
                {
                    time_to_store = d.Ticks;
                }
                else
                {
                    OnPropertyChanged(PropertyHelper.GetPropertyName(this, e => e.TimeToStore));
                }
            }
        }

        partial void Ontime_to_storeChanged()
        {
            _timeToStore = time_to_store.HasValue ? new TimeSpan(time_to_store.Value).ToString() : null;
            OnPropertyChanged(PropertyHelper.GetPropertyName(this, e => e.TimeToStore));
        }

        #endregion

        #region IDataErrorInfo and INotifyPropertyChanged

        public string this[string columnName]
        {
            get { return this.GetError(columnName); }
        }

        public string Error { get { return this.GetError(); } }

        protected override void OnPropertyChanged(string property)
        {
            base.OnPropertyChanged(property);
            var errorPropertyName = PropertyHelper.GetPropertyName(this, e => e.Error);
            if (property != errorPropertyName)
                OnPropertyChanged(errorPropertyName);
        }

        #endregion

        public ICollection<T> GetRepository<T>()
        {
            if (typeof(T) == typeof(alarm_tags))
            {
                return alarm_tags as ICollection<T>;
            }
            throw new NotSupportedException();
        }
    }

    [MetadataType(typeof(AlarmTagsMetadata))]
    public partial class alarm_tags : IDataErrorInfo, INamedEntity
    {
        public class AlarmTagsMetadata : EntityMetadataBase
        {
            private string _name;
            private dig_tags _digTags;
            private string _location;
            private string _description;
            private bool _acknowledgeable;
            private int? _privilege;

            [LocDisplayName("Name", ResourceType = typeof(Res))]
            [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Res))]
            [RegularExpression(RegexPatterns.VariableName, ErrorMessageResourceName = "DotNetNameMessage", ErrorMessageResourceType = typeof(Res))]
            public string name
            {
                get { return _name; }
                set
                {
                    _name = value;
                    OnPropertyChanged(this, m => m.name);
                }
            }

            [LocDisplayName("DigitalTag", ResourceType = typeof(Res))]
            [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Res))]
            public dig_tags dig_tags
            {
                get { return _digTags; }
                set
                {
                    _digTags = value;
                    OnPropertyChanged(this, m => m.dig_tags);
                }
            }

            [LocDisplayName("Location", ResourceType = typeof(Res))]
            public string location
            {
                get { return _location; }
                set
                {
                    _location = value;
                    OnPropertyChanged(this, m => m.location);
                }
            }

            [LocDisplayName("Description", ResourceType = typeof(Res))]
            public string description
            {
                get { return _description; }
                set
                {
                    _description = value;
                    OnPropertyChanged(this, m => m.description);
                }
            }

            [LocDisplayName("Acknowledgeable", ResourceType = typeof(Res))]
            public bool acknowledgeable
            {
                get { return _acknowledgeable; }
                set
                {
                    _acknowledgeable = value;
                    OnPropertyChanged(this, m => m.acknowledgeable);
                }
            }

            [LocDisplayName("Privilege", ResourceType = typeof(Res))]
            public int? privilege
            {
                get { return _privilege; }
                set
                {
                    _privilege = value;
                    OnPropertyChanged(this, m => m.privilege);
                }
            }
        }

        partial void Onref_dig_tagsChanged()
        {
            OnPropertyChanged(PropertyHelper.GetPropertyName(this, t => t.dig_tags));
        }

        #region IDataErrorInfo and INotifyPropertyChanged

        public string this[string columnName]
        {
            get { return this.GetError(columnName); }
        }

        public string Error { get { return this.GetError(); } }

        protected override void OnPropertyChanged(string property)
        {
            base.OnPropertyChanged(property);
            var errorPropertyName = PropertyHelper.GetPropertyName(this, e => e.Error);
            if (property != errorPropertyName)
                OnPropertyChanged(errorPropertyName);
        }

        #endregion
    }

    [MetadataType(typeof(DigTagsMetadata))]
    public partial class dig_tags : IDataErrorInfo, INamedEntity
    {
        public dig_tags()
        {
            can_read = true;
        }

        public class DigTagsMetadata : EntityMetadataBase
        {
            private string _name;
            private string _device;
            private string _description;
            private bool _canRead;

            [LocDisplayName("Name", ResourceType = typeof(Res))]
            [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Res))]
            [RegularExpression(RegexPatterns.VariableName, ErrorMessageResourceName = "DotNetNameMessage", ErrorMessageResourceType = typeof(Res))]
            public string name
            {
                get { return _name; }
                set
                {
                    _name = value;
                    OnPropertyChanged(this, i => i.name);
                }
            }

            [LocDisplayName("AddressInDevice", ResourceType = typeof(Res))]
            [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Res))]
            public string device
            {
                get { return _device; }
                set
                {
                    _device = value;
                    OnPropertyChanged(this, i => i.device);
                }
            }

            [LocDisplayName("Description", ResourceType = typeof(Res))]
            public string description
            {
                get { return _description; }
                set
                {
                    _description = value;
                    OnPropertyChanged(this, i => i.description);
                }
            }

            [LocDisplayName("CanRead", ResourceType = typeof(Res))]
            public bool can_read
            {
                get { return _canRead; }
                set
                {
                    _canRead = value;
                    OnPropertyChanged(this, i => i.can_read);
                }
            }
        }

        public string FullName { get { return io_devices.name + "." + name; } }

        #region IDataErrorInfo and INotifyPropertyChanged

        public string this[string columnName]
        {
            get { return this.GetError(columnName); }
        }

        public string Error { get { return this.GetError(); } }

        protected override void OnPropertyChanged(string property)
        {
            base.OnPropertyChanged(property);
            var errorPropertyName = PropertyHelper.GetPropertyName(this, e => e.Error);
            if (property != errorPropertyName)
                OnPropertyChanged(errorPropertyName);
        }

        #endregion
    }

    [MetadataType(typeof(IoDevicesMetadata))]
    public partial class io_devices : IDataErrorInfo, INamedEntity, IRepository
    {
	    public class IoDevicesMetadata : EntityMetadataBase
	    {
	        private string _name;
	        private string _type;
	        private string _options;

	        [LocDisplayName("Name", ResourceType = typeof(Res))]
            [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Res))]
            [RegularExpression(RegexPatterns.VariableName, ErrorMessageResourceName = "DotNetNameMessage", ErrorMessageResourceType = typeof(Res))]
            public string name
	        {
	            get { return _name; }
	            set
	            {
	                _name = value;
                    OnPropertyChanged(this, i => i.name);
	            }
	        }

	        [LocDisplayName("Type", ResourceType = typeof(Res))]
            [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Res))]
            public string type
	        {
	            get { return _type; }
	            set
	            {
	                _type = value;
                    OnPropertyChanged(this, i => i.type);
	            }
	        }

	        [LocDisplayName("Options", ResourceType = typeof(Res))]
            public string options
	        {
	            get { return _options; }
	            set
	            {
	                _options = value;
                    OnPropertyChanged(this, i => i.options);
	            }
	        }
	    }

        #region IDataErrorInfo and INotifyPropertyChanged

        public string this[string columnName]
        {
            get { return this.GetError(columnName) ; }
        }

        public string Error { get { return this.GetError(); } }

        protected override void OnPropertyChanged(string property)
        {
            base.OnPropertyChanged(property);
            var errorPropertyName = PropertyHelper.GetPropertyName(this, e => e.Error);
            if (property != errorPropertyName)
                OnPropertyChanged(errorPropertyName);
        }

        #endregion

        public ICollection<T> GetRepository<T>()
        {
            if (typeof(T) == typeof(dig_tags))
                return dig_tags as ICollection<T>;
            if (typeof(T) == typeof(num_tags))
                return num_tags as ICollection<T>;
            throw new NotSupportedException();
        }
    }

    [MetadataType(typeof(LogsMetadata))]
    public partial class logs : IDataErrorInfo, INamedEntity
    {
        public logs()
        {
            time_to_store = TimeSpan.FromDays(31).Ticks;
        }

        public class LogsMetadata : EntityMetadataBase
        {
            private string _name;
            private string _timeToStore;

            [LocDisplayName("Name", ResourceType = typeof(Res))]
            [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Res))]
            [RegularExpression(RegexPatterns.VariableName, ErrorMessageResourceName = "DotNetNameMessage", ErrorMessageResourceType = typeof(Res))]
            public string name
            {
                get { return _name; }
                set
                {
                    _name = value;
                    OnPropertyChanged(this, i => i.name);
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

        public string TimeToStore
        {
            get { return _timeToStore; }
            set
            {
                _timeToStore = value;
                if (string.IsNullOrEmpty(_timeToStore))
                {
                    time_to_store = null;
                    return;
                }
                TimeSpan d;
                if (TimeSpan.TryParse(_timeToStore, out d))
                {
                    time_to_store = d.Ticks;
                }
                else
                {
                    OnPropertyChanged(PropertyHelper.GetPropertyName(this, e => e.TimeToStore));
                }
            }
        }

        partial void Ontime_to_storeChanged()
        {
            _timeToStore = time_to_store.HasValue ? new TimeSpan(time_to_store.Value).ToString() : null;
            OnPropertyChanged(PropertyHelper.GetPropertyName(this, e => e.TimeToStore));
        }

        #endregion

        #region IDataErrorInfo and INotifyPropertyChanged

        public string this[string columnName]
        {
            get { return this.GetError(columnName); }
        }

        public string Error { get { return this.GetError(); } }

        protected override void OnPropertyChanged(string property)
        {
            base.OnPropertyChanged(property);
            var errorPropertyName = PropertyHelper.GetPropertyName(this, e => e.Error);
            if (property != errorPropertyName)
                OnPropertyChanged(errorPropertyName);
        }

        #endregion
    }

    [MetadataType(typeof(NumTagsMetadata))]
    public partial class num_tags : IDataErrorInfo, INamedEntity
    {
        public num_tags()
        {
            can_read = true;
        }

        public class NumTagsMetadata : EntityMetadataBase
        {
            private string _name;
            private string _device;
            private string _description;
            private bool _canRead;
            private string _engUnit;
            private num_tag_types _numTagTypes;
            private string _format;
            private string _rawMin;
            private string _rawMax;
            private string _engMin;
            private string _engMax;

            [LocDisplayName("Name", ResourceType = typeof(Res))]
            [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Res))]
            [RegularExpression(RegexPatterns.VariableName, ErrorMessageResourceName = "DotNetNameMessage", ErrorMessageResourceType = typeof(Res))]
            public string name
            {
                get { return _name; }
                set
                {
                    _name = value;
                    OnPropertyChanged(this, i => i.name);
                }
            }

            [LocDisplayName("AddressInDevice", ResourceType = typeof(Res))]
            [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Res))]
            public string device
            {
                get { return _device; }
                set
                {
                    _device = value;
                    OnPropertyChanged(this, i => i.device);
                }
            }

            [LocDisplayName("Description", ResourceType = typeof(Res))]
            public string description
            {
                get { return _description; }
                set
                {
                    _description = value;
                    OnPropertyChanged(this, i => i.description);
                }
            }

            [LocDisplayName("CanRead", ResourceType = typeof(Res))]
            public bool can_read
            {
                get { return _canRead; }
                set
                {
                    _canRead = value;
                    OnPropertyChanged(this, i => i.can_read);
                }
            }

            [LocDisplayName("TagType", ResourceType = typeof(Res))]
            [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Res))]
            public num_tag_types num_tag_types
            {
                get { return _numTagTypes; }
                set
                {
                    _numTagTypes = value;
                    OnPropertyChanged(this, i => i.num_tag_types);
                }
            }

            [LocDisplayName("Format", ResourceType = typeof(Res))]
            public string format
            {
                get { return _format; }
                set
                {
                    _format = value;
                    OnPropertyChanged(this, i => i.format);
                }
            }

            [LocDisplayName("EngUnit", ResourceType = typeof(Res))]
            public string eng_unit
            {
                get { return _engUnit; }
                set
                {
                    _engUnit = value;
                    OnPropertyChanged(this, i => i.eng_unit);
                }
            }

            [LocDisplayName("RawMin", ResourceType = typeof(Res))]
            [ValidDouble(AllowNull = true, ErrorMessageResourceName = "InvalidDoubleMessage", ErrorMessageResourceType = typeof(Res))]
            public string RawMin
            {
                get { return _rawMin; }
                set
                {
                    _rawMin = value;
                    OnPropertyChanged(this, i => i.RawMin);
                }
            }

            [LocDisplayName("RawMax", ResourceType = typeof(Res))]
            [ValidDouble(AllowNull = true, ErrorMessageResourceName = "InvalidDoubleMessage", ErrorMessageResourceType = typeof(Res))]
            public string RawMax
            {
                get { return _rawMax; }
                set
                {
                    _rawMax = value;
                    OnPropertyChanged(this, i => i.RawMax);
                }
            }

            [LocDisplayName("EngMin", ResourceType = typeof(Res))]
            [ValidDouble(AllowNull = true, ErrorMessageResourceName = "InvalidDoubleMessage", ErrorMessageResourceType = typeof(Res))]
            public string EngMin
            {
                get { return _engMin; }
                set
                {
                    _engMin = value;
                    OnPropertyChanged(this, i => i.EngMin);
                }
            }

            [LocDisplayName("EngMax", ResourceType = typeof(Res))]
            [ValidDouble(AllowNull = true, ErrorMessageResourceName = "InvalidDoubleMessage", ErrorMessageResourceType = typeof(Res))]
            public string EngMax
            {
                get { return _engMax; }
                set
                {
                    _engMax = value;
                    OnPropertyChanged(this, i => i.EngMax);
                }
            }
        }

        public string FullName { get { return io_devices.name + "." + name; } }
        
        #region RawMin
        
        private string _rawMin;

        public string RawMin
        {
            get { return _rawMin; }
            set
            {
                _rawMin = value;
                if (string.IsNullOrEmpty(_rawMin))
                {
                    raw_min = null;
                    return;
                }
                double d;
                if (double.TryParse(_rawMin, out d))
                {
                    raw_min = d;
                }
                else
                {
                    OnPropertyChanged(PropertyHelper.GetPropertyName(this, e => e.RawMin));
                }
            }
        }

        partial void Onraw_minChanged()
        {
            _rawMin = string.Format("{0}", raw_min);
            OnPropertyChanged(PropertyHelper.GetPropertyName(this, e => e.RawMin));
        }

        #endregion
        
        #region RawMax
        
        private string _rawMax;

        public string RawMax
        {
            get { return _rawMax; }
            set
            {
                _rawMax = value;
                if (string.IsNullOrEmpty(_rawMax))
                {
                    raw_max = null;
                    return;
                }
                double d;
                if (double.TryParse(_rawMax, out d))
                {
                    raw_max = d;
                }
                else
                {
                    OnPropertyChanged(PropertyHelper.GetPropertyName(this, e => e.RawMax));
                }
            }
        }

        partial void Onraw_maxChanged()
        {
            _rawMax = string.Format("{0}", raw_max);
            OnPropertyChanged(PropertyHelper.GetPropertyName(this, e => e.RawMax));
        }

        #endregion

        #region EngMin

        private string _engMin;

        public string EngMin
        {
            get { return _engMin; }
            set
            {
                _engMin = value;
                if (string.IsNullOrEmpty(_engMin))
                {
                    eng_min = null;
                    return;
                }
                double d;
                if (double.TryParse(_engMin, out d))
                {
                    eng_min = d;
                }
                else
                {
                    OnPropertyChanged(PropertyHelper.GetPropertyName(this, e => e.EngMin));
                }
            }
        }

        partial void Oneng_minChanged()
        {
            _engMin = string.Format("{0}", eng_min);
            OnPropertyChanged(PropertyHelper.GetPropertyName(this, e => e.EngMin));
        }

        #endregion

        #region EngMax

        private string _engMax;

        public string EngMax
        {
            get { return _engMax; }
            set
            {
                _engMax = value;
                if (string.IsNullOrEmpty(_engMax))
                {
                    eng_max = null;
                    return;
                }
                double d;
                if (double.TryParse(_engMax, out d))
                {
                    eng_max = d;
                }
                else
                {
                    OnPropertyChanged(PropertyHelper.GetPropertyName(this, e => e.EngMax));
                }
            }
        }

        partial void Oneng_maxChanged()
        {
            _engMax = string.Format("{0}", eng_max);
            OnPropertyChanged(PropertyHelper.GetPropertyName(this, e => e.EngMax));
        }

        #endregion

        #region dic_a_tag_types

        partial void  Onref_tag_typesChanged()
        {
            OnPropertyChanged(PropertyHelper.GetPropertyName(this, e => e.num_tag_types));
        }

        #endregion

        #region IDataErrorInfo and INotifyPropertyChanged

        public string this[string columnName]
        {
            get { return this.GetError(columnName); }
        }

        public string Error { get { return this.GetError(); } }

        protected override void OnPropertyChanged(string property)
        {
            base.OnPropertyChanged(property);
            var errorPropertyName = PropertyHelper.GetPropertyName(this, e => e.Error);
            if (property != errorPropertyName)
                OnPropertyChanged(errorPropertyName);
        }

        #endregion

        public Type TagType
        {
            get
            {
                switch (num_tag_types.name)
                {
                    case "Int16":
                        return typeof (Int16);
                    case "Int32":
                        return typeof (Int32);
                    case "UInt16":
                        return typeof (UInt16);
                    case "UInt32":
                        return typeof (UInt32);
                    case "Single":
                        return typeof (Single);
                    case "Double":
                        return typeof (Double);
                }
                throw new NotSupportedException(string.Format(Res.TypeNotSupportedMessage, num_tag_types.name));
            }
        }
    }

    [MetadataType(typeof(SettingsMetadata))]
    public partial class settings : IDataErrorInfo
    {
        public class SettingsMetadata : EntityMetadataBase
        {
            private string _server;
            private string _standByServer;

            [LocDisplayName("Server", ResourceType = typeof(Res))]
            [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Res))]
            [RegularExpression(RegexPatterns.Server, ErrorMessageResourceName = "ServerNameMessage", ErrorMessageResourceType = typeof(Res))]
            public string server
            {
                get { return _server; }
                set
                {
                    _server = value;
                    OnPropertyChanged(this, m => m.server);
                }
            }

            [LocDisplayName("StandByServer", ResourceType = typeof(Res))]
            [RegularExpression(RegexPatterns.Server, ErrorMessageResourceName = "ServerNameMessage", ErrorMessageResourceType = typeof(Res))]
            public string stand_by_server
            {
                get { return _standByServer; }
                set
                {
                    _standByServer = value;
                    OnPropertyChanged(this, m => m.stand_by_server);
                }
            }
        }

        #region IDataErrorInfo and INotifyPropertyChanged

        public string this[string columnName]
        {
            get { return this.GetError(columnName); }
        }

        public string Error { get { return this.GetError(); } }

        protected override void OnPropertyChanged(string property)
        {
            base.OnPropertyChanged(property);
            var errorPropertyName = PropertyHelper.GetPropertyName(this, e => e.Error);
            if (property != errorPropertyName)
                OnPropertyChanged(errorPropertyName);
        }

        #endregion
    }

    [MetadataType(typeof(TrendCategoriesMetadata))]
    public partial class trend_categories : IDataErrorInfo, INamedEntity, IRepository
    {
        public trend_categories()
        {
            time_to_store = TimeSpan.FromDays(31).Ticks;
            period = TimeSpan.FromSeconds(5).Ticks;
        }

        public class TrendCategoriesMetadata : EntityMetadataBase
        {
            private string _name;
            private string _timeToStore;
            private string _period;

            [LocDisplayName("Name", ResourceType = typeof(Res))]
            [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Res))]
            [RegularExpression(RegexPatterns.VariableName, ErrorMessageResourceName = "DotNetNameMessage", ErrorMessageResourceType = typeof(Res))]
            public string name
            {
                get { return _name; }
                set
                {
                    _name = value;
                    OnPropertyChanged(this, m => m.name);
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

        public string Period
        {
            get { return _periodStr; }
            set
            {
                _periodStr = value;
                TimeSpan d;
                if (TimeSpan.TryParse(_periodStr, out d))
                {
                    period = d.Ticks;
                }
                else
                {
                    OnPropertyChanged(PropertyHelper.GetPropertyName(this, e => e.Period));
                }
            }
        }

        partial void OnperiodChanged()
        {
            _periodStr = new TimeSpan(period).ToString();
            OnPropertyChanged(PropertyHelper.GetPropertyName(this, e => e.Period));
        }

        #endregion

        #region TimeToStore

        private string _timeToStore;

        public string TimeToStore
        {
            get { return _timeToStore; }
            set
            {
                _timeToStore = value;
                if (string.IsNullOrEmpty(_timeToStore))
                {
                    time_to_store = null;
                    return;
                }
                TimeSpan d;
                if (TimeSpan.TryParse(_timeToStore, out d))
                {
                    time_to_store = d.Ticks;
                }
                else
                {
                    OnPropertyChanged(PropertyHelper.GetPropertyName(this, e => e.TimeToStore));
                }
            }
        }

        partial void Ontime_to_storeChanged()
        {
            _timeToStore = time_to_store.HasValue ? new TimeSpan(time_to_store.Value).ToString() : null;
            OnPropertyChanged(PropertyHelper.GetPropertyName(this, e => e.TimeToStore));
        }

        #endregion

        #region IDataErrorInfo and INotifyPropertyChanged

        public string this[string columnName]
        {
            get { return this.GetError(columnName); }
        }

        public string Error { get { return this.GetError(); } }

        protected override void OnPropertyChanged(string property)
        {
            base.OnPropertyChanged(property);
            var errorPropertyName = PropertyHelper.GetPropertyName(this, e => e.Error);
            if (property != errorPropertyName)
                OnPropertyChanged(errorPropertyName);
        }

        #endregion

        public ICollection<T> GetRepository<T>()
        {
            if (typeof(T) == typeof(trend_tags))
            {
                return trend_tags as ICollection<T>;
            }
            throw new NotSupportedException();
        }
    }

    [MetadataType(typeof(TrendTagsMetadata))]
    public partial class trend_tags : IDataErrorInfo, INamedEntity
    {
        public class TrendTagsMetadata : EntityMetadataBase
        {
            private string _name;
            private num_tags _numTags;
            private dig_tags _digTags;
            private string _description;

            [LocDisplayName("Name", ResourceType = typeof(Res))]
            [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Res))]
            [RegularExpression(RegexPatterns.VariableName, ErrorMessageResourceName = "DotNetNameMessage", ErrorMessageResourceType = typeof(Res))]
            public string name
            {
                get { return _name; }
                set
                {
                    _name = value;
                    OnPropertyChanged(this, m => m.name);
                }
            }

            [LocDisplayName("NumericTag", ResourceType = typeof(Res))]
            [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Res))]
            public num_tags num_tags
            {
                get { return _numTags; }
                set
                {
                    _numTags = value;
                    OnPropertyChanged(this, m => m.num_tags);
                }
            }

            [LocDisplayName("Trigger", ResourceType = typeof(Res))]
            public dig_tags dig_tags
            {
                get { return _digTags; }
                set
                {
                    _digTags = value;
                    OnPropertyChanged(this, m => m.dig_tags);
                }
            }

            [LocDisplayName("Description", ResourceType = typeof(Res))]
            public string description
            {
                get { return _description; }
                set
                {
                    _description = value;
                    OnPropertyChanged(this, m => m.description);
                }
            }
        }

        partial void Onref_num_tagsChanged()
        {
            OnPropertyChanged(PropertyHelper.GetPropertyName(this, t => t.num_tags));
        }

        partial void Onref_triggersChanged()
        {
            OnPropertyChanged(PropertyHelper.GetPropertyName(this, t => t.dig_tags));
        }

        #region IDataErrorInfo and INotifyPropertyChanged

        public string this[string columnName]
        {
            get { return this.GetError(columnName); }
        }

        public string Error { get { return this.GetError(); } }

        protected override void OnPropertyChanged(string property)
        {
            base.OnPropertyChanged(property);
            var errorPropertyName = PropertyHelper.GetPropertyName(this, e => e.Error);
            if (property != errorPropertyName)
                OnPropertyChanged(errorPropertyName);
        }

        #endregion
    }

    [MetadataType(typeof(UsersMetadata))]
    public partial class users : IDataErrorInfo, INamedEntity
    {
        public users()
        {
            enabled = true;
        }

        public class UsersMetadata : EntityMetadataBase
        {
            private string _name;
            private byte[] _photo;
            private string _description;
            private int? _privilege;
            private bool _canChange;
            private bool _enabled;
            private string _password;

            [LocDisplayName("Name", ResourceType = typeof(Res))]
            [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Res))]
            public string name
            {
                get { return _name; }
                set
                {
                    _name = value;
                    OnPropertyChanged(this, m => m.name);
                }
            }

            [LocDisplayName("Photo", ResourceType = typeof(Res))]
            public byte[] photo
            {
                get { return _photo; }
                set
                {
                    _photo = value;
                    OnPropertyChanged(this, m => m.photo);
                }
            }

            [LocDisplayName("Description", ResourceType = typeof(Res))]
            [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Res))]
            public string description
            {
                get { return _description; }
                set
                {
                    _description = value;
                    OnPropertyChanged(this, m => m.description);
                }
            }

            [LocDisplayName("Password", ResourceType = typeof(Res))]
            public string password
            {
                get { return _password; }
                set
                {
                    _password = value;
                    OnPropertyChanged(this, m => m.password);
                }
            }

            [LocDisplayName("UserEnabled", ResourceType = typeof(Res))]
            public bool enabled
            {
                get { return _enabled; }
                set
                {
                    _enabled = value;
                    OnPropertyChanged(this, m => m.enabled);
                }
            }

            [LocDisplayName("CanChange", ResourceType = typeof(Res))]
            public bool can_change
            {
                get { return _canChange; }
                set
                {
                    _canChange = value;
                    OnPropertyChanged(this, m => m.can_change);
                }
            }

            [LocDisplayName("Privilege", ResourceType = typeof(Res))]
            public int? privilege
            {
                get { return _privilege; }
                set
                {
                    _privilege = value;
                    OnPropertyChanged(this, m => m.privilege);
                }
            }
        }

        #region IDataErrorInfo and INotifyPropertyChanged

        public string this[string columnName]
        {
            get { return this.GetError(columnName); }
        }

        public string Error { get { return this.GetError(); } }

        protected override void OnPropertyChanged(string property)
        {
            base.OnPropertyChanged(property);
            var errorPropertyName = PropertyHelper.GetPropertyName(this, e => e.Error);
            if (property != errorPropertyName)
                OnPropertyChanged(errorPropertyName);
        }

        #endregion
    }
}
