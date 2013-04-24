using System.Globalization;
using System.Windows;
using PHmiClient.Converters;
using PHmiClient.Utils;
using PHmiConfigurator.Dialogs;
using PHmiModel;
using PHmiResources;
using PHmiResources.Loc;

namespace PHmiConfigurator.Modules.Collection
{
    public class UsersViewModel : CollectionViewModel<users, users.UsersMetadata>
    {
        public UsersViewModel() : base(null)
        {
        }

        public override string Name
        {
            get { return Res.Users; }
        }

        protected override IEditDialog<users.UsersMetadata> CreateAddDialog()
        {
            return new EditUser
                {
                    Title = Res.AddUser,
                    Owner = Window.GetWindow(View),
                    Icon = IconHelper.GetIcon(ImagesUries.AddUserIco)
                };
        }

        protected override IEditDialog<users.UsersMetadata> CreateEditDialog()
        {
            return new EditUser
                {
                    Title = Res.EditUser,
                    Owner = Window.GetWindow(View),
                    Icon = IconHelper.GetIcon(ImagesUries.EditUserIco)
                };
        }

        protected override string[] GetCopyData(users item)
        {
            return new []
                {
                    item.description,
                    item.enabled.ToString(CultureInfo.InvariantCulture),
                    item.can_change.ToString(CultureInfo.InvariantCulture),
                    Int32ToPrivilegedConverter.Convert(item.privilege)
                };
        }

        protected override string[] GetCopyHeaders()
        {
            return new []
                {
                    Res.Description,
                    Res.UserEnabled,
                    Res.CanChange,
                    Res.Privilege
                };
        }

        protected override void SetCopyData(users item, string[] data)
        {
            item.description = data[0];
            item.enabled = bool.Parse(data[1]);
            item.can_change = bool.Parse(data[2]);
            item.privilege = Int32ToPrivilegedConverter.ConvertBack(data[3]);
        }
    }
}
