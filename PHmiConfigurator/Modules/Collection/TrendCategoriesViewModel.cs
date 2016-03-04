using System.Windows;
using PHmiClient.Utils;
using PHmiConfigurator.Dialogs;
using PHmiModel;
using PHmiResources.Loc;

namespace PHmiConfigurator.Modules.Collection
{
    public class TrendCategoriesViewModel : CollectionViewModel<PHmiModel.Entities.TrendCategory, PHmiModel.Entities.TrendCategory.TrendCategoryMetadata>
    {
        public TrendCategoriesViewModel() : base(null) { }

        public override string Name
        {
            get { return Res.TrendCategories; }
        }

        protected override IEditDialog<PHmiModel.Entities.TrendCategory.TrendCategoryMetadata> CreateAddDialog()
        {
            return new EditTrendCategory { Title = Res.AddTrendCategory, Owner = Window.GetWindow(View) };
        }

        protected override IEditDialog<PHmiModel.Entities.TrendCategory.TrendCategoryMetadata> CreateEditDialog()
        {
            return new EditTrendCategory { Title = Res.EditTrendCategory, Owner = Window.GetWindow(View) };
        }

        protected override string[] GetCopyData(PHmiModel.Entities.TrendCategory item)
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
                    ReflectionHelper.GetDisplayName<PHmiModel.Entities.TrendCategory>(a => a.TimeToStore)
                };
        }

        protected override void SetCopyData(PHmiModel.Entities.TrendCategory item, string[] data)
        {
            item.TimeToStore = data[0];
        }
    }
}
