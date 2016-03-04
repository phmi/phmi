using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PHmiClient.Utils;
using PHmiModel.Interfaces;
using PHmiResources;
using PHmiResources.Loc;

namespace PHmiModel.Entities
{
    [MetadataType(typeof(IoDeviceMetadata))]
    [Table("io_devices", Schema = "public")]
    public class IoDevice : NamedEntity, IRepository
    {
        public class IoDeviceMetadata : EntityMetadataBase
        {
            private string _name;
            private string _type;
            private string _options;

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

            [LocDisplayName("Type", ResourceType = typeof(Res))]
            [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Res))]
            public string Type
            {
                get { return _type; }
                set
                {
                    _type = value;
                    OnPropertyChanged(this, i => i.Type);
                }
            }

            [LocDisplayName("Options", ResourceType = typeof(Res))]
            public string Options
            {
                get { return _options; }
                set
                {
                    _options = value;
                    OnPropertyChanged(this, i => i.Options);
                }
            }
        }

        public ICollection<T> GetRepository<T>()
        {
            if (typeof(T) == typeof(DigTag))
                return DigTags as ICollection<T>;
            if (typeof(T) == typeof(NumTag))
                return NumTags as ICollection<T>;
            throw new NotSupportedException();
        }

        #region Type

        private string _type;

        [Column("type")]
        public string Type
        {
            get { return _type; }
            set
            {
                _type = value;
                OnPropertyChanged(this, e => e.Type);
            }
        }

        #endregion

        #region Options

        private string _options;

        [Column("options")]
        public string Options
        {
            get { return _options; }
            set
            {
                _options = value;
                OnPropertyChanged(this, e => e.Options);
            }
        }

        #endregion

        #region DigTags

        private ICollection<DigTag> _digTags;

        public virtual ICollection<DigTag> DigTags
        {
            get { return _digTags ?? (_digTags = new ObservableCollection<DigTag>()); }
            set { _digTags = value; }
        }

        #endregion

        #region NumTags

        private ICollection<NumTag> _numTags;

        public virtual ICollection<NumTag> NumTags
        {
            get { return _numTags ?? (_numTags = new ObservableCollection<NumTag>()); }
            set { _numTags = value; }
        }

        #endregion
    }
}
