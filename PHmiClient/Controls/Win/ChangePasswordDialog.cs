using System;
using System.Windows;
using System.Windows.Controls;
using PHmiClient.PHmiSystem;

namespace PHmiClient.Controls.Win
{
    [TemplatePart(Name = "pswbOld", Type = typeof(PasswordBox))]
    [TemplatePart(Name = "pswbNew", Type = typeof(PasswordBox))]
    public class ChangePasswordDialog : DialogBase
    {
        private readonly PHmiAbstract _pHmi;
        private PasswordBox _pswbOld;
        private PasswordBox _pswbNew;

        static ChangePasswordDialog()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ChangePasswordDialog),
                new FrameworkPropertyMetadata(typeof(ChangePasswordDialog)));
        }

        public ChangePasswordDialog(PHmiAbstract pHmi)
        {
            _pHmi = pHmi;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _pswbOld = (PasswordBox)GetTemplateChild("pswbOld");
            if (_pswbOld != null)
            {
                _pswbOld.Focus();
            }
            _pswbNew = (PasswordBox)GetTemplateChild("pswbNew");
        }

        protected override void OkCommandExecuted(object obj)
        {
            StartLoading();
            _pHmi.Users.ChangePassword(_pswbOld.Password, _pswbNew.Password, r => Dispatcher.Invoke(new Action(() =>
                {
                    EndLoading(r);
                    if (!r)
                    {
                        _pswbOld.Focus();
                    }
                })));
        }
    }
}
