using PHmiClient.Utils;
using PHmiTools.Utils;
using PHmiTools.Utils.Npg;

namespace PHmiTools.Dialogs.Project
{
    public interface IOpenProjectDialogService : IProjectDialogService
    {
        INpgHelper NpgHelper { get; }
        IActionHelper ActionHelper { get; }
        IPHmiDatabaseHelper DatabaseHelper { get; }
    }
}
