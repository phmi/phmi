using Moq;
using NUnit.Framework;
using PHmiClient.Loc;
using PHmiClient.Users;
using PHmiClient.Wcf;

namespace PHmiClientUnitTests.Client.Users
{
    public class WhenUsingUsersRunTarget : Specification
    {
        internal IUsersRunTarget UsersRunTarget;

        protected override void EstablishContext()
        {
            base.EstablishContext();
            UsersRunTarget = new UsersRunTarget();
        }

        public class ThenNameReturnsUsersService : WhenUsingUsersRunTarget
        {
            [Test]
            public void Test()
            {
                Assert.That(UsersRunTarget.Name, Is.EqualTo(Res.UsersService));
            }
        }

        public class AndInvokingLogOn : WhenUsingUsersRunTarget
        {
            protected string Name;
            protected string Password;
            protected User Result;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                Name = "Name";
                Password = "Password";
                UsersRunTarget.LogOn(Name, Password, Callback);
            }

            private void Callback(User result)
            {
                Result = result;
            }

            public class AndServiceReturnsUser : AndInvokingLogOn
            {
                internal Mock<IService> Service;
                internal User User;

                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    Service = new Mock<IService>();
                    User = new User();
                    Service.Setup(s => s.LogOn(Name, Password)).Returns(User);
                }

                public class AndInvokingRun : AndServiceReturnsUser
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        UsersRunTarget.Run(Service.Object);
                    }

                    public class ThenActionIsInvokedPositively : AndInvokingRun
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Result, Is.SameAs(User));
                        }
                    }
                }

                public class AndInvokingClean : AndServiceReturnsUser
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        UsersRunTarget.Clean();
                    }

                    public class AndInvokingRun1 : AndInvokingClean
                    {
                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            UsersRunTarget.Run(Service.Object);
                        }

                        public class ThenActionIsNotInvoked : AndInvokingRun1
                        {
                            [Test]
                            public void Test()
                            {
                                Service.Verify(s => s.LogOn(Name, Password), Times.Never());
                            }
                        }
                    }
                }
            }

            public class AndServiceReturnsNoUser : AndInvokingLogOn
            {
                internal Mock<IService> Service;

                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    Service = new Mock<IService>();
                }

                public class AndInvokingRun : AndServiceReturnsNoUser
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        UsersRunTarget.Run(Service.Object);
                    }

                    public class ThenActionIsInvokedNegatively : AndInvokingRun
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Result, Is.Null);
                        }
                    }
                }
            }
        }
    }
}
