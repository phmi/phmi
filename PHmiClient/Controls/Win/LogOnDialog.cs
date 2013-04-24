using PHmiClient.PHmiSystem;
using System;
using System.Windows;
using System.Windows.Controls;

namespace PHmiClient.Controls.Win
{
    [TemplatePart(Name = "tbName", Type = typeof(TextBox))]
    [TemplatePart(Name = "pswb", Type = typeof(PasswordBox))]
    public class LogOnDialog : DialogBase
    {
        private readonly PHmiAbstract _pHmi;
        private TextBox _tbName;
        private PasswordBox _pswb;

        static LogOnDialog()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(LogOnDialog),
                new FrameworkPropertyMetadata(typeof(LogOnDialog)));
        }

        public LogOnDialog(PHmiAbstract pHmi)
        {
            _pHmi = pHmi;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _tbName = (TextBox) GetTemplateChild("tbName");
            if (_tbName != null)
            {
                _tbName.Focus();
            }
            _pswb = (PasswordBox) GetTemplateChild("pswb");
        }
        
        protected override void OkCommandExecuted(object obj)
        {
            StartLoading();
            _pHmi.Users.LogOn(_tbName.Text, _pswb.Password, r => Dispatcher.Invoke(new Action(() =>
                {
                    EndLoading(r);
                    if (!r)
                    {
                        _pswb.Focus();
                    }
                })));
        }
    }
}
