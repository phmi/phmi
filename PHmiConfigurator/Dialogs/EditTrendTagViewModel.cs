using System.Collections.Generic;
using PHmiModel;
using PHmiModel.Entities;

namespace PHmiConfigurator.Dialogs
{
    public class EditTrendTagViewModel : EditDialogViewModel<TrendTag.TrendTagMetadata>
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

        private IEnumerable<NumTag> _numericTags;
        public IEnumerable<NumTag> NumericTags
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
