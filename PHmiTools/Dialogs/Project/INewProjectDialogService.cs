using PHmiClient.Utils;
using PHmiTools.Utils.Npg;

namespace PHmiTools.Dialogs.Project
{
    public interface INewProjectDialogService : IProjectDialogService
    {
        INpgHelper NpgHelper { get; }
        IActionHelper ActionHelper { get; }
        INpgScriptHelper ScriptHelper { get; }
    }
}
