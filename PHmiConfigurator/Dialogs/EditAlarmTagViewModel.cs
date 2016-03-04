using System.Collections.Generic;
using PHmiModel;
using PHmiModel.Entities;

namespace PHmiConfigurator.Dialogs
{
    public class EditAlarmTagViewModel : EditDialogViewModel<AlarmTag.AlarmTagMetadata>
    {
        private IEnumerable<DigTag> _digitalTags;
        public IEnumerable<DigTag> DigitalTags
        {
            get { return _digitalTags; }
            set
            {
                _digitalTags = value;
                OnPropertyChanged(this, v => v.DigitalTags);
            }
        }
    }
}
