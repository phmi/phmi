using System.Collections.Generic;
using PHmiClient.Utils;
using PHmiModel;
using PHmiModel.Entities;

namespace PHmiConfigurator.Dialogs
{
    /// <summary>
    /// Interaction logic for EditAlarmTag.xaml
    /// </summary>
    public partial class EditAlarmTag : IEditDialog<AlarmTag.AlarmTagMetadata>
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

        public AlarmTag.AlarmTagMetadata Entity
        {
            get { return ViewModel.Entity; }
            set { ViewModel.Entity = value; }
        }

        public IEnumerable<DigTag> DigitalTags
        {
            get { return ViewModel.DigitalTags; }
            set { ViewModel.DigitalTags = value; }
        }
    }
}
