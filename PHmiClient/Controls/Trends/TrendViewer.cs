using System.Windows.Markup;
using PHmiClient.Controls.Input;
using PHmiClient.PHmiSystem;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Path = System.Windows.Shapes.Path;

namespace PHmiClient.Controls.Trends
{
    [TemplatePart(Name = "dgTrendPens", Type = typeof (DataGrid))]
    [TemplatePart(Name = "gTrendPresenters", Type = typeof (Grid))]
    [TemplatePart(Name = "gVerticalAxes", Type = typeof (Grid))]
    [TemplatePart(Name = "cVerticalAxes", Type = typeof (Canvas))]
    [TemplatePart(Name = "cHorizontalAxes", Type = typeof (Canvas))]
    [TemplatePart(Name = "spHorizontalAxes", Type = typeof (StackPanel))]
    [TemplatePart(Name = "slSlider", Type = typeof (Slider))]
    [TemplatePart(Name = "cdSliderOffset", Type = typeof (ColumnDefinition))]
    [TemplatePart(Name = "gSlider", Type = typeof (Grid))]
    [TemplatePart(Name = "cSlider", Type = typeof (Canvas))]
    [TemplatePart(Name = "spSlider", Type = typeof (Panel))]
    [TemplatePart(Name = "popupPeriod", Type = typeof (Popup))]
    [TemplatePart(Name = "tspSetPeriod", Type = typeof (TimeSpanPicker))]
    [TemplatePart(Name = "bSetPeriod", Type = typeof (Button))]
    [TemplatePart(Name = "popupTime", Type = typeof (Popup))]
    [TemplatePart(Name = "dtpSetTime", Type = typeof (DateTimePicker))]
    [TemplatePart(Name = "bSetTime", Type = typeof (Button))]
    [ContentProperty("Pens")]
    public class TrendViewer : Control
    {
        private PHmiAbstract _pHmi;

        private readonly Dictionary<TrendPen, TrendPresenter> _trendPresenters =
            new Dictionary<TrendPen, TrendPresenter>();

        private readonly Dictionary<TrendPen, Line> _sliderLines = new Dictionary<TrendPen, Line>();

        private readonly Dictionary<TrendPen, SliderPresenter> _sliderPresenters =
            new Dictionary<TrendPen, SliderPresenter>();

        private readonly Timer _timer = new Timer(1000);
        private readonly ObservableCollection<TrendPen> _trendPens = new ObservableCollection<TrendPen>();

        private Canvas _cHorizontalAxes;
        private Canvas _cSlider;
        private Canvas _cVerticalAxes;
        private ColumnDefinition _cdSliderOffset;
        private DataGrid _dgTrendPens;
        private DateTimePicker _dtpSetTime;
        private Grid _gTrendPresenters;
        private Grid _gSlider;
        private Grid _gVerticalAxes;
        private bool _loaded;
        private int _maxPeriodGain = 100000;
        private int _maxPoints = 200;
        private Popup _popupPeriod;
        private Popup _popupTime;
        private Slider _slSlider;
        private StackPanel _spHorisontalAxes;
        private Panel _spSlider;
        private DateTime _startTime = DateTime.Now - new TimeSpan(0, 0, 1, 0);
        private TimeSpanPicker _tspSetPeriod;

