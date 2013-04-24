using System.Collections.Generic;
using PHmiModel;

namespace PHmiConfigurator.Dialogs
{
    public class EditAlarmTagViewModel : EditDialogViewModel<alarm_tags.AlarmTagsMetadata>
    {
        private IEnumerable<dig_tags> _digitalTags;
        public IEnumerable<dig_tags> DigitalTags
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
