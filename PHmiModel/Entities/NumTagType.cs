using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PHmiModel.Entities
{
    [Table("num_tag_types", Schema = "public")]
    public class NumTagType : NamedEntity
    {
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
