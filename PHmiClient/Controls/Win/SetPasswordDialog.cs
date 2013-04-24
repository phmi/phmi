using System;
using System.Windows;
using System.Windows.Controls;
using PHmiClient.Users;

namespace PHmiClient.Controls.Win
{
    [TemplatePart(Name = "pswb", Type = typeof(PasswordBox))]
    public class SetPasswordDialog : DialogBase
    {
        private readonly IUsers _users;
        private readonly User _user;
        private PasswordBox _pswb;
        
        static SetPasswordDialog()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(SetPasswordDialog),
                new FrameworkPropertyMetadata(typeof(SetPasswordDialog)));
        }

        public SetPasswordDialog(IUsers users, User user)
        {
            _users = users;
            _user = user;
        }

        public User User
        {
            get { return _user; }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _pswb = (PasswordBox)GetTemplateChild("pswb");
            if (_pswb != null)
            {
                _pswb.Focus();
            }
            _pswb = (PasswordBox)GetTemplateChild("pswb");
        }

        protected override void OkCommandExecuted(object obj)
        {
            StartLoading();
            _users.SetPassword(_user.Id, _pswb.Password, r => Dispatcher.Invoke(new Action(() =>
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
