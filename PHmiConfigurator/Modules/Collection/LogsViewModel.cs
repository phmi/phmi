using System;
using System.Windows;
using PHmiClient.Utils;
using PHmiConfigurator.Dialogs;
using PHmiModel;
using PHmiResources.Loc;

namespace PHmiConfigurator.Modules.Collection
{
    public class LogsViewModel : CollectionViewModel<PHmiModel.Entities.Log, PHmiModel.Entities.Log.LogMetadata>
    {
        public LogsViewModel()
            : base(null)
        {
        }

        public override string Name
        {
            get { return Res.Logs; }
        }

        protected override IEditDialog<PHmiModel.Entities.Log.LogMetadata> CreateAddDialog()
        {
            return new EditLog { Title = Res.AddLog, Owner = Window.GetWindow(View) };
        }

        protected override IEditDialog<PHmiModel.Entities.Log.LogMetadata> CreateEditDialog()
        {
            return new EditLog { Title = Res.EditLog, Owner = Window.GetWindow(View) };
        }

        protected override string[] GetCopyData(PHmiModel.Entities.Log item)
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
                    ReflectionHelper.GetDisplayName<PHmiModel.Entities.Log>(a => a.TimeToStore)
                };
        }

        protected override void SetCopyData(PHmiModel.Entities.Log item, string[] data)
        {
            item.TimeToStore = data[0];
        }
    }
}
