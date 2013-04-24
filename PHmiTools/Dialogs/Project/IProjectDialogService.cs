using PHmiClient.Utils;
using PHmiTools.Utils;
using PHmiTools.Utils.Npg;

namespace PHmiTools.Dialogs.Project
{
    public interface IProjectDialogService
    {
        IDialogHelper DialogHelper { get; }
        IConnectionStringHelper ConnectionStringHelper { get; }
        INpgConnectionParameters ConnectionParameters { get; }
    }
}
