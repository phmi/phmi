using System;
using System.Windows;
using PHmiClient.Controls;
using PHmiClient.Controls.Pages;

namespace PHmiClientSample.Pages
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : IPage
    {
        public HomePage()
        {
            InitializeComponent();
        }

        private void ButtonClick1(object sender, RoutedEventArgs e)
        {
            var phmi = (PHmi) DataContext;
            phmi.Reporter.Report("wasup" + Guid.NewGuid().ToString());
        }

        public IRoot Root { get; set; }

        public object PageName { get { return "Home"; } }
    }
}
