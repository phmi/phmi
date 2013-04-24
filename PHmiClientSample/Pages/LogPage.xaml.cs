using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using PHmiClient.Controls;
using PHmiClient.Controls.Pages;
using PHmiClient.Logs;
using PHmiClient.Utils.Pagination;

namespace PHmiClientSample.Pages
{
    /// <summary>
    /// Interaction logic for LogPage.xaml
    /// </summary>
    public partial class LogPage : IPage
    {
        private LogAbstract _log;
        private readonly ObservableCollection<LogItem> _logs = new ObservableCollection<LogItem>();

        public LogPage()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            lb.ItemsSource = _logs;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var pHmi = (PHmi) DataContext;
            if (pHmi != null)
                _log = pHmi.Log;
        }

        public IRoot Root { get; set; }

        public object PageName
        {
            get { return "Log"; }
        }

        private void BSaveClick(object sender, RoutedEventArgs e)
        {
            var item = new LogItem {Text = tbMessage.Text};
            item.SetToBytes(tbMessage2.Text);
            _log.Save(item);
            _logs.Add(item);
        }

        private void BRefreshClick(object sender, RoutedEventArgs e)
        {
            _log.GetItems(CriteriaType.DownFromInfinity, new DateTime(), 1000, true, items => 
                Dispatcher.Invoke(new Action(() => Draw(items))));
        }

        private void Draw(IEnumerable<LogItem> items)
        {
            _logs.Clear();
            foreach (var logItem in items)
            {
                _logs.Add(logItem);
            }
        }

        private void BUpdateClick(object sender, RoutedEventArgs e)
        {
            var logItem = lb.SelectedItem as LogItem;
            if (logItem == null)
                return;
            logItem.Text = tbMessage.Text;
            logItem.SetToBytes(tbMessage2.Text);
            _log.Save(logItem);
        }

        private void BDeleteClick(object sender, RoutedEventArgs e)
        {
            var logItem = lb.SelectedItem as LogItem;
            if (logItem == null)
                return;
            _log.Delete(logItem);
        }
    }
}
