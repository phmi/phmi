using PHmiClient.Utils;
using PHmiModel;
using PHmiModel.Interfaces;
using PHmiTools.Utils;

namespace PHmiConfigurator.Modules
{
    public class ModuleService : IModuleService
    {
        private readonly IDialogHelper _dialogHelper = new DialogHelper();

        private readonly IModelContextFactory _contextFactory = new PHmiModelContextFactory();

        private readonly IEditorHelper _editorHelper = new EditorHelper();

        private readonly IClipboardHelper _clipboardHelper = new ClipboardHelper();

        private readonly IActionHelper _actionHelper = new ActionHelper();

        public IDialogHelper DialogHelper
        {
            get { return _dialogHelper; }
        }

        public IModelContextFactory ContextFactory
        {
            get { return _contextFactory; }
        }

        public IEditorHelper EditorHelper
        {
            get { return _editorHelper; }
        }

        public IClipboardHelper ClipboardHelper
        {
            get { return _clipboardHelper; }
        }

        public IActionHelper ActionHelper
        {
            get { return _actionHelper; }
        }
    }
}
