using PHmiTools.Utils;

namespace PHmiConfigurator
{
    public class MainWindowService : IMainWindowService
    {
        private readonly IDialogHelper _dialogHelper = new DialogHelper();

        public IDialogHelper DialogHelper
        {
            get { return _dialogHelper; }
        }
    }
}
