using PHmiClient.Utils;
using PHmiModel.Interfaces;
using PHmiTools.Utils;

namespace PHmiConfigurator.Modules
{
    public interface IModuleService
    {
        IDialogHelper DialogHelper { get; }
        IModelContextFactory ContextFactory { get; }
        IEditorHelper EditorHelper { get; }
        IClipboardHelper ClipboardHelper { get; }
        IActionHelper ActionHelper { get; }
    }
}
