using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using PHmiClient.Trends;

namespace PHmiClient.Controls.Trends
{
    public class TrendPresenter : FrameworkElement
    {
        private readonly Pen _pen = new Pen(Brushes.Black, 1)
            {
                LineJoin = PenLineJoin.Round,
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round
            };
        private readonly object _lockObject = new object();
        private readonly TrendPen _trendPen;
        private long _endTime = DateTime.Now.Ticks;
        private int _maxPoints = 170;
        private int _rarerer;
        private double _sliderOffset;
        private long _startTime = DateTime.Now.Ticks - TimeSpan.TicksPerMinute;
        private KeyValuePair<long, double>[] _values;

        #region Query

        private readonly Timer _queryTimer = new Timer(200);

        private Action<Tuple<DateTime, double>[]> _queryCallback;

        private Tuple<DateTime, DateTime?, int> _queryParameters;

        private void QueryTimerElapsed(object sender, ElapsedEventArgs e)
        {
            lock (_lockObject)
            {
                if (!_queryTimer.Enabled)
                    return;
                _queryTimer.Stop();
                if (_queryParameters == null || _queryCallback == null)
                    return;
                ShowProgressBar();
                TrendTag.GetSamples(_queryParameters.Item1, _queryParameters.Item2, _queryParameters.Item3, _queryCallback);
            }
        }

        private void ShowProgressBar()
        {
            Dispatcher.Invoke(new Action(() => { _trendPen.WaitingSamples = true; }));
        }

        private void HideProgressBar()
        {
            Dispatcher.Invoke(new Action(() => { _trendPen.WaitingSamples = false; }));
        }

        #endregion
        
        public Brush LineBrush
        {
            get { return _pen.Brush; }
            set
            {
                _pen.Brush = value;
                InvalidateVisual();
            }
        }

        public double LineThickness
        {
            get { return _pen.Thickness; }
            set
            {
                _pen.Thickness = value;
                InvalidateVisual();
            }
        }
        
        #region CursorCoordinate

        public static readonly DependencyProperty CursorCoordinateProperty =
            DependencyProperty.Register("CursorCoordinate", typeof (double?), typeof (TrendPresenter));

        public double? CursorCoordinate
        {
            get { return (double?) GetValue(CursorCoordinateProperty); }
            set { SetValue(CursorCoordinateProperty, value); }
        }

        #endregion

        #region Scale

        #region MinValue
        
