using System;
using System.Windows;
using System.Windows.Controls;

namespace PHmiClient.Controls
{
    public class TimeSpanPicker : Control
    {
        static TimeSpanPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimeSpanPicker), new FrameworkPropertyMetadata(typeof(TimeSpanPicker)));
        }

        #region TimeSpan
        
        public TimeSpan? TimeSpan
        {
            get { return (TimeSpan?)GetValue(TimeSpanProperty); }
            set { SetValue(TimeSpanProperty, value); }
        }

        public static readonly DependencyProperty TimeSpanProperty =
            DependencyProperty.Register("TimeSpan", typeof(TimeSpan?), typeof(TimeSpanPicker),
            new PropertyMetadata(TimeSpanPropertyChanged));

        private static void TimeSpanPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var p = (TimeSpanPicker) d;
            p.Update(e.NewValue as TimeSpan?);
        }

        #endregion

        private void Update(TimeSpan? newValue)
        {
            if (!newValue.HasValue)
            {
                Days = TimeSpanDays = null;
                Hours = TimeSpanHours = null;
                Minutes = TimeSpanMinutes = null;
                Seconds = TimeSpanSeconds = null;
                Milliseconds = TimeSpanMilliseconds = null;
                return;
            }
            var timeSpan = newValue.Value;

            Days = TimeSpanDays = timeSpan.Days;
            Hours = TimeSpanHours = timeSpan.Hours;
            Minutes = TimeSpanMinutes = timeSpan.Minutes;
            Seconds = TimeSpanSeconds = timeSpan.Seconds;
            Milliseconds = TimeSpanMilliseconds = timeSpan.Milliseconds;
        }

        #region Days

        public int? Days
        {
            get { return (int?)GetValue(DaysProperty); }
            set { SetValue(DaysProperty, value); }
        }

        public static readonly DependencyProperty DaysProperty =
            DependencyProperty.Register("Days", typeof(int?), typeof(TimeSpanPicker),
            new PropertyMetadata(DaysPropertyChanged));

        private static void DaysPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var p = (TimeSpanPicker) d;
            var newDay = e.NewValue as int?;
            if (newDay.HasValue && (newDay.Value < 0))
            {
                p.Days = e.OldValue as int?;
            }
            else
            {
                p.UpdateDays(e.NewValue as int?);
            }
        }

        #endregion

        private void UpdateDays(int? newDays)
        {
            if (!newDays.HasValue)
            {
                TimeSpan = null;
                return;
            }
            var days = newDays.Value;
            var timeSpan = TimeSpan.HasValue ? TimeSpan.Value : new TimeSpan();
            var difference = System.TimeSpan.FromDays(days - timeSpan.Days);
            TimeSpan = timeSpan + difference;
        }

        #region TimeSpanDays
        
        public int? TimeSpanDays
        {
            get { return (int?)GetValue(TimeSpanDaysProperty); }
            set { SetValue(TimeSpanDaysProperty, value); }
        }

        public static readonly DependencyProperty TimeSpanDaysProperty =
            DependencyProperty.Register("TimeSpanDays", typeof(int?), typeof(TimeSpanPicker));

        #endregion

        #region Hours
        
        public int? Hours
        {
            get { return (int?)GetValue(HoursProperty); }
            set { SetValue(HoursProperty, value); }
        }

        public static readonly DependencyProperty HoursProperty =
            DependencyProperty.Register("Hours", typeof(int?), typeof(TimeSpanPicker),
            new PropertyMetadata(HoursPropertyChanged));

        private static void HoursPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var p = (TimeSpanPicker)d;
            var newHours = e.NewValue as int?;
            if (newHours.HasValue && (newHours.Value < 0 || newHours.Value > 23))
            {
                p.Days = e.OldValue as int?;
            }
            else
            {
                p.UpdateHours(e.NewValue as int?);
            }
        }

        #endregion
        
        private void UpdateHours(int? newHours)
        {
            if (!newHours.HasValue)
            {
                TimeSpan = null;
                return;
            }
            var hours = newHours.Value;
            var timeSpan = TimeSpan.HasValue ? TimeSpan.Value : new TimeSpan();
            var difference = System.TimeSpan.FromHours(hours - timeSpan.Hours);
            TimeSpan = timeSpan + difference;
        }

        #region TimeSpanHours

        public int? TimeSpanHours
        {
            get { return (int?)GetValue(TimeSpanHoursProperty); }
            set { SetValue(TimeSpanHoursProperty, value); }
        }

        public static readonly DependencyProperty TimeSpanHoursProperty =
            DependencyProperty.Register("TimeSpanHours", typeof(int?), typeof(TimeSpanPicker));

        #endregion

        #region Minutes

        public int? Minutes
        {
            get { return (int?)GetValue(MinutesProperty); }
            set { SetValue(MinutesProperty, value); }
        }

        public static readonly DependencyProperty MinutesProperty =
            DependencyProperty.Register("Minutes", typeof(int?), typeof(TimeSpanPicker),
            new PropertyMetadata(MinutesPropertyChanged));

        private static void MinutesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var p = (TimeSpanPicker)d;
            var newMinutes = e.NewValue as int?;
            if (newMinutes.HasValue && (newMinutes.Value < 0 || newMinutes.Value > 59))
            {
                p.Days = e.OldValue as int?;
            }
            else
            {
                p.UpdateMinutes(e.NewValue as int?);
            }
        }

        #endregion

        private void UpdateMinutes(int? newMinutes)
        {
            if (!newMinutes.HasValue)
            {
                TimeSpan = null;
                return;
            }
            var minutes = newMinutes.Value;
            var timeSpan = TimeSpan.HasValue ? TimeSpan.Value : new TimeSpan();
            var difference = System.TimeSpan.FromMinutes(minutes - timeSpan.Minutes);
            TimeSpan = timeSpan + difference;
        }

        #region TimeSpanMinutes

        public int? TimeSpanMinutes
        {
            get { return (int?)GetValue(TimeSpanMinutesProperty); }
            set { SetValue(TimeSpanMinutesProperty, value); }
        }

        public static readonly DependencyProperty TimeSpanMinutesProperty =
            DependencyProperty.Register("TimeSpanMinutes", typeof(int?), typeof(TimeSpanPicker));

        #endregion

        #region Seconds

        public int? Seconds
        {
            get { return (int?)GetValue(SecondsProperty); }
            set { SetValue(SecondsProperty, value); }
        }

        public static readonly DependencyProperty SecondsProperty =
            DependencyProperty.Register("Seconds", typeof(int?), typeof(TimeSpanPicker),
            new PropertyMetadata(SecondsPropertyChanged));

        private static void SecondsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var p = (TimeSpanPicker)d;
            var newSeconds = e.NewValue as int?;
            if (newSeconds.HasValue && (newSeconds.Value < 0 || newSeconds.Value > 59))
            {
                p.Days = e.OldValue as int?;
            }
            else
            {
                p.UpdateSeconds(e.NewValue as int?);
            }
        }

        #endregion

        private void UpdateSeconds(int? newSeconds)
        {
            if (!newSeconds.HasValue)
            {
                TimeSpan = null;
                return;
            }
            var seconds = newSeconds.Value;
            var timeSpan = TimeSpan.HasValue ? TimeSpan.Value : new TimeSpan();
            var difference = System.TimeSpan.FromSeconds(seconds - timeSpan.Seconds);
            TimeSpan = timeSpan + difference;
        }

        #region TimeSpanSeconds

        public int? TimeSpanSeconds
        {
            get { return (int?)GetValue(TimeSpanSecondsProperty); }
            set { SetValue(TimeSpanSecondsProperty, value); }
        }

        public static readonly DependencyProperty TimeSpanSecondsProperty =
            DependencyProperty.Register("TimeSpanSeconds", typeof(int?), typeof(TimeSpanPicker));

        #endregion

        #region Milliseconds

        public int? Milliseconds
        {
            get { return (int?)GetValue(MillisecondsProperty); }
            set { SetValue(MillisecondsProperty, value); }
        }

        public static readonly DependencyProperty MillisecondsProperty =
            DependencyProperty.Register("Milliseconds", typeof(int?), typeof(TimeSpanPicker),
            new PropertyMetadata(MillisecondsPropertyChanged));

        private static void MillisecondsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var p = (TimeSpanPicker)d;
            var newMilliseconds = e.NewValue as int?;
            if (newMilliseconds.HasValue && (newMilliseconds.Value < 0 || newMilliseconds.Value > 59))
            {
                p.Days = e.OldValue as int?;
            }
            else
            {
                p.UpdateMilliseconds(e.NewValue as int?);
            }
        }

        #endregion

        private void UpdateMilliseconds(int? newMilliseconds)
        {
            if (!newMilliseconds.HasValue)
            {
                TimeSpan = null;
                return;
            }
            var milliseconds = newMilliseconds.Value;
            var timeSpan = TimeSpan.HasValue ? TimeSpan.Value : new TimeSpan();
            var difference = System.TimeSpan.FromMilliseconds(milliseconds - timeSpan.Milliseconds);
            TimeSpan = timeSpan + difference;
        }

        #region TimeSpanMilliseconds

        public int? TimeSpanMilliseconds
        {
            get { return (int?)GetValue(TimeSpanMillisecondsProperty); }
            set { SetValue(TimeSpanMillisecondsProperty, value); }
        }

        public static readonly DependencyProperty TimeSpanMillisecondsProperty =
            DependencyProperty.Register("TimeSpanMilliseconds", typeof(int?), typeof(TimeSpanPicker));

        #endregion
    }
}
