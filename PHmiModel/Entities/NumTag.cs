using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PHmiClient.Utils;
using PHmiClient.Utils.ValidationAttributes;
using PHmiResources;
using PHmiResources.Loc;

namespace PHmiModel.Entities
{
    [MetadataType(typeof(NumTagMetadata))]
    [Table("num_tags", Schema = "public")]
    public class NumTag : NamedEntity
    {
        public NumTag()
        {
            CanRead = true;
        }

        public class NumTagMetadata : EntityMetadataBase
        {
            private string _name;
            private string _device;
            private string _description;
            private bool _canRead;
            private string _engUnit;
            private NumTagType _numTagType;
            private string _format;
            private string _rawMin;
            private string _rawMax;
            private string _engMin;
            private string _engMax;

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

            [LocDisplayName("TagType", ResourceType = typeof(Res))]
            [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Res))]
            public NumTagType NumTagType
            {
                get { return _numTagType; }
                set
                {
                    _numTagType = value;
                    OnPropertyChanged(this, i => i.NumTagType);
                }
            }

            [LocDisplayName("Format", ResourceType = typeof(Res))]
            public string Format
            {
                get { return _format; }
                set
                {
                    _format = value;
                    OnPropertyChanged(this, i => i.Format);
                }
            }

            [LocDisplayName("EngUnit", ResourceType = typeof(Res))]
            public string EngUnit
            {
                get { return _engUnit; }
                set
                {
                    _engUnit = value;
                    OnPropertyChanged(this, i => i.EngUnit);
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

        public string FullName { get { return IoDevice.Name + "." + Name; } }

        #region RawMin

        private string _rawMin;

        [NotMapped]
        public string RawMin
        {
            get { return _rawMin; }
            set
            {
                _rawMin = value;
                if (string.IsNullOrEmpty(_rawMin))
                {
                    RawMinDb = null;
                    return;
                }
                double d;
                if (double.TryParse(_rawMin, out d))
                {
                    RawMinDb = d;
                }
                else
                {
                    OnPropertyChanged(this, e => e.RawMin);
                }
            }
        }

        #endregion

        #region RawMax

        private string _rawMax;

        [NotMapped]
        public string RawMax
        {
            get { return _rawMax; }
            set
            {
                _rawMax = value;
                if (string.IsNullOrEmpty(_rawMax))
                {
                    RawMaxDb = null;
                    return;
                }
                double d;
                if (double.TryParse(_rawMax, out d))
                {
                    RawMaxDb = d;
                }
                else
                {
                    OnPropertyChanged(this, e => e.RawMax);
                }
            }
        }

        #endregion

        #region EngMin

        private string _engMin;

        [NotMapped]
        public string EngMin
        {
            get { return _engMin; }
            set
            {
                _engMin = value;
                if (string.IsNullOrEmpty(_engMin))
                {
                    EngMinDb = null;
                    return;
                }
                double d;
                if (double.TryParse(_engMin, out d))
                {
                    EngMinDb = d;
                }
                else
                {
                    OnPropertyChanged(this, e => e.EngMin);
                }
            }
        }

        #endregion

        #region EngMax

        private string _engMax;

        [NotMapped]
        public string EngMax
        {
            get { return _engMax; }
            set
            {
                _engMax = value;
                if (string.IsNullOrEmpty(_engMax))
                {
                    EngMaxDb = null;
                    return;
                }
                double d;
                if (double.TryParse(_engMax, out d))
                {
                    EngMaxDb = d;
                }
                else
                {
                    OnPropertyChanged(this, e => e.EngMax);
                }
            }
        }

        #endregion

        public Type TagType
        {
            get
            {
                switch (NumTagType.Name)
                {
                    case "Int16":
                        return typeof(Int16);
                    case "Int32":
                        return typeof(Int32);
                    case "UInt16":
                        return typeof(UInt16);
                    case "UInt32":
                        return typeof(UInt32);
                    case "Single":
                        return typeof(Single);
                    case "Double":
                        return typeof(Double);
                }
                throw new NotSupportedException(string.Format(Res.TypeNotSupportedMessage, NumTagType.Name));
            }
        }

        #region RefIoDevice

        private int _refIoDevice;

        [Column("ref_io_devices")]
        public int RefIoDevice
        {
            get { return _refIoDevice; }
            set
            {
                _refIoDevice = value;
                OnPropertyChanged(this, e => e.RefIoDevice);
            }
        }

        #endregion

        #region RefTagType

        private int _refTagType;

        [Column("ref_tag_types")]
        public int RefTagType
        {
            get { return _refTagType; }
            set
            {
                _refTagType = value;
                OnPropertyChanged(this, e => e.NumTagType);
                OnPropertyChanged(this, e => e.RefTagType);
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

        #region EngUnit

        private string _engUnit;

        [Column("eng_unit")]
        public string EngUnit
        {
            get { return _engUnit; }
            set
            {
                _engUnit = value;
                OnPropertyChanged(this, e => e.EngUnit);
            }
        }

        #endregion

        #region RawMinDb

        private double? _rawMinDb;

        [Column("raw_min")]
        public double? RawMinDb
        {
            get { return _rawMinDb; }
            set
            {
                _rawMinDb = value;
                _rawMin = string.Format("{0}", RawMinDb);
                OnPropertyChanged(this, e => e.RawMin);
                OnPropertyChanged(this, e => e.RawMinDb);
            }
        }

        #endregion

        #region RawMaxDb

        private double? _rawMaxDb;

        [Column("raw_max")]
        public double? RawMaxDb
        {
            get { return _rawMaxDb; }
            set
            {
                _rawMaxDb = value;
                _rawMax = string.Format("{0}", RawMaxDb);
                OnPropertyChanged(this, e => e.RawMax);
                OnPropertyChanged(this, e => e.RawMaxDb);
            }
        }

        #endregion

        #region EngMinDb

        private double? _engMinDb;

        [Column("eng_min")]
        public double? EngMinDb
        {
            get { return _engMinDb; }
            set
            {
                _engMinDb = value;
                _engMin = string.Format("{0}", EngMinDb);
                OnPropertyChanged(this, e => e.EngMin);
                OnPropertyChanged(this, e => e.EngMinDb);
            }
        }

        #endregion

        #region EngMaxDb

        private double? _engMaxDb;

        [Column("eng_max")]
        public double? EngMaxDb
        {
            get { return _engMaxDb; }
            set
            {
                _engMaxDb = value;
                _engMax = string.Format("{0}", EngMaxDb);
                OnPropertyChanged(this, e => e.EngMax);
                OnPropertyChanged(this, e => EngMaxDb);
            }
        }

        #endregion

        #region Format

        private string _format;

        [Column("format")]
        public string Format
        {
            get { return _format; }
            set
            {
                _format = value;
                OnPropertyChanged(this, e => e.Format);
            }
        }

        #endregion

        #region IoDevice

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

        #region NumTagType

        private NumTagType _numTagType;

        public virtual NumTagType NumTagType
        {
            get { return _numTagType; }
            set
            {
                _numTagType = value;
                OnPropertyChanged(this, e => e.NumTagType);
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
