using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PHmiClient.Utils;
using PHmiResources;
using PHmiResources.Loc;

namespace PHmiModel.Entities
{
    [MetadataType(typeof(DigTagMetadata))]
    [Table("dig_tags", Schema = "public")]
    public class DigTag : NamedEntity
    {
        public DigTag()
        {
            CanRead = true;
        }

        public class DigTagMetadata : EntityMetadataBase
        {
            private string _name;
            private string _device;
            private string _description;
            private bool _canRead;

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

            [LocDisplayName("AddressInDevice", ResourceType = typeof(Res))]
            [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Res))]
            public string Device
            {
                get { return _device; }
                set
                {
                    _device = value;
                    OnPropertyChanged(this, i => i.Device);
                }
            }

            [LocDisplayName("Description", ResourceType = typeof(Res))]
            public string Description
            {
                get { return _description; }
                set
                {
                    _description = value;
                    OnPropertyChanged(this, i => i.Description);
                }
            }

            [LocDisplayName("CanRead", ResourceType = typeof(Res))]
            public bool CanRead
            {
                get { return _canRead; }
                set
                {
                    _canRead = value;
                    OnPropertyChanged(this, i => i.CanRead);
                }
            }
        }

        public string FullName { get { return IoDevice.Name + "." + Name; } }

        #region RefIoDevices

        private int _refIoDevices;

        [Column("ref_io_devices")]
        public int RefIoDevices
        {
            get { return _refIoDevices; }
            set
            {
                _refIoDevices = value; 
                OnPropertyChanged(this, e => e.RefIoDevices);
            }
        }

        #endregion

        #region Device

        private string _device;

        [Column("device")]
        public string Device
        {
            get { return _device; }
            set
            {
                _device = value; 
                OnPropertyChanged(this, e => e.Device);
            }
        }

        #endregion

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

        #region CanRead

        private bool _canRead;

        [Column("can_read")]
        public bool CanRead
        {
            get { return _canRead; }
            set
            {
                _canRead = value; 
                OnPropertyChanged(this, e => e.CanRead);
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

        #region IoDevices

        private IoDevice _ioDevice;

        public virtual IoDevice IoDevice
        {
            get { return _ioDevice; }
            set
            {
                _ioDevice = value;
                OnPropertyChanged(this, e => e.IoDevice);
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
