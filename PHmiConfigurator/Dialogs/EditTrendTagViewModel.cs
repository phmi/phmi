using System.Collections.Generic;
using PHmiModel;

namespace PHmiConfigurator.Dialogs
{
    public class EditTrendTagViewModel : EditDialogViewModel<trend_tags.TrendTagsMetadata>
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

        private IEnumerable<num_tags> _numericTags;
        public IEnumerable<num_tags> NumericTags
        {
            get { return _numericTags; }
            set
            {
                _numericTags = value;
                OnPropertyChanged(this, v => v.NumericTags);
            }
        }
    }
}
