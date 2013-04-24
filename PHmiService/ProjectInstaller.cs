using PHmiService.Utils;
using System.ComponentModel;

namespace PHmiService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
            var postgresNames = ServiceHelper.GetServiceNames("postgresql");
            if (postgresNames != null)
            {
                serviceInstaller1.ServicesDependedOn = postgresNames;
            } 
        }
    }
}
