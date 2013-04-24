using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using PHmiClient.Utils.Pagination;

namespace PHmiClient.Controls
{
    public abstract class PaginatorControl : Control
    {
        private IPaginator _paginator;
        private readonly DispatcherTimer _timer;
        private bool _loaded;

        protected PaginatorControl()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            SizeChanged += OnSizeChanged;
            _timer = new DispatcherTimer();
            _timer.Tick += TimerTick;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _loaded = true;
            UpdatePageSize();
            CheckAutoRefresh();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _loaded = false;
        }

        private void CheckAutoRefresh()
        {
            if (AutoRefresh)
            {
                _timer.Start();
            }
        }

        private void TimerTick(object sender, EventArgs e)
        {
            _timer.Stop();
            if (_loaded && _paginator != null && _paginator.RefreshCommand.CanExecute(null))
            {
                _paginator.RefreshCommand.Execute(null);
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdatePageSize();
        }

        protected void SetPaginator(IPaginator paginator)
        {
            _paginator = paginator;
            _paginator.RefreshCommand.CanExecuteChanged += RefreshCommandCanExecuteChanged;
            _paginator.PropertyChanged += PaginatorPropertyChanged;
            UpdatePageSize();
        }

        private void RefreshCommandCanExecuteChanged(object sender, EventArgs e)
        {
            if (_paginator.RefreshCommand.CanExecute(null))
            {
                CheckAutoRefresh();
            }
        }

        private void PaginatorPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Busy")
                return;
            if (_paginator.Busy)
            {
                _timer.Stop();
            }
            else
            {
                CheckAutoRefresh();
            }
        }

        #region ItemHeight
        
        public double ItemHeight
        {
            get { return (double)GetValue(ItemHeightProperty); }
            set { SetValue(ItemHeightProperty, value); }
        }

        public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(
            "ItemHeight",
            typeof(double),
            typeof(PaginatorControl),
            new PropertyMetadata(10.0, OnItemHeightPropertyChanged));

        private static void OnItemHeightPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = (PaginatorControl)d;
            c.UpdatePageSize();
        }

        #endregion

        #region ControlHeightWithoutItemContainer
        
        public double ControlHeightWithoutItemContainer
        {
            get { return (double)GetValue(ControlHeightWithoutItemContainerProperty); }
            set { SetValue(ControlHeightWithoutItemContainerProperty, value); }
        }

        public static readonly DependencyProperty ControlHeightWithoutItemContainerProperty = DependencyProperty.Register(
            "ControlHeightWithoutItemContainer",
            typeof(double),
            typeof(PaginatorControl),
            new PropertyMetadata(0.0, OnControlHeightWithoutItemContainerPropertyChanged));

        private static void OnControlHeightWithoutItemContainerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = (PaginatorControl) d;
            c.UpdatePageSize();
        }

        #endregion

        private void UpdatePageSize()
        {
            if (_paginator == null)
                return;

            if (ActualHeight <= 0)
                return;

            var oldPageSize = _paginator.PageSize;
            _paginator.PageSize = (int) Math.Max((ActualHeight - ControlHeightWithoutItemContainer)/ItemHeight, 0);

            if (_paginator.PageSize > oldPageSize)
            {
                _timer.Stop();
                _timer.Start();
            }
        }

        #region AutoRefresh
        
        public bool AutoRefresh
        {
            get { return (bool)GetValue(AutoRefreshProperty); }
            set { SetValue(AutoRefreshProperty, value); }
        }

        public static readonly DependencyProperty AutoRefreshProperty =
            DependencyProperty.Register("AutoRefresh", typeof(bool), typeof(PaginatorControl),
            new PropertyMetadata(false, AutoRefreshPropertyChanged));

        private static void AutoRefreshPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var p = (PaginatorControl) d;
            p.CheckAutoRefresh();
        }

        #endregion
    }
}
