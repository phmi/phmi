using PHmiClient.Controls;
using PHmiClient.Controls.Pages;

namespace PHmiClientSample.Pages
{
    /// <summary>
    /// Interaction logic for TagPage.xaml
    /// </summary>
    public partial class TagPage : IPage
    {
        public TagPage()
        {
            InitializeComponent();
        }

        public IRoot Root { get; set; }

        public object PageName { get { return "Tags"; } }
    }
}
