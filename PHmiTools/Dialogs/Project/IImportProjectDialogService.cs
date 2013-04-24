using PHmiClient.Utils;
using PHmiTools.Utils.Npg;
using PHmiTools.Utils.Npg.ExIm;

namespace PHmiTools.Dialogs.Project
{
    public interface IImportProjectDialogService : IProjectDialogService
    {
        INpgHelper NpgHelper { get; }
        IActionHelper ActionHelper { get; }
        INpgScriptHelper ScriptHelper { get; }
        INpgExImHelper ExImHelper { get; }
    }
}
