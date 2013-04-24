using System.Windows;
using PHmiClient.Utils;
using PHmiConfigurator.Dialogs;
using PHmiModel;
using PHmiResources.Loc;

namespace PHmiConfigurator.Modules.Collection
{
    public class TrendCategoriesViewModel : CollectionViewModel<trend_categories, trend_categories.TrendCategoriesMetadata>
    {
        public TrendCategoriesViewModel() : base(null) { }

        public override string Name
        {
            get { return Res.TrendCategories; }
        }

        protected override IEditDialog<trend_categories.TrendCategoriesMetadata> CreateAddDialog()
        {
            return new EditTrendCategory { Title = Res.AddTrendCategory, Owner = Window.GetWindow(View) };
        }

        protected override IEditDialog<trend_categories.TrendCategoriesMetadata> CreateEditDialog()
        {
            return new EditTrendCategory { Title = Res.EditTrendCategory, Owner = Window.GetWindow(View) };
        }

        protected override string[] GetCopyData(trend_categories item)
        {
            return new[]
                {
                    item.TimeToStore
                };
        }

        protected override string[] GetCopyHeaders()
        {
            return new[]
                {
                    ReflectionHelper.GetDisplayName<trend_categories>(a => a.TimeToStore)
                };
        }

        protected override void SetCopyData(trend_categories item, string[] data)
        {
            item.TimeToStore = data[0];
        }
    }
}
