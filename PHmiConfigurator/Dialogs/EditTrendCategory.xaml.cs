using PHmiClient.Utils;
using PHmiModel;

namespace PHmiConfigurator.Dialogs
{
    /// <summary>
    /// Interaction logic for EditTrendCategory.xaml
    /// </summary>
    public partial class EditTrendCategory : IEditDialog<trend_categories.TrendCategoriesMetadata>
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

        public trend_categories.TrendCategoriesMetadata Entity
        {
            get { return ViewModel.Entity; }
            set { ViewModel.Entity = value; }
        }
    }
}
