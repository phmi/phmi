using System;
using System.Windows;
using System.Windows.Controls;
using PHmiClient.Loc;
using PHmiClient.Users;

namespace PHmiClient.Controls.Win
{
    [TemplatePart(Name = "tbName", Type = typeof (TextBox))]
    [TemplatePart(Name = "tbError", Type = typeof (TextBlock))]
    public class AddUserDialog : DialogBase
    {
        private readonly IUsers _users;
        private readonly User _user;
        private TextBox _tb;
        private TextBlock _tbError;

        static AddUserDialog()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(AddUserDialog),
                new FrameworkPropertyMetadata(typeof(AddUserDialog)));
        }

        public AddUserDialog(IUsers users, long id)
        {
            _users = users;
            _user = new User { Id = id };
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _tb = (TextBox) GetTemplateChild("tbName");
            if (_tb != null)
            {
                _tb.Focus();
            }
            _tbError = (TextBlock) GetTemplateChild("tbError");
        }

        public User User
        {
            get { return _user; }
        }

        protected override void OkCommandExecuted(object obj)
        {
            StartLoading();
            _users.InsertUser(_user, r => Dispatcher.Invoke(new Action(() =>
                {
                    EndLoading(r == InsertUserResult.Success);
                    if (r != InsertUserResult.Success)
                    {
                        _tb.Focus();
                    }
                    switch (r)
                    {
                        case InsertUserResult.NameConflict:
                            _tbError.Text = Res.NameConflict;
                            break;
                        case InsertUserResult.IdConflict:
                            _tbError.Text = Res.IdConflict;
                            break;
                        case InsertUserResult.NullValue:
                            _tbError.Text = Res.EmptyValue;
                            break;
                        default:
                            _tbError.Text = Res.Error;
                            break;
                    }
                })));
        }
    }
}
