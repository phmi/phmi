using System.Windows;
using PHmiClient.Utils;
using PHmiConfigurator.Dialogs;
using PHmiModel;
using PHmiResources.Loc;

namespace PHmiConfigurator.Modules.Collection
{
    public class AlarmCategoriesViewModel : CollectionViewModel<alarm_categories, alarm_categories.AlarmCategoriesMetadata>
    {
        public AlarmCategoriesViewModel() : base(null)
        {
        }

        public override string Name
        {
            get { return Res.AlarmCategories; }
        }

        protected override IEditDialog<alarm_categories.AlarmCategoriesMetadata> CreateAddDialog()
        {
            return new EditAlarmCategory { Title = Res.AddAlarmCategory, Owner = Window.GetWindow(View) };
        }

        protected override IEditDialog<alarm_categories.AlarmCategoriesMetadata> CreateEditDialog()
        {
            return new EditAlarmCategory { Title = Res.EditAlarmCategory, Owner = Window.GetWindow(View) };
        }

        protected override string[] GetCopyData(alarm_categories item)
        {
            return new []
                {
                    item.description,
                    item.TimeToStore
                };
        }

        protected override string[] GetCopyHeaders()
        {
            return new []
                {
                    ReflectionHelper.GetDisplayName<alarm_categories>(a => a.description),
                    ReflectionHelper.GetDisplayName<alarm_categories>(a => a.TimeToStore)
                };
        }

        protected override void SetCopyData(alarm_categories item, string[] data)
        {
            item.description = data[0];
            item.TimeToStore = data[1];
        }
    }
}
