using System;
using System.Windows;
using System.Windows.Media;
using PHmiClient.Trends;
using PHmiClient.Utils;

namespace PHmiClient.Controls.Trends
{
    public class TrendPen : DependencyObject
    {
        #region WaitingSamples

        public static readonly DependencyProperty WaitingSamplesProperty =
            DependencyProperty.Register("WaitingSamples", typeof (bool), typeof (TrendPen));

        public bool WaitingSamples
        {
            get { return (bool)GetValue(WaitingSamplesProperty); }
            set { SetValue(WaitingSamplesProperty, value); }
        }

        #endregion

        #region TrendTag
        
        public static readonly DependencyProperty TrendTagProperty = DependencyProperty.Register(
            "TrendTag", typeof (ITrendTag), typeof (TrendPen), new PropertyMetadata(OnTrendTagChanged));

        public ITrendTag TrendTag
        {
            get { return (ITrendTag)GetValue(TrendTagProperty); }
            set { SetValue(TrendTagProperty, value); }
        }

        private static void OnTrendTagChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newTag = e.NewValue as ITrendTag;
            if (newTag == null)
                return;
            var trendPen = (TrendPen)d;
            if (!trendPen.MaxValue.HasValue)
            {
                trendPen.MaxValue = newTag.MaxValue;
            }
            if (!trendPen.MinValue.HasValue)
            {
                trendPen.MinValue = newTag.MinValue;
            }
        }

        #endregion

        #region Brush

        public static readonly DependencyProperty BrushProperty =
            DependencyProperty.Register("Brush", typeof (Brush), typeof (TrendPen), new PropertyMetadata(Brushes.DarkBlue));

        public Brush Brush
        {
            get { return (Brush)GetValue(BrushProperty); }
            set { SetValue(BrushProperty, value); }
        }

        #endregion

        #region Thickness

        public static readonly DependencyProperty ThicknessProperty =
            DependencyProperty.Register("Thickness", typeof (double), typeof (TrendPen), new PropertyMetadata(2.0));

        public double Thickness
        {
            get { return (double)GetValue(ThicknessProperty); }
            set { SetValue(ThicknessProperty, value); }
        }

        #endregion

        #region Visible

        public static readonly DependencyProperty VisibleProperty = DependencyProperty.Register(
            "Visible", typeof (bool), typeof (TrendPen), new PropertyMetadata(true, OnVisibleChanged));

        public bool Visible
        {
            get { return (bool)GetValue(VisibleProperty); }
            set { SetValue(VisibleProperty, value); }
        }

        private static void OnVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var trendPen = (TrendPen)d;
            trendPen.IsVisible = (bool)e.NewValue;
        }

        public event EventHandler IsVisibleChanged;

        private bool _isVisible = true;

        public bool IsVisible
        {
            get { return _isVisible; }
            private set
            {
                _isVisible = value;
                EventHelper.Raise(ref IsVisibleChanged, this, EventArgs.Empty);
            }
        }

        #endregion

        #region CursorTime

        public static readonly DependencyProperty CursorTimeProperty =
            DependencyProperty.Register("CursorTime", typeof (DateTime?), typeof (TrendPen));

        public DateTime? CursorTime
        {
            get { return (DateTime?)GetValue(CursorTimeProperty); }
            set { SetValue(CursorTimeProperty, value); }
        }

        #endregion

        #region CursorValue

        public static readonly DependencyProperty CursorValueProperty =
            DependencyProperty.Register("CursorValue", typeof (double?), typeof (TrendPen));

        public double? CursorValue
        {
            get { return (double?)GetValue(CursorValueProperty); }
            set { SetValue(CursorValueProperty, value); }
        }

        #endregion

        #region MaxValue

        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register(
            "MaxValue", typeof (double?), typeof (TrendPen), new PropertyMetadata(OnMaxValueChanged));

        public double? MaxValue
        {
            get { return (double?)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        private static void OnMaxValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var max = e.NewValue as double?;
            if (max.HasValue && (double.IsNaN(max.Value) || double.IsInfinity(max.Value)))
                ((TrendPen)d).MaxValue = (double)e.OldValue;
        }

        #endregion

        #region MinValue

        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register(
            "MinValue", typeof (double?), typeof (TrendPen), new PropertyMetadata(OnMinValueChanged));

        public double? MinValue
        {
            get { return (double?)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        private static void OnMinValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var min = e.NewValue as double?;
            if (min.HasValue && (double.IsNaN(min.Value) || double.IsInfinity(min.Value)))
            {
                ((TrendPen)d).MinValue = (double)e.OldValue;
            }
        }

        #endregion

        #region MaxScale
        
        public double MaxScale
        {
            get { return (double)GetValue(MaxScaleProperty); }
            set { SetValue(MaxScaleProperty, value); }
        }

        public static readonly DependencyProperty MaxScaleProperty = DependencyProperty.Register(
            "MaxScale", typeof(double), typeof(TrendPen), new PropertyMetadata(1.0));
        
        #endregion

        #region MinScale
        
        public double MinScale
        {
            get { return (double)GetValue(MinScaleProperty); }
            set { SetValue(MinScaleProperty, value); }
        }

        public static readonly DependencyProperty MinScaleProperty = DependencyProperty.Register(
            "MinScale", typeof(double), typeof(TrendPen), new PropertyMetadata(0.0));

        #endregion
    }
}