using System.Windows.Controls;

namespace PHmiClient.Controls.Trends
{
    /// <summary>
    /// Interaction logic for SliderPresenter.xaml
    /// </summary>
    public partial class SliderPresenter : UserControl
    {
        public SliderPresenter(TrendPen pen)
        {
            InitializeComponent();
            DataContext = pen;
        }
    }
}