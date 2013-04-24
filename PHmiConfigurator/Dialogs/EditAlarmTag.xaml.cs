using System.Collections.Generic;
using PHmiClient.Utils;
using PHmiModel;

namespace PHmiConfigurator.Dialogs
{
    /// <summary>
    /// Interaction logic for EditAlarmTag.xaml
    /// </summary>
    public partial class EditAlarmTag : IEditDialog<alarm_tags.AlarmTagsMetadata>
    {
        public EditAlarmTag()
        {
            this.UpdateLanguage();
            InitializeComponent();
            ViewModel.View = this;
            tbName.Focus();
        }

        public EditAlarmTagViewModel ViewModel
        {
            get { return (EditAlarmTagViewModel) Resources["ViewModel"]; }
        }

        public alarm_tags.AlarmTagsMetadata Entity
        {
            get { return ViewModel.Entity; }
            set { ViewModel.Entity = value; }
        }

        public IEnumerable<dig_tags> DigitalTags
        {
            get { return ViewModel.DigitalTags; }
            set { ViewModel.DigitalTags = value; }
        }
    }
}
