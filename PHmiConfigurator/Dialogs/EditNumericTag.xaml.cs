using System.Collections.Generic;
using PHmiClient.Utils;
using PHmiModel;
using PHmiModel.Entities;

namespace PHmiConfigurator.Dialogs
{
    /// <summary>
    /// Interaction logic for EditNumericTag.xaml
    /// </summary>
    public partial class EditNumericTag : IEditDialog<NumTag.NumTagMetadata>
    {
        public EditNumericTag()
        {
            this.UpdateLanguage();
            InitializeComponent();
            ViewModel.View = this;
            tbName.Focus();
        }

        public EditNumericTagViewModel ViewModel
        {
            get { return (EditNumericTagViewModel)Resources["ViewModel"]; }
        }

        public NumTag.NumTagMetadata Entity
        {
            get { return ViewModel.Entity; }
            set { ViewModel.Entity = value; }
        }

        public IEnumerable<NumTagType> NumTagTypes
        {
            get { return ViewModel.NumTagTypes; }
            set { ViewModel.NumTagTypes = value; }
        }
    }
}
