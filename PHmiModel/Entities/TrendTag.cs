using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PHmiClient.Utils;
using PHmiResources;
using PHmiResources.Loc;

namespace PHmiModel.Entities
{
    [MetadataType(typeof(TrendTagMetadata))]
    [Table("trend_tags", Schema = "public")]
    public class TrendTag : NamedEntity
    {
        public class TrendTagMetadata : EntityMetadataBase
        {
            private string _name;
            private NumTag _numTag;
            private DigTag _trigger;
            private string _description;

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

            [LocDisplayName("NumericTag", ResourceType = typeof(Res))]
            [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Res))]
            public NumTag NumTag
            {
                get { return _numTag; }
                set
                {
                    _numTag = value;
                    OnPropertyChanged(this, m => m.NumTag);
                }
            }

            [LocDisplayName("Trigger", ResourceType = typeof(Res))]
            public DigTag Trigger
            {
                get { return _trigger; }
                set
                {
                    _trigger = value;
                    OnPropertyChanged(this, m => m.Trigger);
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
        }

        #region RefNumTag

        private int _refNumTag;

        [Column("ref_num_tags")]
        public int RefNumTag
        {
            get { return _refNumTag; }
            set
            {
                _refNumTag = value;
                OnPropertyChanged(this, t => t.NumTag);
                OnPropertyChanged(this, e => e.RefNumTag);
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

        #region RefCategory

        private int _refCategory;

        [Column("ref_categories")]
        public int RefCategory
        {
            get { return _refCategory; }
            set
            {
                _refCategory = value;
                OnPropertyChanged(this, e => e.RefCategory);
            }
        }

        #endregion

        #region RefTrigger

        private int? _refTrigger;

        [Column("ref_triggers")]
        public int? RefTrigger
        {
            get { return _refTrigger; }
            set
            {
                _refTrigger = value;
                OnPropertyChanged(this, t => t.Trigger);
                OnPropertyChanged(this, e => e.RefTrigger);
            }
        }

        #endregion

        #region Trigger

        private DigTag _trigger;

        public virtual DigTag Trigger
        {
            get { return _trigger; }
            set
            {
                _trigger = value;
                OnPropertyChanged(this, e => e.Trigger);
            }
        }

        #endregion

        #region NumTag

        private NumTag _numTag;

        public virtual NumTag NumTag
        {
            get { return _numTag; }
            set
            {
                _numTag = value;
                OnPropertyChanged(this, e => e.NumTag);
            }
        }

        #endregion

        #region TrendCategory

        private TrendCategory _trendCategory;

        public virtual TrendCategory TrendCategory
        {
            get { return _trendCategory; }
            set
            {
                _trendCategory = value;
                OnPropertyChanged(this, e => e.TrendCategory);
            }
        }

        #endregion
    }
}