        public double? MinValue
        {
            get { return (double?)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof(double?), typeof(TrendPresenter), new PropertyMetadata(OnScaleValueChanged));
        
        #endregion

        #region MaxValue
        
        public double? MaxValue
        {
            get { return (double?)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(double?), typeof(TrendPresenter), new PropertyMetadata(OnScaleValueChanged));
        
        #endregion

        private static void OnScaleValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var trendPresenter = (TrendPresenter) d;
            trendPresenter.InvalidateVisual();
        }

        #endregion

        #region ShowPoints

        public bool ShowPoints
        {
            get { return (bool)GetValue(ShowPointsProperty); }
            set { SetValue(ShowPointsProperty, value); }
        }

        public static readonly DependencyProperty ShowPointsProperty =
            DependencyProperty.Register(
            "ShowPoints", typeof(bool), typeof(TrendPresenter), new PropertyMetadata(OnShowPointsChanged));

        private static void OnShowPointsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TrendPresenter)d).InvalidateVisual();
        }

        #endregion

        public TrendPresenter(TrendPen trendPen)
        {
            _trendPen = trendPen;
            TrendTag = trendPen.TrendTag;
            LineThickness = trendPen.Thickness;
            LineBrush = trendPen.Brush;
            _queryTimer.Elapsed += QueryTimerElapsed;
            Loaded += TrendPresenterLoaded;
            Unloaded += TrendPresenterUnloaded;
        }

        public ITrendTag TrendTag { get; private set; }

        public double SliderOffset
        {
            get { return _sliderOffset; }
            set
            {
                _sliderOffset = value;
                UpdateCursor();
            }
        }

        public int MaxPoints
        {
            get { return _maxPoints; }
            set
            {
                if (_maxPoints == value)
                    return;
                _maxPoints = value;
                RedrawTrend();
            }
        }

        private void TrendPresenterLoaded(object sender, RoutedEventArgs e)
        {
            var minValueBinding = new Binding("MinValue") {Source = _trendPen};
            SetBinding(MinValueProperty, minValueBinding);
            var maxValueBinding = new Binding("MaxValue") {Source = _trendPen};
            SetBinding(MaxValueProperty, maxValueBinding);

            var visibilityBinding = new Binding("Visible")
            {
                Source = _trendPen,
                Mode = BindingMode.OneWay,
                Converter = new BooleanToVisibilityConverter()
            };
            SetBinding(VisibilityProperty, visibilityBinding);
            _trendPen.IsVisibleChanged += TrendPenIsVisibleChanged;
        }

        private void TrendPenIsVisibleChanged(object sender, EventArgs e)
        {
            ShowTrend(_startTime, _endTime);
        }

        private void TrendPresenterUnloaded(object sender, RoutedEventArgs e)
        {
            BindingOperations.ClearBinding(this, MinValueProperty);
            BindingOperations.ClearBinding(this, MaxValueProperty);
            BindingOperations.ClearBinding(this, VisibilityProperty);
            _trendPen.IsVisibleChanged -= TrendPenIsVisibleChanged;
        }

        private void RedrawTrend()
        {
            if (_values != null)
            {
                ShowTrend(_startTime, _endTime);
            }
        }

        private void UpdateCursor()
        {
            var ticks = _startTime + (long) ((_endTime - _startTime)*_sliderOffset/ActualWidth);
            if (ticks < 0)
                return;
            var nearest = FindNearestValue(ticks, _values);
            var time = ticks;
            double? value = null;
            if (nearest.HasValue)
            {
                if (Math.Abs(nearest.Value.Key - ticks) <= TrendTag.Category.Period.Ticks * (long)Math.Pow(2, _rarerer))
                {
                    time = nearest.Value.Key;
                    value = nearest.Value.Value;
                }
            }
            
            Dispatcher.BeginInvoke(new Action(() =>
                {
                    _trendPen.CursorTime = new DateTime(time);
                    _trendPen.CursorValue = value;
                    if (value == null)
                    {
                        CursorCoordinate = null;
                    }
                    else
                    {
                        CursorCoordinate = Math.Max(0, Math.Min(
                            ActualHeight,
                            ActualHeight - (value.Value - _trendPen.MinScale)*ActualHeight/(_trendPen.MaxScale - _trendPen.MinScale)));
                    }
                }));
        }

        private static KeyValuePair<long, double>? FindNearestValue(long time, KeyValuePair<long, double>[] values)
        {
            if (values == null || values.Length == 0)
                return null;
            var from = 0;
            var to = values.Length - 1;
            if (to < from)
                return null;

            if (time > values[to].Key)
                return values[to];
            if (time < values[from].Key)
                return values[from];

            while (true)
            {
                if (to - from <= 1)
                {
                    var vFrom = values[from];
                    var vTo = values[to];
                    return time - vFrom.Key > vTo.Key - time ? vTo : vFrom;
                }
                var middle = from + (to - from) / 2;
                if (time > values[middle].Key)
                {
                    from = middle;
                }
                else if (time < values[middle].Key)
                {
                    to = middle;
                }
                else
                {
                    return values[middle];
                }
            }
        }
        
        public void ShowTrend(long startTime, long endTime)
        {
            lock (_lockObject)
            {
                _startTime = startTime;
                _endTime = endTime;
                if (!_trendPen.IsVisible)
                    return;

                var rarererNotLog = (int) ((endTime - startTime)/TrendTag.Category.Period.Ticks/_maxPoints) + 1;
                if (rarererNotLog < 1)
                    return;
                var popravka = TrendTag.Category.Period.Ticks * rarererNotLog;

                var rarerer = (int) Math.Max(Math.Min(Math.Log(rarererNotLog, 2), TrendsService.MaxRarerer), 0);

                if (_values != null)
                {
                    var values = (from v in _values
                                  where v.Key >= startTime && v.Key <= endTime
                                  select v).ToList();
                    if (values.Any())
                    {
                        if (values.First().Key > startTime)
                        {
                            var firstValue = _values.Where(v => v.Key < startTime).LastOrDefault();
                            if (_values.Contains(firstValue))
                            {
                                values.Insert(0, firstValue);
                            }
                        }
                        if (values.Last().Key < endTime)
                        {
                            var lastValue = _values.Where(v => v.Key > endTime).FirstOrDefault();
                            if (_values.Contains(lastValue))
                            {
                                values.Add(lastValue);
                            }
                        }
                    }
                    _values = values.ToArray();
                }
                InvalidateVisual();

                _queryTimer.Stop();
                _queryParameters = new Tuple<DateTime, DateTime?, int>(
                    new DateTime(startTime - popravka), NullableLongToDateTime(endTime + popravka), rarerer);
                _queryCallback = results => QueryCallback(startTime, endTime, rarerer, results);
                _queryTimer.Start();
            }
        }

        private void QueryCallback(long startTime, long endTime, int rarerer, IEnumerable<Tuple<DateTime, double>> results)
        {
            lock (_lockObject)
            {
                if (startTime != _startTime || endTime != _endTime)
                {
                    return;
                }
                HideProgressBar();
                _rarerer = rarerer;
                _values = results.Select(r => new KeyValuePair<long, double>(r.Item1.Ticks, r.Item2)).ToArray();
                Dispatcher.Invoke(new Action(InvalidateVisual));
            }
        }

        private static DateTime? NullableLongToDateTime(long? value)
        {
            if (value.HasValue)
                return new DateTime(value.Value);
            return null;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var values = _values;
            var start = _startTime;
            var end = _endTime;

            if (MinValue.HasValue)
            {
                _trendPen.MinScale = MinValue.Value;
            }
            else
            {
                var min = 0.0;
                if (values != null)
                {
                    min = values.OrderBy(k => k.Value).Select(k => k.Value).FirstOrDefault();
                }
                _trendPen.MinScale = min;
            }
            var minValue = _trendPen.MinScale;

            if (MaxValue.HasValue)
            {
                _trendPen.MaxScale = MaxValue.Value;
            }
            else
            {
                var max = 1.0;
                if (values != null)
                {
                    max = values.OrderByDescending(k => k.Value).Select(k => k.Value).FirstOrDefault();
                }
                if (max.Equals(minValue))
                {
                    max = minValue + 1;
                }
                _trendPen.MaxScale = max;
            }
            var maxValue = _trendPen.MaxScale;

            var delta = end - start;
            var valueDelta = maxValue - minValue;
            var pointRadius = _pen.Thickness * 1.5;
            var maxPeriod = TrendTag.Category.Period.Ticks * (long)Math.Pow(2, _rarerer) * 3;
            if (!valueDelta.Equals(0.0) && values != null)
            {
                var lastIndex = values.Length - 2;
                for (var i = 0; i <= lastIndex; i++)
                {
                    var first = values[i];
                    var firstStart = first.Key;
                    var firstValue = first.Value;

                    var showLine = true;
                    if (values[i + 1].Key - firstStart >= maxPeriod)
                    {
                        if (!ShowPoints)
                            continue;
                        showLine = false;
                    }

                    if (!ShowPoints && first.Value.Equals(values[i + 1].Value))
                        while (i + 2 < values.Length
                            && first.Value.Equals(values[i + 2].Value)
                            && values[i + 2].Key - values[i + 1].Key < maxPeriod)
                        {
                            i++;
                        }
                    var second = values[i + 1];
                    var secondStart = second.Key;
                    var secondValue = second.Value;

                    if (firstStart <= start && secondStart <= start || firstStart >= end && secondStart >= end)
                        continue;
                    var showPoint0 = true;
                    if (firstStart < start)
                    {
                        firstValue = (start - firstStart) * (secondValue - firstValue) / (secondStart - firstStart) + firstValue;
                        firstStart = start;
                        showPoint0 = false;
                    }
                    var showPoint1 = true;
                    if (secondStart > end)
                    {
                        secondValue = (end - firstStart) * (secondValue - firstValue) / (secondStart - firstStart) + firstValue;
                        secondStart = end;
                        showPoint1 = false;
                    }
                    if (!double.IsNaN(firstValue) && !double.IsNaN(secondValue))
                    {
                        var point0 = new Point(
                        (firstStart - start) * ActualWidth / delta,
                        Math.Max(0, Math.Min(
                            ActualHeight,
                            ActualHeight - (firstValue - minValue) * ActualHeight / valueDelta)));
                        var point1 = new Point(
                            (secondStart - start) * ActualWidth / delta,
                            Math.Max(0, Math.Min(
                                ActualHeight,
                                ActualHeight -
                                (secondValue - minValue) * ActualHeight / valueDelta)));
                        if (showLine)
                            drawingContext.DrawLine(_pen, point0, point1);
                        if (ShowPoints)
                        {
                            if (showPoint0)
                            {
                                drawingContext.DrawEllipse(_pen.Brush, _pen, point0, pointRadius, pointRadius);
                            }
                            if (i == lastIndex && showPoint1)
                            {
                                drawingContext.DrawEllipse(_pen.Brush, _pen, point1, pointRadius, pointRadius);
                            }
                        }
                    }
                }
                
            }
            UpdateCursor();
            base.OnRender(drawingContext);
        }

        public KeyValuePair<long, double>[] GetValues()
        {
            return _values;
        }
    }
}
