using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using PHmiClient.Converters;
using PHmiClient.Users;

namespace PHmiClientUnitTests.Client.Users
{
    public class WhenUsingUsers : Specification
    {
        internal Mock<IUsersRunTarget> UsersRunTarget;
        internal IUsers Users;
        protected int CurrentPropertyChangedCount;

        protected override void EstablishContext()
        {
            base.EstablishContext();
            UsersRunTarget = new Mock<IUsersRunTarget>();
            Users = new PHmiClient.Users.Users(UsersRunTarget.Object);
            CurrentPropertyChangedCount = 0;
            Users.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == "Current")
                        CurrentPropertyChangedCount++;
                };
        }

        public class AndRegisteringLogOn : WhenUsingUsers
        {
            protected string Name;
            protected string Password;
            protected bool? CallbackResult;
            protected List<Action<User>> UserActions;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                CallbackResult = null;
                Name = "Name";
                Password = "Password";
                UserActions = new List<Action<User>>();
                UsersRunTarget
                    .Setup(t => t.LogOn(Name, PasswordConverter.ConvertBack(Password), It.IsAny<Action<User>>()))
                    .Callback<string, string, Action<User>>(LogOnCallback);
                Users.LogOn(Name, Password, Callback);
            }

            protected void Callback(bool result)
            {
                CallbackResult = result;
            }

            private void LogOnCallback(string name, string password, Action<User> callback)
            {
                UserActions.Add(callback);
            }

            public class ThenPasswordHashed : AndRegisteringLogOn
            {
                [Test]
                public void Test()
                {
                    UsersRunTarget.Verify(t => t.LogOn(Name, PasswordConverter.ConvertBack(Password),
                         It.IsAny<Action<User>>()), Times.Once());
                }
            }

            public class AndCallingCallbackWithSuccess : AndRegisteringLogOn
            {
                protected User User;

                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    User = new User {Name = Name};
                    UserActions.Single().Invoke(User);
                }

                public class ThenCallbackResultIsTrue : AndCallingCallbackWithSuccess
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(CallbackResult, Is.True);
                    }
                }

                public class ThenCurrentIsUser : AndCallingCallbackWithSuccess
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(Users.Current, Is.SameAs(User));
                    }
                }

                public class AndRegisteringLogOnAgain : AndCallingCallbackWithSuccess
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        Name = "Name2";
                        Password = "Password2";
                        UserActions = new List<Action<User>>();
                        UsersRunTarget
                            .Setup(t => t.LogOn(Name, PasswordConverter.ConvertBack(Password), It.IsAny<Action<User>>()))
                            .Callback<string, string, Action<User>>(LogOnCallback);
                        Users.LogOn(Name, Password, Callback);
                    }

                    public class AndCallingCallbackWithFailThisTime : AndRegisteringLogOnAgain
                    {
                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            UserActions.Single().Invoke(null);
                        }

                        public class ThenCallbackResultIsFalse : AndCallingCallbackWithFailThisTime
                        {
                            [Test]
                            public void Test()
                            {
                                Assert.That(CallbackResult, Is.False);
                            }
                        }

                        public class ThenCurrentStaysUser : AndCallingCallbackWithFailThisTime
                        {
                            [Test]
                            public void Test()
                            {
                                Assert.That(Users.Current, Is.SameAs(User));
                            }
                        }
                    }
                }

                public class AndInvokingLogOff : AndCallingCallbackWithSuccess
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        Users.LogOff();
                    }

                    public class ThenCurrentIsNull : AndInvokingLogOff
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Users.Current, Is.Null);
                        }
                    }

                    public class ThenCurrentChangedRaised1 : AndInvokingLogOff
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(CurrentPropertyChangedCount, Is.EqualTo(2));
                        }
                    }
                }

                public class ThenCurrentChangedRaised : AndCallingCallbackWithSuccess
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(CurrentPropertyChangedCount, Is.EqualTo(1));
                    }
                }

                public class AndRegisteringPasswordChange1 : AndCallingCallbackWithSuccess
                {
                    protected string OldPassword;
                    protected string NewPassword;
                    protected bool? CallbackResult1;
                    protected List<Action<bool>> PswChangeActions;

                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        CallbackResult1 = null;
                        OldPassword = "OldPassword";
                        NewPassword = "NewPassword";
                        PswChangeActions = new List<Action<bool>>();
                        UsersRunTarget
                            .Setup(t => t.ChangePassword(Name, PasswordConverter.ConvertBack(OldPassword), PasswordConverter.ConvertBack(NewPassword), It.IsAny<Action<bool>>()))
                            .Callback<string, string, string, Action<bool>>(ChangePasswordCallback);
                        Users.ChangePassword(OldPassword, NewPassword, Callback1);
                    }

                    protected void Callback1(bool result)
                    {
                        CallbackResult1 = result;
                    }

                    private void ChangePasswordCallback(string name, string oldPassword, string newPassword, Action<bool> callback)
                    {
                        PswChangeActions.Add(callback);
                    }

                    public class ThenPasswordHashed1 : AndRegisteringPasswordChange1
                    {
                        [Test]
                        public void Test()
                        {
                            UsersRunTarget.Verify(t => t.ChangePassword(Name, PasswordConverter.ConvertBack(OldPassword),
                                PasswordConverter.ConvertBack(NewPassword), It.IsAny<Action<bool>>()), Times.Once());
                        }
                    }

                    public class AndCallingCallbackWithSuccess1 : AndRegisteringPasswordChange1
                    {
                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            PswChangeActions.Single().Invoke(true);
                        }

                        public class ThenCallbackResultIsTrue1 : AndCallingCallbackWithSuccess1
                        {
                            [Test]
                            public void Test()
                            {
                                Assert.That(CallbackResult1, Is.True);
                            }
                        }

                        public class AndRegisteringPasswordChange1Again : AndCallingCallbackWithSuccess1
                        {
                            protected override void EstablishContext()
                            {
                                base.EstablishContext();
                                OldPassword = "Password2";
                                NewPassword = "NewPassword2";
                                PswChangeActions = new List<Action<bool>>();
                                UsersRunTarget
                                    .Setup(t => t.ChangePassword(
                                        Name,
                                        PasswordConverter.ConvertBack(OldPassword),
                                        PasswordConverter.ConvertBack(NewPassword),
                                        It.IsAny<Action<bool>>()))
                                    .Callback<string, string, string, Action<bool>>(ChangePasswordCallback);
                                Users.ChangePassword(OldPassword, NewPassword, Callback1);
                            }

                            public class AndCallingCallbackWithFailThisTime : AndRegisteringPasswordChange1Again
                            {
                                protected override void EstablishContext()
                                {
                                    base.EstablishContext();
                                    PswChangeActions.Single().Invoke(false);
                                }

                                public class ThenCallbackResultIsFalse : AndCallingCallbackWithFailThisTime
                                {
                                    [Test]
                                    public void Test()
                                    {
                                        Assert.That(CallbackResult1, Is.False);
                                    }
                                }
                            }
                        }
                    }

                    public class AndCallingCallbackWithFail1 : AndRegisteringPasswordChange1
                    {
                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            PswChangeActions.Single().Invoke(false);
                        }

                        public class ThenCallbackResultIsFalse : AndCallingCallbackWithFail1
                        {
                            [Test]
                            public void Test()
                            {
                                Assert.That(CallbackResult1, Is.False);
                            }
                        }
                    }
                }
            }

            public class AndCallingCallbackWithFail : AndRegisteringLogOn
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    UserActions.Single().Invoke(null);
                }

                public class ThenCallbackResultIsFalse : AndCallingCallbackWithFail
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(CallbackResult, Is.False);
                    }
                }

                public class ThenCurrentIsUser : AndCallingCallbackWithFail
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(Users.Current, Is.Null);
                    }
                }
            }
        }

        public class AndRegisteringPasswordChange : WhenUsingUsers
        {
            protected string Name;
            protected string OldPassword;
            protected string NewPassword;
            protected bool? CallbackResult;
            protected List<Action<bool>> PswChangeActions;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                CallbackResult = null;
                Name = "Name";
                OldPassword = "OldPassword";
                NewPassword = "NewPassword";
                PswChangeActions = new List<Action<bool>>();
                Users.ChangePassword(OldPassword, NewPassword, Callback);
            }

            protected void Callback(bool result)
            {
                CallbackResult = result;
            }

            public class ThenResultIsFalse : AndRegisteringPasswordChange
            {
                [Test]
                public void Test()
                {
                    Assert.That(CallbackResult, Is.EqualTo(false));
                }
            }
        }
    }
}
