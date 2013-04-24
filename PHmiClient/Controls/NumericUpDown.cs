using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PHmiClient.Controls.Input;

namespace PHmiClient.Controls
{
    public class NumericUpDown : Control
    {
        private readonly DelegateCommand _upCommand;
        private readonly DelegateCommand _downCommand;

        static NumericUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericUpDown), new FrameworkPropertyMetadata(typeof(NumericUpDown)));
        }

        public NumericUpDown()
        {
            _upCommand = new DelegateCommand(UpCommandExecuted, UpCommandCanExecute);
            _downCommand = new DelegateCommand(DownCommandExecuted, DownCommandCanExecute);
        }

        #region Minimum
        
        public int Minimum
        {
            get { return (int)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(int), typeof(NumericUpDown),
            new PropertyMetadata(int.MinValue, OnMinimumPropertyChanged));

        private static void OnMinimumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var upDown = (NumericUpDown)d;
            upDown._downCommand.RaiseCanExecuteChanged();
        }

        #endregion

        #region Maximum

        public int Maximum
        {
            get { return (int)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(int), typeof(NumericUpDown),
            new PropertyMetadata(int.MaxValue, OnMaximumPropertyChanged));

        private static void OnMaximumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var upDown = (NumericUpDown) d;
            upDown._upCommand.RaiseCanExecuteChanged();
        }

        #endregion

        #region Value

        public int? Value
        {
            get { return (int?)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int?), typeof(NumericUpDown), new PropertyMetadata(OnValueCountChanged));

        private static void OnValueCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var upDown = (NumericUpDown)d;
            var newValue = (int?)e.NewValue;
            if (newValue < upDown.Minimum)
            {
                upDown.Value = upDown.Minimum;
            }
            if (newValue > upDown.Maximum)
            {
                upDown.Value = upDown.Maximum;
            }
            upDown._downCommand.RaiseCanExecuteChanged();
            upDown._upCommand.RaiseCanExecuteChanged();
        }

        #endregion

        #region UpCommand

        public ICommand UpCommand
        {
            get { return _upCommand; }
        }

        private bool UpCommandCanExecute(object obj)
        {
            return Value < Maximum;
        }

        private void UpCommandExecuted(object obj)
        {
            Value += 1;
        }

        #endregion

        #region DownCommand

        public ICommand DownCommand
        {
            get { return _downCommand; }
        }

        private bool DownCommandCanExecute(object obj)
        {
            return Value > Minimum;
        }

        private void DownCommandExecuted(object obj)
        {
            Value -= 1;
        }

        #endregion
    }
}
