using System.Collections.Generic;
using PHmiModel;

namespace PHmiConfigurator.Dialogs
{
    public class EditNumericTagViewModel : EditDialogViewModel<num_tags.NumTagsMetadata>
    {
        private IEnumerable<num_tag_types> _numTagTypes;

        public IEnumerable<num_tag_types> NumTagTypes
        {
            get { return _numTagTypes; }
            set
            {
                _numTagTypes = value;
                OnPropertyChanged(this, v => v.NumTagTypes);
            }
        }
    }
}
