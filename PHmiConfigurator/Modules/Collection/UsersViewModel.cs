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
    public class UsersViewModel : CollectionViewModel<PHmiModel.Entities.User, PHmiModel.Entities.User.UserMetadata>
    {
        public UsersViewModel() : base(null)
        {
        }

        public override string Name
        {
            get { return Res.Users; }
        }

        protected override IEditDialog<PHmiModel.Entities.User.UserMetadata> CreateAddDialog()
        {
            return new EditUser
                {
                    Title = Res.AddUser,
                    Owner = Window.GetWindow(View),
                    Icon = IconHelper.GetIcon(ImagesUries.AddUserIco)
                };
        }

        protected override IEditDialog<PHmiModel.Entities.User.UserMetadata> CreateEditDialog()
        {
            return new EditUser
                {
                    Title = Res.EditUser,
                    Owner = Window.GetWindow(View),
                    Icon = IconHelper.GetIcon(ImagesUries.EditUserIco)
                };
        }

        protected override string[] GetCopyData(PHmiModel.Entities.User item)
        {
            return new []
                {
                    item.Description,
                    item.Enabled.ToString(CultureInfo.InvariantCulture),
                    item.CanChange.ToString(CultureInfo.InvariantCulture),
                    Int32ToPrivilegedConverter.Convert(item.Privilege)
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

        protected override void SetCopyData(PHmiModel.Entities.User item, string[] data)
        {
            item.Description = data[0];
            item.Enabled = bool.Parse(data[1]);
            item.CanChange = bool.Parse(data[2]);
            item.Privilege = Int32ToPrivilegedConverter.ConvertBack(data[3]);
        }
    }
}
