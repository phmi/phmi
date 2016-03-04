using System.Collections.Generic;
using PHmiModel;
using PHmiModel.Entities;

namespace PHmiConfigurator.Dialogs
{
    public class EditNumericTagViewModel : EditDialogViewModel<NumTag.NumTagMetadata>
    {
        private IEnumerable<NumTagType> _numTagTypes;

        public IEnumerable<NumTagType> NumTagTypes
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
