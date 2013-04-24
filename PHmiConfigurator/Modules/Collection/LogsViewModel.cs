using System;
using System.Windows;
using PHmiClient.Utils;
using PHmiConfigurator.Dialogs;
using PHmiModel;
using PHmiResources.Loc;

namespace PHmiConfigurator.Modules.Collection
{
    public class LogsViewModel : CollectionViewModel<logs, logs.LogsMetadata>
    {
        public LogsViewModel()
            : base(null)
        {
        }

        public override string Name
        {
            get { return Res.Logs; }
        }

        protected override IEditDialog<logs.LogsMetadata> CreateAddDialog()
        {
            return new EditLog { Title = Res.AddLog, Owner = Window.GetWindow(View) };
        }

        protected override IEditDialog<logs.LogsMetadata> CreateEditDialog()
        {
            return new EditLog { Title = Res.EditLog, Owner = Window.GetWindow(View) };
        }

        protected override string[] GetCopyData(logs item)
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
                    ReflectionHelper.GetDisplayName<logs>(a => a.TimeToStore)
                };
        }

        protected override void SetCopyData(logs item, string[] data)
        {
            item.TimeToStore = data[0];
        }
    }
}
