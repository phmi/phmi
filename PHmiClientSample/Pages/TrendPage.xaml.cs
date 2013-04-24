using System.Windows.Data;
using PHmiClient.Controls;
using PHmiClient.Controls.Pages;
using PHmiClient.Controls.Trends;

namespace PHmiClientSample.Pages
{
    /// <summary>
    /// Interaction logic for TrendPage.xaml
    /// </summary>
    public partial class TrendPage : IPage
    {
        public TrendPage()
        {
            InitializeComponent();
        }

        public IRoot Root { get; set; }

        public object PageName { get { return "Trend"; } }
    }
}
