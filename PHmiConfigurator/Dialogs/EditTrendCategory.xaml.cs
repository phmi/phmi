using PHmiClient.Utils;
using PHmiModel;
using PHmiModel.Entities;

namespace PHmiConfigurator.Dialogs
{
    /// <summary>
    /// Interaction logic for EditTrendCategory.xaml
    /// </summary>
    public partial class EditTrendCategory : IEditDialog<TrendCategory.TrendCategoryMetadata>
    {
        public EditTrendCategory()
        {
            this.UpdateLanguage();
            InitializeComponent();
            ViewModel.View = this;
            tbName.Focus();
        }

        public EditTrendCategoryViewModel ViewModel
        {
            get { return (EditTrendCategoryViewModel)Resources["ViewModel"]; }
        }

        public TrendCategory.TrendCategoryMetadata Entity
        {
            get { return ViewModel.Entity; }
            set { ViewModel.Entity = value; }
        }
    }
}
