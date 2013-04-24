using System.Windows;
using PHmiClient.Utils;
using PHmiClient.Utils.ViewInterfaces;

namespace PHmiTools.Dialogs.Project
{
    public class ProjectDialogBase : Window, IWindow
    {
        public ProjectDialogBase()
        {
            this.UpdateLanguage();
            Owner = Application.Current.MainWindow;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var viewModel = (ProjectDialogViewModel)Resources["ViewModel"];
            viewModel.View = this;
        }
    }
}