        static TrendViewer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (TrendViewer), new FrameworkPropertyMetadata(typeof (TrendViewer)));
        }

        public TrendViewer()
        {
            _timer.Elapsed += TimerElapsed;
            Loaded += TrendViewerLoaded;
            Unloaded += TrendViewerUnloaded;
            var minBinding = new Binding("CurrentTrendPen.MinScale") {Source = this};
            SetBinding(ScaleMinimumProperty, minBinding);
            var maxBinding = new Binding("CurrentTrendPen.MaxScale") {Source = this};
            SetBinding(ScaleMaximumProperty, maxBinding);
        }

        #region public properties

        #region Pens

        private readonly ObservableCollection<TrendPen> _pens = new ObservableCollection<TrendPen>();

        public ObservableCollection<TrendPen> Pens
        {
            get { return _pens; }
        }

        private void LoadPens()
        {
            TimeSpan? minimumPeriod = null;
            foreach (var trendPen in Pens)
            {
                AddPen(trendPen);
                if (minimumPeriod == null || minimumPeriod > trendPen.TrendTag.Category.Period)
                {
                    minimumPeriod = trendPen.TrendTag.Category.Period;
                }
            }
            Pens.CollectionChanged += PensCollectionChanged;
            if (minimumPeriod == null)
            {
                minimumPeriod = new TimeSpan(0, 0, 0, 1);
            }
            if (Period == null)
            {
                Period = new TimeSpan(_maxPoints/4*3*minimumPeriod.Value.Ticks);
            }
        }

        private void UnloadPens()
        {
            _pens.CollectionChanged -= PensCollectionChanged;
            foreach (var p in _pens)
            {
                RemovePen(p);
            }
        }

        private void PensCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (TrendPen p in e.NewItems)
                    {
                        AddPen(p);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (TrendPen p in e.OldItems)
                    {
                        RemovePen(p);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (var p in Pens.ToArray())
                    {
                        RemovePen(p);
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                    throw new NotSupportedException("Move and replace trend pens are not supported");
            }
        }

        #endregion

        public int MaxPoints
        {
            get { return _maxPoints; }
            set
            {
                _maxPoints = value;
                foreach (var trendPresenter in _trendPresenters.Values)
                {
                    trendPresenter.MaxPoints = _maxPoints;
                } 
            }
        }

        public int MaxPeriodGain
        {
            get { return _maxPeriodGain; }
            set { _maxPeriodGain = value; }
        }

        #endregion

        #region Add/Remove pen

        private void AddPen(TrendPen trendPen)
        {
            var trendTagBinding = BindingOperations.GetBinding(trendPen, TrendPen.TrendTagProperty);
            if (trendTagBinding != null
                && trendTagBinding.Source == null
                && trendTagBinding.RelativeSource == null
                && trendTagBinding.ElementName == null)
            {
                BindingOperations.ClearBinding(trendPen, TrendPen.TrendTagProperty);
                trendTagBinding = new Binding(trendTagBinding.Path.Path) {Source = DataContext};
                BindingOperations.SetBinding(trendPen, TrendPen.TrendTagProperty, trendTagBinding);
            }
            var trendPresenter = new TrendPresenter(trendPen) { MaxPoints = _maxPoints };
            var showPointsBinding = new Binding("ShowPoints") { Source = this };
            trendPresenter.SetBinding(TrendPresenter.ShowPointsProperty, showPointsBinding);
            _trendPresenters.Add(trendPen, trendPresenter);
            _gTrendPresenters.Children.Add(trendPresenter);
            _trendPens.Add(trendPen);

            #region Slider

            var sliderPresenter = new SliderPresenter(trendPen);
            _sliderPresenters.Add(trendPen, sliderPresenter);
            sliderPresenter.SizeChanged += SliderPresenterSizeChanged;
            sliderPresenter.LayoutUpdated += SliderPresenterLayoutUpdated;
            _spSlider.Children.Add(sliderPresenter);
            var sliderPresenterVisibilityBinding = new Binding("Visible")
                {
                    Source = trendPen,
                    Converter = new BooleanToVisibilityConverter()
                };
            sliderPresenter.SetBinding(VisibilityProperty, sliderPresenterVisibilityBinding);
            var sliderLine = new Line();
            _sliderLines.Add(trendPen, sliderLine);
            sliderLine.Stroke = _sliderLinesBrush;
            sliderLine.StrokeThickness = _sliderLinesThickness;
            var cursorCoordinateBinding = new Binding("CursorCoordinate") { Source = trendPresenter };
            sliderLine.SetBinding(Line.Y1Property, cursorCoordinateBinding);
            var sliderLineVisibilityBinding = new MultiBinding {Converter = new SliderLineVisibilityConverter()};
            sliderLineVisibilityBinding.Bindings.Add(new Binding("Visible") { Source = trendPen });
            sliderLineVisibilityBinding.Bindings.Add(new Binding("CursorCoordinate") { Source = trendPresenter });
            sliderLine.SetBinding(VisibilityProperty, sliderLineVisibilityBinding);

            UpdateSliderLine(sliderLine, trendPen);
            _cSlider.Children.Add(sliderLine);

            #endregion
        }

        private void RemovePen(TrendPen trendPen)
        {
            var trendPresenter = _trendPresenters[trendPen];
            BindingOperations.ClearBinding(trendPresenter, TrendPresenter.ShowPointsProperty);
            _trendPresenters.Remove(trendPen);
            _gTrendPresenters.Children.Remove(trendPresenter);
            _trendPens.Remove(trendPen);

            #region Slider

            var sliderPresenter = _sliderPresenters[trendPen];
            _sliderPresenters.Remove(trendPen);
            sliderPresenter.SizeChanged -= SliderPresenterSizeChanged;
            sliderPresenter.LayoutUpdated -= SliderPresenterLayoutUpdated;
            _spSlider.Children.Remove(sliderPresenter);
            BindingOperations.ClearBinding(sliderPresenter, VisibilityProperty);
            var sliderLine = _sliderLines[trendPen];
            _sliderLines.Remove(trendPen);
            BindingOperations.ClearBinding(sliderLine, Line.Y1Property);
            BindingOperations.ClearBinding(sliderLine, VisibilityProperty);
            _cSlider.Children.Remove(sliderLine);

            #endregion
        }

        #endregion

        #region Axes

        #region MinimumYAxesStep

        public int MinimumYAxesStep
        {
            get { return (int)GetValue(MinimumYAxesStepProperty); }
            set { SetValue(MinimumYAxesStepProperty, value); }
        }

        public static readonly DependencyProperty MinimumYAxesStepProperty =
            DependencyProperty.Register("MinimumYAxesStep", typeof(int), typeof(TrendViewer), new UIPropertyMetadata(100));
        
        #endregion

        #region MinimumXAxesStep

        public int MinimumXAxesStep
        {
            get { return (int)GetValue(MinimumXAxesStepProperty); }
            set { SetValue(MinimumXAxesStepProperty, value); }
        }

        public static readonly DependencyProperty MinimumXAxesStepProperty =
            DependencyProperty.Register("MinimumXAxesStep", typeof(int), typeof(TrendViewer), new UIPropertyMetadata(150));
        
        #endregion

        private KeyValuePair<double, double>[] GetVerticalAxesMarkers(
            double downValue,
            double upValue,
            double down,
            double up)
        {
            var yKoefficient = (up - down)/(upValue - downValue);

            var deltaY = Math.Abs(up - down);

            var gridLinesNumber = (int) deltaY/MinimumYAxesStep + 1;

            var markers = new List<KeyValuePair<double, double>>();

            if (gridLinesNumber > 1)
            {
                var step = (upValue - downValue)/(gridLinesNumber - 1);
                step = GetNormalValue(step);
                var absStep = Math.Abs(step);
                var currentLevelNumber = Math.Round(downValue/absStep);
                var currentLevel = currentLevelNumber*absStep;

                var sign = Math.Sign(upValue - downValue);

                while (currentLevel*sign <= upValue*sign)
                {
                    if (currentLevel*sign >= downValue*sign)
                    {
                        var yKoordinate = down + (currentLevel - downValue)*yKoefficient;
                        markers.Add(new KeyValuePair<double, double>(yKoordinate, currentLevel));
                    }
                    currentLevelNumber = currentLevelNumber + sign;
                    currentLevel = currentLevelNumber*absStep;
                }
            }
            return markers.ToArray();
        }

        private static double GetNormalValue(double value)
        {
            var sign = Math.Sign(value);
            value = Math.Abs(value);
            var valMin = 1d;
            var val2 = 2d;
            var val5 = 5d;
            var valMax = 10d;
            while (value < valMin)
            {
                valMin /= 10;
                val2 /= 10;
                val5 /= 10;
                valMax /= 10;
            }
            while (value > valMax)
            {
                valMin *= 10;
                val2 *= 10;
                val5 *= 10;
                valMax *= 10;
            }
            if (value >= valMin && value < val2)
            {
                value = valMin;
            }
            else if (value >= val2 && value < val5)
            {
                value = val2;
            }
            else if (value >= val5 && value < valMax)
            {
                value = val5;
            }
            return value*sign;
        }

        private IEnumerable<KeyValuePair<double, DateTime>> GetHorisontalAxesMarkers(
            DateTime leftTime,
            DateTime rightTime,
            double left,
            double right,
            out DateTimeIntervalType intervalType,
            out int interval)
        {
            var deltaX = Math.Abs(right - left);
            var gridLinesNumber = (int) deltaX/MinimumXAxesStep + 1;

            interval = 0;
            intervalType = DateTimeIntervalType.Years;

            var markers = new List<KeyValuePair<double, DateTime>>();
            if (gridLinesNumber > 1)
            {
                var leftTimeTicks = leftTime.Ticks;
                var rightTimeTicks = rightTime.Ticks;
                var xKoefficient = (right - left)/(rightTimeTicks - leftTimeTicks);

                var step = new TimeSpan((rightTimeTicks - leftTimeTicks)/(gridLinesNumber - 1));

                GetDateTimeInterval(step, out intervalType, out interval);
                var currentDateTime = GetStartDateTime(leftTime, intervalType, interval);
                while (currentDateTime <= rightTime)
                {
                    if (currentDateTime >= leftTime)
                    {
                        var xKoordinate = left + (currentDateTime.Ticks - leftTimeTicks)*xKoefficient;
                        markers.Add(new KeyValuePair<double, DateTime>(xKoordinate, currentDateTime));
                    }
                    currentDateTime = AddIntervalToDateTime(currentDateTime, intervalType, interval);
                }
            }
            return markers;
        }

        private static DateTime AddIntervalToDateTime(DateTime dateTime, DateTimeIntervalType intervalType, int interval)
        {
            switch (intervalType)
            {
                case DateTimeIntervalType.Years:
                    return dateTime.AddYears(interval);
                case DateTimeIntervalType.Months:
                    return dateTime.AddMonths(interval);
                case DateTimeIntervalType.Days:
                    return dateTime.AddDays(interval);
                case DateTimeIntervalType.Hours:
                    return dateTime.AddHours(interval);
                case DateTimeIntervalType.Minutes:
                    return dateTime.AddMinutes(interval);
                case DateTimeIntervalType.Seconds:
                    return dateTime.AddSeconds(interval);
                case DateTimeIntervalType.Milliseconds:
                    return dateTime.AddMilliseconds(interval);
                default:
                    return dateTime.AddYears(1);
            }
        }

        private static void GetDateTimeInterval(TimeSpan step, out DateTimeIntervalType intervalType, out int interval)
        {
            const int daysInYear = 365;
            const int daysInMonth = 31;
            intervalType = DateTimeIntervalType.Years;
            interval = 0;
            if (step.TotalDays >= daysInYear)
            {
                intervalType = DateTimeIntervalType.Years;
                interval = (int) GetNormalValue(step.TotalDays/daysInYear);
            }
            else if (step.TotalDays >= daysInMonth)
            {
                intervalType = DateTimeIntervalType.Months;
                interval = (int) GetNormalValue(step.TotalDays/daysInMonth);
            }
            else if ((int) step.TotalDays > 0)
            {
                intervalType = DateTimeIntervalType.Days;
                interval = (int) GetNormalValue(step.TotalDays);
            }
            else if ((int) step.TotalHours > 0)
            {
                intervalType = DateTimeIntervalType.Hours;
                interval = (int) GetNormalValue(step.TotalHours);
            }
            else if ((int) step.TotalMinutes > 0)
            {
                intervalType = DateTimeIntervalType.Minutes;
                interval = (int) GetNormalValue(step.TotalMinutes);
            }
            else if ((int) step.TotalSeconds > 0)
            {
                intervalType = DateTimeIntervalType.Seconds;
                interval = (int) GetNormalValue(step.TotalSeconds);
            }
            else if ((int) step.TotalMilliseconds > 0)
            {
                intervalType = DateTimeIntervalType.Milliseconds;
                interval = (int) GetNormalValue(step.TotalMilliseconds);
            }
            if (interval == 0)
                interval = 1;
        }

        private static DateTime GetStartDateTime(DateTime leftDateTime, DateTimeIntervalType intervalType, int interval)
        {
            switch (intervalType)
            {
                case DateTimeIntervalType.Years:
                    return new DateTime(
                        leftDateTime.Year,
                        1,
                        1);
                case DateTimeIntervalType.Months:
                    return new DateTime(
                        leftDateTime.Year,
                        leftDateTime.Month,
                        1);
                case DateTimeIntervalType.Days:
                    return new DateTime(
                        leftDateTime.Year,
                        leftDateTime.Month,
                        leftDateTime.Day);
                case DateTimeIntervalType.Hours:
                    return new DateTime(
                        leftDateTime.Year,
                        leftDateTime.Month,
                        leftDateTime.Day,
                        leftDateTime.Hour/interval*interval,
                        0,
                        0);
                case DateTimeIntervalType.Minutes:
                    return new DateTime(
                        leftDateTime.Year,
                        leftDateTime.Month,
                        leftDateTime.Day,
                        leftDateTime.Hour,
                        leftDateTime.Minute/interval*interval,
                        0);
                case DateTimeIntervalType.Seconds:
                    return new DateTime(
                        leftDateTime.Year,
                        leftDateTime.Month,
                        leftDateTime.Day,
                        leftDateTime.Hour,
                        leftDateTime.Minute,
                        leftDateTime.Second/interval*interval);
                case DateTimeIntervalType.Milliseconds:
                    return new DateTime(
                        leftDateTime.Year,
                        leftDateTime.Month,
                        leftDateTime.Day,
                        leftDateTime.Hour,
                        leftDateTime.Minute,
                        leftDateTime.Second,
                        leftDateTime.Millisecond/interval*interval);
                default:
                    return leftDateTime;
            }
        }

        private static string GetDateTimeLabelFormat(DateTimeIntervalType intervalType)
        {
            switch (intervalType)
            {
                case DateTimeIntervalType.Years:
                    return "MMM";
                case DateTimeIntervalType.Months:
                    return "MMM";
                case DateTimeIntervalType.Days:
                    return "dd.MM";
                case DateTimeIntervalType.Hours:
                    return "HH:mm";
                case DateTimeIntervalType.Minutes:
                    return "HH:mm";
                case DateTimeIntervalType.Seconds:
                    return "HH:mm:ss";
                case DateTimeIntervalType.Milliseconds:
                    return "ss.fff";
                default:
                    return null;
            }
        }

        private static string GetFirstDateTimeLabelFormat(DateTimeIntervalType intervalType)
        {
            switch (intervalType)
            {
                case DateTimeIntervalType.Years:
                    return "yyyy";
                case DateTimeIntervalType.Months:
                    return "yyyy";
                case DateTimeIntervalType.Days:
                    return "yyyy";
                case DateTimeIntervalType.Hours:
                    return "dd.MM.yyyy";
                case DateTimeIntervalType.Minutes:
                    return "dd.MM.yyyy";
                case DateTimeIntervalType.Seconds:
                    return "dd.MM.yyyy";
                case DateTimeIntervalType.Milliseconds:
                    return "dd.MM.yyyy HH:mm";
                default:
                    return null;
            }
        }

        #endregion

        #region Axes

        #region HorizontalAxes

        private Brush _horizontalGridLinesBrush = Brushes.LightGray;

        private double _horizontalGridLinesThickness = 1;

        public Brush HorizontalGridLinesBrush
        {
            get { return _horizontalGridLinesBrush; }
            set { _horizontalGridLinesBrush = value; }
        }

        public double HorizontalGridLinesThickness
        {
            get { return _horizontalGridLinesThickness; }
            set { _horizontalGridLinesThickness = value; }
        }

        public string HorisontalAxesLabelsFormat { get; set; }

        public string HorisontalAxesFirstLabelsFormat { get; set; }

        private void CHorizontalAxesSizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawHorisontalAxes();
        }

        private void DrawHorisontalAxes()
        {
            DateTimeIntervalType dateTimeIntervalType;
            int interval;
            var leftTime = _startTime;
            var rightTime = EndTime;
            var markers = GetHorisontalAxesMarkers(
                leftTime,
                rightTime,
                0,
                _cHorizontalAxes.ActualWidth,
                out dateTimeIntervalType,
                out interval);
            var pathFigureCollection = new PathFigureCollection();
            var gLabels = new Grid
                              {
                                  Margin = new Thickness(_gVerticalAxes.ActualWidth - FontSize*1.2, FontSize*0.5, 0, 0)
                              };
            var format = HorisontalAxesLabelsFormat ?? GetDateTimeLabelFormat(dateTimeIntervalType);
            var gFirstLabels = new Grid
                                   {
                                       Margin =
                                           new Thickness(_gVerticalAxes.ActualWidth - FontSize*1.2, FontSize*0.35, 0, 0)
                                   };
            var firstLabelFormat = HorisontalAxesFirstLabelsFormat ?? GetFirstDateTimeLabelFormat(dateTimeIntervalType);
            var previousFirstLabel = string.Empty;
            foreach (var marker in markers)
            {
                var pathFigure = new PathFigure
                                     {
                                         StartPoint = new Point(marker.Key, _cHorizontalAxes.ActualHeight)
                                     };
                pathFigure.Segments.Add(new LineSegment(new Point(marker.Key, 0), true));
                pathFigureCollection.Add(pathFigure);

                var tbLabel = new TextBlock
                                  {
                                      Text = marker.Value.ToLocalTime().ToString(format),
                                      Margin = new Thickness(marker.Key, 0, 0, 0)
                                  };
                gLabels.Children.Add(tbLabel);

                var firstLabel = marker.Value.ToLocalTime().ToString(firstLabelFormat);
                if (firstLabel != previousFirstLabel)
                {
                    var tbFirstLabel = new TextBlock
                                           {
                                               Text = firstLabel,
                                               Margin = tbLabel.Margin
                                           };
                    gFirstLabels.Children.Add(tbFirstLabel);
                    previousFirstLabel = firstLabel;
                }
            }
            var pathGeometry = new PathGeometry(pathFigureCollection);
            var path = new Path
                           {
                               Data = pathGeometry,
                               Stroke = HorizontalGridLinesBrush,
                               StrokeThickness = HorizontalGridLinesThickness
                           };
            _cHorizontalAxes.Children.Clear();
            _cHorizontalAxes.Children.Add(path);

            _spHorisontalAxes.Children.Clear();
            _spHorisontalAxes.Children.Add(gLabels);
            _spHorisontalAxes.Children.Add(gFirstLabels);
        }

        #endregion

        #region VerticalAxes

        private Brush _verticalGridLinesBrush = Brushes.LightGray;

        private double _verticalGridLinesThickness = 1;

        public Brush VerticalGridLinesBrush
        {
            get { return _verticalGridLinesBrush; }
            set { _verticalGridLinesBrush = value; }
        }

        public double VerticalGridLinesThickness
        {
            get { return _verticalGridLinesThickness; }
            set { _verticalGridLinesThickness = value; }
        }

        public string VerticalAxesLabelsFormat { get; set; }

        private void CVerticalAxesSizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawVerticalAxes();
        }

        private void DrawVerticalAxes()
        {
            if (CurrentTrendPen == null)
            {
                _gVerticalAxes.Children.Clear();
                _cVerticalAxes.Children.Clear();
                return;
            }
            DrawVerticalAxes(ScaleMaximum, ScaleMinimum);
        }

        private void DrawVerticalAxes(double max, double min)
        {
            if (_cVerticalAxes == null)
                return;
            var markers = GetVerticalAxesMarkers(
                min,
                max,
                _cVerticalAxes.ActualHeight,
                0);
            DrawVerticalAxes(markers);
        }

        private void DrawVerticalAxes(KeyValuePair<double, double>[] markers)
        {
            var offset = -FontSize*0.7;
            _gVerticalAxes.Children.Clear();
            _cVerticalAxes.Children.Clear();
            foreach (var marker in markers)
            {
                var textBlock = new TextBlock
                                    {
                                        Margin = new Thickness(2, marker.Key + offset, 2, 0),
                                        Text = marker.Value.ToString(VerticalAxesLabelsFormat)
                                    };
                _gVerticalAxes.Children.Add(textBlock);
            }

            var pathFigureCollection = new PathFigureCollection();
            foreach (var marker in markers)
            {
                var pathFigure = new PathFigure
                                     {
                                         StartPoint = new Point(0, marker.Key)
                                     };
                pathFigure.Segments.Add(new LineSegment(new Point(_cVerticalAxes.ActualWidth, marker.Key), true));
                pathFigureCollection.Add(pathFigure);
            }
            var pathGeometry = new PathGeometry(pathFigureCollection);
            var gridLinesPath = new Path
                                    {
                                        Data = pathGeometry,
                                        Stroke = VerticalGridLinesBrush,
                                        StrokeThickness = VerticalGridLinesThickness
                                    };

            _cVerticalAxes.Children.Add(gridLinesPath);
        }

        #endregion

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _dgTrendPens = GetTemplateChild("dgTrendPens") as DataGrid;
            if (_dgTrendPens != null)
            {
                UpdateDgPensColumnsVisibility();
            }

            _gTrendPresenters = (Grid) GetTemplateChild("gTrendPresenters");
            if (_gTrendPresenters != null)
            {
                _gTrendPresenters.MouseLeftButtonDown += GTrendPresentersMouseLeftButtonDown;
                _gTrendPresenters.MouseLeftButtonUp += GTrendPresentersMouseLeftButtonUp;
                _gTrendPresenters.MouseMove += GTrendPresentersMouseMove;
            }

            _gVerticalAxes = (Grid)GetTemplateChild("gVerticalAxes");
            _cVerticalAxes = (Canvas)GetTemplateChild("cVerticalAxes");
            if (_cVerticalAxes != null)
            {
                _cVerticalAxes.SizeChanged += CVerticalAxesSizeChanged;
            }
            _cHorizontalAxes = (Canvas)GetTemplateChild("cHorizontalAxes");
            if (_cHorizontalAxes != null)
            {
                _cHorizontalAxes.SizeChanged += CHorizontalAxesSizeChanged;
            }
            _spHorisontalAxes = (StackPanel)GetTemplateChild("spHorisontalAxes");

            #region Slider

            _slSlider = GetTemplateChild("slSlider") as Slider;
            if (_slSlider != null)
            {
                var sliderBinding = new Binding("CursorPosition") { Source = this, Mode = BindingMode.TwoWay };
                _slSlider.SetBinding(RangeBase.ValueProperty, sliderBinding);
            }

            _cdSliderOffset = (ColumnDefinition)GetTemplateChild("cdSliderOffset");
            _gSlider = (Grid)GetTemplateChild("gSlider");
            if (_gSlider != null)
                _gSlider.SizeChanged += GSliderSizeChanged;
            _cSlider = (Canvas)GetTemplateChild("cSlider");
            if (_cSlider != null)
            {
                var presentersVisibilityBinding = new Binding("ShowCursorPresenters") { Source = this, Converter = new BooleanToVisibilityConverter() };
                _cSlider.SetBinding(VisibilityProperty, presentersVisibilityBinding);
                _cSlider.SizeChanged += CSliderSizeChanged;
            }
            _spSlider = (Panel)GetTemplateChild("spSlider");
            UpdateSliderVisibility();

            #endregion

            #region SetPeriod

            _popupPeriod = GetTemplateChild("popupPeriod") as Popup;
            _tspSetPeriod = GetTemplateChild("tspSetPeriod") as TimeSpanPicker;
            if (_popupPeriod != null && _tspSetPeriod != null)
            {
                _popupPeriod.Opened += PopupPeriodOpened;
                _tspSetPeriod.KeyUp += TspSetPeriodKeyUp;
                var bSetPeriod = GetTemplateChild("bSetPeriod") as Button;
                if (bSetPeriod != null)
                    bSetPeriod.Click += BSetPeriodClick;
            }

            #endregion

            #region SetTime

            _popupTime = GetTemplateChild("popupTime") as Popup;
            _dtpSetTime = GetTemplateChild("dtpSetTime") as DateTimePicker;
            if (_popupTime != null && _dtpSetTime != null)
            {
                _popupTime.Opened += PopupTimeOpened;
                _dtpSetTime.KeyUp += DtpSetTimeKeyUp;
                var bSetTime = GetTemplateChild("bSetTime") as Button;
                if (bSetTime != null)
                {
                    bSetTime.Click += BSetTimeClick;
                }
            }

            #endregion
        }

        #region private methods
        
        private void UpdateDgPensColumnsVisibility()
        {
            if (_dgTrendPens == null)
                return;
            var columns = _dgTrendPens.Columns;
            var visibility = ShowCursor ? Visibility.Visible : Visibility.Collapsed;
            if (columns.Count >= 6)
            {
                columns[5].Visibility = visibility;
                columns[6].Visibility = visibility;
            }
        }

        private TimeSpan GetPeriod()
        {
            if (!Period.HasValue || Period.Value.Ticks == 0)
                Period = new TimeSpan(0, 0, 1, 0);
            TimeSpan? minimalPeriod = null;
            foreach (var trendPen in _trendPens)
            {
                if (minimalPeriod == null || minimalPeriod > trendPen.TrendTag.Category.Period)
                {
                    minimalPeriod = trendPen.TrendTag.Category.Period;
                }
            }
            if (minimalPeriod.HasValue && Period.Value.Ticks > minimalPeriod.Value.Ticks*MaxPeriodGain)
                Period = new TimeSpan(minimalPeriod.Value.Ticks*MaxPeriodGain);
            if (Period.Value.TotalDays > 3650)
                Period = new TimeSpan(3650, 0, 0, 0);
            return Period.Value;
        }

        private void SetTime(long ticks)
        {
            if (ticks > DateTime.MinValue.Ticks && ticks < DateTime.MaxValue.Ticks)
                EndTime = new DateTime(ticks);
        }

        #endregion

        #region event handlsers

        private void TrendViewerLoaded(object sender, RoutedEventArgs e)
        {
            if (!_loaded)
            {
                _pHmi = (PHmiAbstract) DataContext;
                _pHmi.AfterUpdate += PHmiAfterUpdate;
                LoadPens();
                if (CurrentTrendPen == null)
                {
                    CurrentTrendPen = _trendPens.FirstOrDefault();
                }
                UpdateTrend();
            }
            _loaded = true;
        }
        
        private void TrendViewerUnloaded(object sender, RoutedEventArgs e)
        {
            if (_loaded)
            {
                UnloadPens();
                _pHmi.AfterUpdate -= PHmiAfterUpdate;
                _pHmi = null;
            }
            _loaded = false;
        }

        private void PHmiAfterUpdate(object sender, EventArgs e)
        {
            if (!_timer.Enabled)
            {
                _timer.Start();
            }
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();
            Dispatcher.Invoke(new Action(UpdateTrend));
        }

        private void UpdateTrend()
        {
            var pHmi = _pHmi;
            if (pHmi == null)
                return;
            if (Play)
            {
                EndTime = pHmi.Time;
                DrawTrends();
            }
            else if (EndTime > pHmi.Time)
            {
                DrawTrends();
            }
        }

        #endregion

        public void DrawTrends()
        {
            DrawHorisontalAxes();
            foreach (var trendPresenter in _trendPresenters.Values)
            {
                trendPresenter.ShowTrend(_startTime.Ticks, EndTime.Ticks);
            }
        }

        #region Dependency Properties

        #region Play

        public static readonly DependencyProperty PlayProperty =
            DependencyProperty.Register("Play", typeof (bool), typeof (TrendViewer), new PropertyMetadata(true, OnPlayChanged));

        private static void OnPlayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var trendViewer = (TrendViewer)d;
            trendViewer.UpdateTrend();
        }

        public bool Play
        {
            get { return (bool) GetValue(PlayProperty); }
            set { SetValue(PlayProperty, value); }
        }

        #endregion

        #region ShowCursor

        public static readonly DependencyProperty ShowCursorProperty =
            DependencyProperty.Register("ShowCursor", typeof (bool), typeof (TrendViewer),
                                        new PropertyMetadata(OnShowCursorChanged));

        public bool ShowCursor
        {
            get { return (bool) GetValue(ShowCursorProperty); }
            set { SetValue(ShowCursorProperty, value); }
        }

        private static void OnShowCursorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var trendViewer = (TrendViewer) d;
            trendViewer.UpdateSliderVisibility();
            trendViewer.UpdateDgPensColumnsVisibility();
        }

        #endregion

        #region CursorPosition

        public static readonly DependencyProperty CursorPositionProperty =
            DependencyProperty.Register("CursorPosition", typeof (double), typeof (TrendViewer),
                                        new PropertyMetadata(OnCursorPositionChanged));

        public double CursorPosition
        {
            get { return (double) GetValue(CursorPositionProperty); }
            set { SetValue(CursorPositionProperty, value); }
        }

        private static void OnCursorPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var trendViewer = (TrendViewer) d;
            trendViewer.UpdateSlider();
        }

        #endregion

        #region ShowCursorPresenters

        public static readonly DependencyProperty ShowCursorPresentersProperty =
            DependencyProperty.Register("ShowCursorPresenters", typeof (bool), typeof (TrendViewer),
                                        new PropertyMetadata(true));

        public bool ShowCursorPresenters
        {
            get { return (bool) GetValue(ShowCursorPresentersProperty); }
            set { SetValue(ShowCursorPresentersProperty, value); }
        }

        #endregion

        #region ScaleMinimum

        public static readonly DependencyProperty ScaleMinimumProperty =
            DependencyProperty.Register("ScaleMinimum", typeof (double), typeof (TrendViewer),
                                        new UIPropertyMetadata(OnScaleChanged));

        public double ScaleMinimum
        {
            get { return (double) GetValue(ScaleMinimumProperty); }
            set { SetValue(ScaleMinimumProperty, value); }
        }

        #endregion

        #region ScaleMaximum

        public static readonly DependencyProperty ScaleMaximumProperty =
            DependencyProperty.Register("ScaleMaximum", typeof (double), typeof (TrendViewer),
                                        new UIPropertyMetadata(OnScaleChanged));

        public double ScaleMaximum
        {
            get { return (double) GetValue(ScaleMaximumProperty); }
            set { SetValue(ScaleMaximumProperty, value); }
        }

        #endregion

        private static void OnScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var trendViewer = (TrendViewer) d;
            trendViewer.DrawVerticalAxes();
        }

        #region CurrentTrendPen

        public static readonly DependencyProperty CurrentTrendPenProperty =
            DependencyProperty.Register("CurrentTrendPen", typeof (TrendPen), typeof (TrendViewer),
                                        new PropertyMetadata(OnScaleChanged));

        public TrendPen CurrentTrendPen
        {
            get { return (TrendPen) GetValue(CurrentTrendPenProperty); }
            set { SetValue(CurrentTrendPenProperty, value); }
        }

        #endregion

        #region Period

        public static readonly DependencyProperty PeriodProperty =
            DependencyProperty.Register("Period", typeof (TimeSpan?), typeof (TrendViewer),
                                        new PropertyMetadata(null, OnPeriodChanged));

        public TimeSpan? Period
        {
            get { return (TimeSpan?) GetValue(PeriodProperty); }
            set { SetValue(PeriodProperty, value); }
        }

        private static void OnPeriodChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var trendViewer = (TrendViewer) d;
            trendViewer.UpdateNewEndTime(trendViewer.EndTime);
        }

        #endregion

        #region EndTime

        public static readonly DependencyProperty EndTimeProperty =
            DependencyProperty.Register("EndTime", typeof (DateTime), typeof (TrendViewer),
                                        new PropertyMetadata(DateTime.Now, OnEndTimeChanged));

        public DateTime EndTime
        {
            get { return (DateTime) GetValue(EndTimeProperty); }
            set { SetValue(EndTimeProperty, value); }
        }

        private static void OnEndTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var trendViewer = (TrendViewer) d;
            trendViewer.UpdateNewEndTime((DateTime) e.NewValue);
        }

        public void UpdateNewEndTime(DateTime time)
        {
            var newEndTime = time;
            var minimumDate = new DateTime(1754, 1, 1) + GetPeriod();
            if (newEndTime < minimumDate)
                newEndTime = minimumDate;
            EndTime = newEndTime;
            _startTime = EndTime - GetPeriod();
            if (!Play)
            {
                DrawTrends();
            }
        }

        #endregion

        #endregion

        #region Slider

        private const double SliderCapWidth = 10;
        private Brush _sliderLinesBrush = Brushes.Gray;
        private double _sliderLinesThickness = 1;

        public Brush SliderLinesBrush
        {
            get { return _sliderLinesBrush; }
            set
            {
                _sliderLinesBrush = value;
                foreach (var sliderLine in _sliderLines)
                {
                    sliderLine.Value.Stroke = _sliderLinesBrush;
                }
            }
        }

        public double SliderLinesThickness
        {
            get { return _sliderLinesThickness; }
            set
            {
                _sliderLinesThickness = value;
                foreach (var sliderLine in _sliderLines)
                {
                    sliderLine.Value.StrokeThickness = _sliderLinesThickness;
                }
            }
        }

        private void SliderPresenterLayoutUpdated(object sender, EventArgs e)
        {
            UpdateAllSliderLines();
        }

        private void UpdateSliderVisibility()
        {
            var visibility = ShowCursor ? Visibility.Visible : Visibility.Collapsed;
            if (_slSlider != null)
            {
                _slSlider.Visibility = visibility;
            }
            if (_gSlider != null)
            {
                _gSlider.Visibility = visibility;
            }
        }

        private void UpdateSliderLine(Line line, TrendPen trendPen)
        {
            if (!_sliderPresenters.ContainsKey(trendPen))
                return;
            var sliderPresenter = _sliderPresenters[trendPen];
            var transform = sliderPresenter.TransformToVisual(_cSlider);
            var position = transform.Transform(new Point(0, 0));
            line.Y2 = position.Y + sliderPresenter.ActualHeight/2;
            if (position.X < 0)
            {
                line.X2 = position.X + sliderPresenter.ActualWidth;
            }
            else
            {
                line.X2 = position.X;
            }
        }

        private void UpdateAllSliderLines()
        {
            foreach (var sliderLine in _sliderLines)
            {
                UpdateSliderLine(sliderLine.Value, sliderLine.Key);
            }
        }

        private void SliderPresenterSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateAllSliderLines();
        }

        private void CSliderSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateAllSliderLines();
        }

        private void UpdateSlider()
        {
            if (!_loaded)
                return;
            if (!ShowCursor)
                return;
            var sliderOffset = CursorPosition + (SliderCapWidth/2);
            _cdSliderOffset.Width = new GridLength(sliderOffset);
            foreach (var trendPresenter in _trendPresenters.Values)
            {
                trendPresenter.SliderOffset = sliderOffset;
            }
        }

        private void GSliderSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var maximum = _gSlider.ActualWidth - SliderCapWidth;
            if (!double.IsNaN(maximum) && _slSlider != null)
            {
                _slSlider.Minimum = 0;
                _slSlider.Maximum = maximum;
            }
            UpdateSlider();
        }

        #endregion

        #region Commands

        private ICommand _forwardForwardCommand;

        public ICommand ForwardForwardCommand
        {
            get { return _forwardForwardCommand ?? (_forwardForwardCommand = new DelegateCommand(ForwardForwardExecuted)); }
        }

        private void ForwardForwardExecuted(object obj)
        {
            Play = false;
            EndTime = EndTime + GetPeriod();
        }

        private ICommand _forwardCommand;

        public ICommand ForwardCommand
        {
            get { return _forwardCommand ?? (_forwardCommand = new DelegateCommand(ForwardExecuted)); }
        }

        private void ForwardExecuted(object obj)
        {
            Play = false;
            SetTime(EndTime.Ticks + GetPeriod().Ticks/4);
        }

        private ICommand _backBackCommand;

        public ICommand BackBackCommand
        {
            get { return _backBackCommand ?? (_backBackCommand = new DelegateCommand(BackBackExecuted)); }
        }

        private void BackBackExecuted(object obj)
        {
            Play = false;
            EndTime = EndTime - GetPeriod();
        }

        private ICommand _backCommand;

        public ICommand BackCommand
        {
            get { return _backCommand ?? (_backCommand = new DelegateCommand(BackExecuted)); }
        }

        private void BackExecuted(object obj)
        {
            Play = false;
            SetTime(EndTime.Ticks - GetPeriod().Ticks/4);
        }

        #endregion

        #region Hand movement

        private bool _draggingEnabled;
        private double? _draggingLastEndTimeTicks;
        private double? _draggingLastPosition;

        private void GTrendPresentersMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                _draggingEnabled = false;
                Mouse.Capture(null);
                _draggingLastEndTimeTicks = null;
                _draggingLastPosition = null;
            }
            if (!_draggingEnabled)
                return;

            var currentPosition = e.GetPosition(_gTrendPresenters).X;
            var deltaX = _draggingLastPosition - currentPosition;
            _draggingLastPosition = currentPosition;

            var deltaTicks = GetPeriod().Ticks/_gTrendPresenters.ActualWidth*deltaX;
            var endTimeTicks = _draggingLastEndTimeTicks + deltaTicks;
            _draggingLastEndTimeTicks = endTimeTicks;

            Play = false;
            if (endTimeTicks.HasValue)
                SetTime((long) endTimeTicks.Value);
        }

        private void GTrendPresentersMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _draggingEnabled = false;
            Mouse.Capture(null);
            _draggingLastEndTimeTicks = null;
            _draggingLastPosition = null;
        }

        private void GTrendPresentersMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _draggingEnabled = true;
            Mouse.Capture(_gTrendPresenters);
            _draggingLastPosition = e.GetPosition(_gTrendPresenters).X;
            _draggingLastEndTimeTicks = EndTime.Ticks;
        }

        #endregion

        #region SetTime
        
        public DateTime TimeToSet
        {
            get { return (DateTime)GetValue(TimeToSetProperty); }
            set { SetValue(TimeToSetProperty, value); }
        }

        public static readonly DependencyProperty TimeToSetProperty =
            DependencyProperty.Register("TimeToSet", typeof (DateTime), typeof (TrendViewer));

        private void BSetTimeClick(object sender, RoutedEventArgs e)
        {
            Play = false;
            EndTime = TimeToSet;
            _popupTime.IsOpen = false;
        }

        private void DtpSetTimeKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    BSetTimeClick(null, null);
                    break;
                case Key.Escape:
                    _popupTime.IsOpen = false;
                    break;
            }
        }

        private void PopupTimeOpened(object sender, EventArgs e)
        {
            TimeToSet = EndTime;
            _dtpSetTime.Focus();
        }

        #endregion

        #region SetPeriod

        private void BSetPeriodClick(object sender, RoutedEventArgs e)
        {
            if (_tspSetPeriod.TimeSpan.HasValue)
            {
                Period = _tspSetPeriod.TimeSpan;
            }
            _popupPeriod.IsOpen = false;
        }

        private void TspSetPeriodKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    BSetPeriodClick(null, null);
                    break;
                case Key.Escape:
                    _popupPeriod.IsOpen = false;
                    break;
            }
        }

        private void PopupPeriodOpened(object sender, EventArgs e)
        {
            _tspSetPeriod.TimeSpan = GetPeriod();
            _tspSetPeriod.Focus();
        }

        #endregion

        #region ShowPoints

        public bool ShowPoints
        {
            get { return (bool)GetValue(ShowPointsProperty); }
            set { SetValue(ShowPointsProperty, value); }
        }

        public static readonly DependencyProperty ShowPointsProperty =
            DependencyProperty.Register("ShowPoints", typeof(bool), typeof(TrendViewer), null);
        
        #endregion
    }
}