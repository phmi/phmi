using PHmiClient.Utils;
using PHmiTools.Utils;
using PHmiTools.Utils.Npg;
using PHmiTools.Utils.Npg.ExIm;

namespace PHmiTools.Dialogs.Project
{
    public interface IExportProjectDialogService
    {
        IActionHelper ActionHelper { get; }
        INpgExImHelper ExImHelper { get; }
        IDialogHelper DialogHelper { get; }
        INpgScriptHelper ScriptHelper { get; }
        INpgHelper NpgHelper { get; }
    }
}
