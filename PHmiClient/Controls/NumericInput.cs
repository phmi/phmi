using PHmiClient.Controls.Input;
using PHmiClient.Tags;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PHmiClient.Controls
{
    [TemplatePart(Name = "textBox", Type = typeof(TextBox))]
    public class NumericInput : Control
    {
        private readonly string _numberDecimalSeparator;
        private TextBox _textBox;

        static NumericInput()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(NumericInput),
                new FrameworkPropertyMetadata(typeof(NumericInput)));
        }

        public NumericInput()
        {
            _numberDecimalSeparator = 1.1.ToString(CultureInfo.CurrentCulture)[1].ToString(CultureInfo.InvariantCulture);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _textBox = (TextBox)GetTemplateChild("textBox");
        }

        #region IsOpen

        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register("IsOpen", typeof(bool), typeof(NumericInput), new PropertyMetadata(OnIsOpenChanged));

        private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var opened = (bool) e.NewValue;
            if (!opened)
                return;
            var n = (NumericInput)d;
            n._textBox.Text = null;
            var tag = n.NumericTag;
            n._textBox.SelectedText = tag == null ? string.Empty : tag.Value.ToString();
            n.Dispatcher.BeginInvoke(new Action(() => n._textBox.Focus()));
        }

        #endregion

        #region NumericTag
        
        public INumericTag NumericTag
        {
            get { return (INumericTag)GetValue(NumericTagProperty); }
            set { SetValue(NumericTagProperty, value); }
        }

        public static readonly DependencyProperty NumericTagProperty =
            DependencyProperty.Register("NumericTag", typeof(INumericTag), typeof(NumericInput));

        #endregion

        #region PlusMinusCommand

        private ICommand _plusMinusCommand;

        public ICommand PlusMinusCommand
        {
            get { return _plusMinusCommand ?? (_plusMinusCommand = new DelegateCommand(PlusMinusCommandExecute)); }
        }

        private void PlusMinusCommandExecute(object obj)
        {
            var selectedText = _textBox.SelectedText;
            if (!string.IsNullOrEmpty(selectedText))
                _textBox.Text = _textBox.Text.Replace(selectedText, "");
            var selectionStart = _textBox.SelectionStart;
            if (string.IsNullOrEmpty(_textBox.Text))
            {
                _textBox.Text = "-";
                _textBox.SelectionStart = selectionStart + 1;
            }
            else if (_textBox.Text[0] == '-')
            {
                _textBox.Text = _textBox.Text.Substring(1);
                _textBox.SelectionStart = selectionStart - 1;
            }
            else
            {
                _textBox.Text = _textBox.Text.Insert(0, "-");
                _textBox.SelectionStart = selectionStart + 1;
            }
        }

        #endregion

        #region BackSpaceCommand

        private ICommand _backSpaceCommand;

        public ICommand BackSpaceCommand
        {
            get { return _backSpaceCommand ?? (_backSpaceCommand = new DelegateCommand(BackSpaceCommandExecuted)); }
        }

        private void BackSpaceCommandExecuted(object obj)
        {
            var selectedText = _textBox.SelectedText;
            if (!string.IsNullOrEmpty(selectedText))
                _textBox.Text = _textBox.Text.Replace(selectedText, "");
            var selectionStart = _textBox.SelectionStart;
            if (selectionStart > 0)
            {
                _textBox.Text = _textBox.Text.Remove(selectionStart - 1, 1);
                _textBox.SelectionStart = selectionStart - 1;
            }
        }

        #endregion

        #region CloseCommand

        private ICommand _closeCommand;

        public ICommand CloseCommand
        {
            get { return _closeCommand ?? (_closeCommand = new DelegateCommand(CloseCommandExecuted)); }
        }

        private void CloseCommandExecuted(object obj)
        {
            IsOpen = false;
        }

        #endregion

        #region EnterCommand

        private ICommand _enterCommand;

        public ICommand EnterCommand
        {
            get { return _enterCommand ?? (_enterCommand = new DelegateCommand(EnterCommandExecuted)); }
        }

        private void EnterCommandExecuted(object obj)
        {
            if (!string.IsNullOrEmpty(_textBox.Text))
            {
                double value;
                if (double.TryParse(_textBox.Text.Replace(".", _numberDecimalSeparator).Replace(",", _numberDecimalSeparator), out value))
                {
                    var tag = NumericTag;
                    if (tag != null)
                    {
                        tag.Value = value;
                    }
                }
            }
            CloseCommand.Execute(null);
        }

        #endregion

        #region ClearCommand

        private ICommand _clearCommand;

        public ICommand ClearCommand
        {
            get { return _clearCommand ?? (_clearCommand = new DelegateCommand(ClearCommandExecuted)); }
        }

        private void ClearCommandExecuted(object obj)
        {
            _textBox.Text = null;
        }

        #endregion

        #region EnterSymbolCommand

        private ICommand _enterSymbolCommand;

        public ICommand EnterSymbolCommand
        {
            get { return _enterSymbolCommand ?? (_enterSymbolCommand = new DelegateCommand(EnterSymbolCommandExecuted)); }
        }

        private void EnterSymbolCommandExecuted(object obj)
        {
            var selectedText = _textBox.SelectedText;
            if (!string.IsNullOrEmpty(selectedText))
            {
                _textBox.Text = _textBox.Text.Replace(selectedText, string.Empty);
            }
            var selectionStart = _textBox.SelectionStart;
            var symbol = obj.ToString();
            _textBox.Text = _textBox.Text.Insert(selectionStart, symbol);
            _textBox.SelectionStart = selectionStart + symbol.Length;
        }

        #endregion

        #region EnterPointCommand

        private ICommand _enterPointCommand;

        public ICommand EnterPointCommand
        {
            get { return _enterPointCommand ?? (_enterPointCommand = new DelegateCommand(EnterPointCommandExecuted)); }
        }

        private void EnterPointCommandExecuted(object obj)
        {
            EnterSymbolCommand.Execute(_numberDecimalSeparator);
        }

        #endregion
    }
}
