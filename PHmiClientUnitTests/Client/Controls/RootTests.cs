using System;
using NUnit.Framework;
using PHmiClient.Controls;
using PHmiClient.Controls.Pages;

namespace PHmiClientUnitTests.Client.Controls
{
    public class WhenUsingRoot : Specification
    {
        protected IRoot Root;

        protected override void EstablishContext()
        {
            base.EstablishContext();
            Root = new Root();
        }

        public class AndShowObjInvoked : WhenUsingRoot
        {
            protected object Obj;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                Obj = new object();
                Root.Show(Obj);
            }

            public class ThenObjIsContent : AndShowObjInvoked
            {
                [Test]
                [STAThread]
                public void Test()
                {
                    Assert.That(Root.Content, Is.SameAs(Obj));
                }
            }

            public class AndSettingHomePage1 : AndShowObjInvoked
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    Root.HomePage = new object();
                }

                public class ThenContentStaisObj : AndSettingHomePage1
                {
                    [Test]
                    [STAThread]
                    public void Test()
                    {
                        Assert.That(Root.Content, Is.SameAs(Obj));
                    }
                }
            }
        }

        public class AndShowObjTypeInvoked : WhenUsingRoot
        {
            public class Obj
            {
            }

            protected Type Type;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                Type = typeof (Obj);
                Root.Show(Type);
            }

            public class ThenContentIsObjectOfType : AndShowObjTypeInvoked
            {
                [Test]
                [STAThread]
                public void Test()
                {
                    Assert.That(Root.Content, Is.InstanceOf<Obj>());
                }
            }
        }

        public class AndShowPageInvoked : WhenUsingRoot
        {
            public class Page : IPage
            {
                public IRoot Root { get; set; }
                public object PageName { get; private set; }
            }

            protected Page P;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                P = new Page();
                Root.Show(P);
            }

            public class ThenPageRootSetted : AndShowPageInvoked
            {
                [Test]
                [STAThread]
                public void Test()
                {
                    Assert.That(P.Root, Is.SameAs(Root));
                }
            }
        }

        public class AndGenericShowTypeInvoked : WhenUsingRoot
        {
            public class Obj
            {
            }

            protected override void EstablishContext()
            {
                base.EstablishContext();
                Root.Show<Obj>();
            }

            public class ThenContentIsObjectOfType : AndGenericShowTypeInvoked
            {
                [Test]
                [STAThread]
                public void Test()
                {
                    Assert.That(Root.Content, Is.InstanceOf<Obj>());
                }
            }
        }

        public class AndShowCommandExecuted : WhenUsingRoot
        {
            protected object Obj;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                Obj = new object();
                Root.ShowCommand.Execute(Obj);
            }

            public class ThenObjIsContent : AndShowCommandExecuted
            {
                [Test]
                [STAThread]
                public void Test()
                {
                    Assert.That(Root.Content, Is.SameAs(Obj));
                }
            }
        }

        public class ThenHomeCommandCanExecuteIsFalse : WhenUsingRoot
        {
            [Test]
            [STAThread]
            public void Test()
            {
                Assert.That(Root.HomeCommand.CanExecute(null), Is.False);
            }
        }

        public class AndSettingHomePage : WhenUsingRoot
        {
            protected object HomePage;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                HomePage = new object();
                Root.HomePage = HomePage;
            }

            public class ThenHomeCommandCanExecuteIsTrue : AndSettingHomePage
            {
                [Test]
                [STAThread]
                public void Test()
                {
                    Assert.That(Root.HomeCommand.CanExecute(null), Is.True);
                }
            }

            public class ThenContentIsHomePage : AndSettingHomePage
            {
                [Test]
                [STAThread]
                public void Test()
                {
                    Assert.That(Root.Content, Is.SameAs(HomePage));
                }
            }

            public class AndHomeCommandExecuted : AndSettingHomePage
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    Root.HomeCommand.Execute(null);
                }

                public class ThenContentIsHomePage1 : AndHomeCommandExecuted
                {
                    [Test]
                    [STAThread]
                    public void Test()
                    {
                        Assert.That(Root.Content, Is.SameAs(HomePage));
                    }
                }
            }
        }

        public class ThenBackCommandCanExecuteIsFalse : WhenUsingRoot
        {
            [Test]
            [STAThread]
            public void Test()
            {
                Assert.That(Root.BackCommand.CanExecute(null), Is.False);
            }
        }

        public class ThenForwardCommandCanExecuteIsFalse : WhenUsingRoot
        {
            [Test]
            [STAThread]
            public void Test()
            {
                Assert.That(Root.ForwardCommand.CanExecute(null), Is.False);
            }
        }

        public class AndShowTypeExecuted : WhenUsingRoot
        {
            public class Obj
            {
            }

            protected override void EstablishContext()
            {
                base.EstablishContext();
                Root.Show<Obj>();
            }

            public class ThenBackCommandCanExecuteIsFalse1 : AndShowTypeExecuted
            {
                [Test]
                [STAThread]
                public void Test()
                {
                    Assert.That(Root.BackCommand.CanExecute(null), Is.False);
                }
            }

            public class ThenForwardCommandCanExecuteIsFalse1 : AndShowTypeExecuted
            {
                [Test]
                [STAThread]
                public void Test()
                {
                    Assert.That(Root.ForwardCommand.CanExecute(null), Is.False);
                }
            }

            public class AndShowOtherTypeExecuted : AndShowTypeExecuted
            {
                public class Obj2
                {
                }

                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    Root.Show<Obj2>();
                }

                public class ThenBackCommandCanExecuteIsTrue : AndShowOtherTypeExecuted
                {
                    [Test]
                    [STAThread]
                    public void Test()
                    {
                        Assert.That(Root.BackCommand.CanExecute(null), Is.True);
                    }
                }

                public class ThenForwardCommandCanExecuteIsFalse2 : AndShowOtherTypeExecuted
                {
                    [Test]
                    [STAThread]
                    public void Test()
                    {
                        Assert.That(Root.ForwardCommand.CanExecute(null), Is.False);
                    }
                }

                public class AndBackCommandExecuted : AndShowOtherTypeExecuted
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        Root.BackCommand.Execute(null);
                    }

                    public class ThenContentIsObj : AndBackCommandExecuted
                    {
                        [Test]
                        [STAThread]
                        public void Test()
                        {
                            Assert.That(Root.Content, Is.InstanceOf<Obj>());
                        }
                    }

                    public class ThenBackCommandCanExecuteIsFalse2 : AndBackCommandExecuted
                    {
                        [Test]
                        [STAThread]
                        public void Test()
                        {
                            Assert.That(Root.BackCommand.CanExecute(null), Is.False);
                        }
                    }

                    public class ThenForwardCommandCanExecuteIsTrue : AndBackCommandExecuted
                    {
                        [Test]
                        [STAThread]
                        public void Test()
                        {
                            Assert.That(Root.ForwardCommand.CanExecute(null), Is.True);
                        }
                    }

                    public class AndForwardCommandExecuted : AndBackCommandExecuted
                    {
                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            Root.ForwardCommand.Execute(null);
                        }

                        public class ThenContentIsObj2 : AndForwardCommandExecuted
                        {
                            [Test]
                            [STAThread]
                            public void Test()
                            {
                                Assert.That(Root.Content, Is.InstanceOf<Obj2>());
                            }
                        }
                    }

                    public class AndShowTypeExecuted1 : AndBackCommandExecuted
                    {
                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            Root.Show<Obj2>();
                        }

                        public class ThenForwardCommandCanExecuteIsFalse3 : AndShowTypeExecuted1
                        {
                            [Test]
                            [STAThread]
                            public void Test()
                            {
                                Assert.That(Root.ForwardCommand.CanExecute(null), Is.False);
                            }
                        }
                    }
                }

                public class AndShowThirdTypeExecuted : AndShowOtherTypeExecuted
                {
                    public class Obj3
                    {
                    }

                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        Root.Show<Obj3>();
                    }

                    public class AndBackCommandExecuted1 : AndShowThirdTypeExecuted
                    {
                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            Root.BackCommand.Execute(null);
                        }

                        public class ThenBackCommandCanExecuteIsTrue1 : AndBackCommandExecuted1
                        {
                            [Test]
                            [STAThread]
                            public void Test()
                            {
                                Assert.That(Root.BackCommand.CanExecute(null), Is.True);
                            }
                        }

                        public class ThenForwardCommandCanExecuteIsTrue : AndBackCommandExecuted1
                        {
                            [Test]
                            [STAThread]
                            public void Test()
                            {
                                Assert.That(Root.ForwardCommand.CanExecute(null), Is.True);
                            }
                        }
                    }

                    public class AndObj2SetToHomePage : AndShowThirdTypeExecuted
                    {
                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            Root.HomePage = typeof(Obj2);
                        }

                        public class AndBackCommandExecuted2 : AndObj2SetToHomePage
                        {
                            protected override void EstablishContext()
                            {
                                base.EstablishContext();
                                Root.BackCommand.Execute(null);
                            }

                            public class ThenBackCommandCanExecuteIsFalse2 : AndBackCommandExecuted2
                            {
                                [Test]
                                [STAThread]
                                public void Test()
                                {
                                    Assert.That(Root.BackCommand.CanExecute(null), Is.False);
                                }
                            }

                            public class ThenForwardCommandCanExecuteIsTrue : AndBackCommandExecuted2
                            {
                                [Test]
                                [STAThread]
                                public void Test()
                                {
                                    Assert.That(Root.ForwardCommand.CanExecute(null), Is.True);
                                }
                            }
                        }
                    }

                    public class AndSetToHomePage : AndShowThirdTypeExecuted
                    {
                        public class HomePage
                        {
                        }

                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            Root.HomePage = typeof(HomePage);
                        }

                        public class AndShowHomePageExecuted : AndSetToHomePage
                        {
                            protected override void EstablishContext()
                            {
                                base.EstablishContext();
                                Root.Show<HomePage>();
                            }

                            public class ThenBackCommandCanExecuteIsFalse2 : AndShowHomePageExecuted
                            {
                                [Test]
                                [STAThread]
                                public void Test()
                                {
                                    Assert.That(Root.BackCommand.CanExecute(null), Is.False);
                                }
                            }

                            public class ThenForwardCommandCanExecuteIsFalse3 : AndShowHomePageExecuted
                            {
                                [Test]
                                [STAThread]
                                public void Test()
                                {
                                    Assert.That(Root.ForwardCommand.CanExecute(null), Is.False);
                                }
                            }
                        }
                    }
                }
            }
        }

        public class AndRegisteredForCommandsCanCanExecuteChanged : WhenUsingRoot
        {
            protected int HomeCount;
            protected int BackCount;
            protected int ForwardCount;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                HomeCount = 0;
                Root.HomeCommand.CanExecuteChanged += (sender, args) =>
                                                          {
                                                              HomeCount++;
                                                          };
                BackCount = 0;
                Root.BackCommand.CanExecuteChanged += (sender, args) =>
                                                          {
                                                              BackCount++;
                                                          };
                ForwardCount = 0;
                Root.ForwardCommand.CanExecuteChanged += (sender, args) =>
                                                             {
                                                                 ForwardCount++;
                                                             };
            }

            public class AndSettedHomePage : AndRegisteredForCommandsCanCanExecuteChanged
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    Root.HomePage = new object();
                }

                public class ThenHomeCommandCanExecuteChangedRaised : AndSettedHomePage
                {
                    [Test]
                    [STAThread]
                    public void Test()
                    {
                        Assert.That(HomeCount, Is.EqualTo(1));
                    }
                }
            }

            public class AndShowTypeInvoked : AndRegisteredForCommandsCanCanExecuteChanged
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    Root.Show<Object>();
                }

                public class ThenBackCommandCanExecuteChangedRaised : AndShowTypeInvoked
                {
                    [Test]
                    [STAThread]
                    public void Test()
                    {
                        Assert.That(BackCount, Is.EqualTo(1));
                    }
                }

                public class ThenForwardCommandCanExecuteChangedRaised : AndShowTypeInvoked
                {
                    [Test]
                    [STAThread]
                    public void Test()
                    {
                        Assert.That(ForwardCount, Is.EqualTo(1));
                    }
                }
            }
        }
    }
}
