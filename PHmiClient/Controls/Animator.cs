using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Threading;

namespace PHmiClient.Controls
{
    [ContentProperty("Contents")]
    public class Animator : ContentControl
    {
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private int _index;

        #region Animate property

        public static readonly DependencyProperty AnimateProperty =
            DependencyProperty.Register("Animate", typeof(bool?), typeof(Animator),
                                        new PropertyMetadata(AnimateChangedCallback));

        public bool? Animate
        {
            get { return GetValue(AnimateProperty) as bool?; }
            set { SetValue(AnimateProperty, value); }
        }

        private static void AnimateChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var animator = (Animator)d;
            animator.UpdateAnimate(e.NewValue as bool?);
        }

        #endregion

        public Animator()
        {
            IsTabStop = false;
            AnimationPeriod = new TimeSpan(0, 0, 0, 1);
            Contents = new List<object>();
            _timer.Tick += TimerTick;
            Loaded += AnimatorLoaded;
        }

        public TimeSpan AnimationPeriod
        {
            get { return _timer.Interval; }
            set { _timer.Interval = value; }
        }

        public List<object> Contents { get; set; }

        public object DefaultContent { get; set; }

        private void AnimatorLoaded(object sender, RoutedEventArgs e)
        {
            Content = DefaultContent;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (Contents == null)
                return;
            if (_index >= Contents.Count)
                _index = 0;
            if (Contents.Count > 0)
                Content = Contents[_index];
            _index++;
        }

        private void UpdateAnimate(bool? newValue)
        {
            if (newValue == true)
            {
                _timer.Start();
                TimerTick(null, null);
            }
            else
            {
                _timer.Stop();
                Content = DefaultContent;
                _index = 0;
            }
        }
    }
}
