using System.Linq;
using PHmiModel;
using PHmiResources.Loc;

namespace PHmiConfigurator.Modules
{
    public class SettingsViewModel : ModuleViewModel
    {
        private settings _settings;

        public SettingsViewModel() : base(null)
        {
        }

        public override string Name
        {
            get { return Res.Settings; }
        }

        public override string Error
        {
            get { return Settings == null ? string.Empty : Settings.Error; }
        }

        protected override void PostReloadAction()
        {
            Settings = Context.Get<settings>().Single();
        }

        public settings Settings
        {
            get { return _settings; }
            set
            {
                _settings = value;
                OnPropertyChanged(this, v => v.Settings);
            }
        }
    }
}
