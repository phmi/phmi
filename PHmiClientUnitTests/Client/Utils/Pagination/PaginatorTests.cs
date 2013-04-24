using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using PHmiClient.Utils;
using PHmiClient.Utils.Pagination;

namespace PHmiClientUnitTests.Client.Utils.Pagination
{
    public class WhenUsingPaginator : Specification
    {
        public class Item
        {
            public Item(int criteria, bool value)
            {
                Criteria = criteria;
                Value = value;
            }

            public int Criteria { get; private set; }
            public bool Value { get; set; }
        }

        protected IPaginator<Item, int> Paginator;

        protected override void EstablishContext()
        {
            base.EstablishContext();
            Paginator = new Paginator<Item, int>(
                item => item.Criteria,
                (item, newItem) => item.Value = newItem.Value)
                {
                    PageSize = 3
                };
        }

        public class ThenUpUpCommandCanExecuteIsFalse : WhenUsingPaginator
        {
            [Test]
            public void Test()
            {
                Assert.That(Paginator.UpUpCommand.CanExecute(null), Is.False);
            }
        }

        public class ThenUpCommandCanExecuteIsFalse : WhenUsingPaginator
        {
            [Test]
            public void Test()
            {
                Assert.That(Paginator.UpCommand.CanExecute(null), Is.False);
            }
        }

        public class ThenDownCommandCanExecuteIsFalse : WhenUsingPaginator
        {
            [Test]
            public void Test()
            {
                Assert.That(Paginator.DownCommand.CanExecute(null), Is.False);
            }
        }

        public class ThenDownDownCommandCanExecuteIsFalse : WhenUsingPaginator
        {
            [Test]
            public void Test()
            {
                Assert.That(Paginator.DownDownCommand.CanExecute(null), Is.False);
            }
        }

        public class ThenRefreshCommandCanExecuteIsFalse : WhenUsingPaginator
        {
            [Test]
            public void Test()
            {
                Assert.That(Paginator.RefreshCommand.CanExecute(null), Is.False);
            }
        }

        public class ThenCancelCommandCanExecuteIsFalse : WhenUsingPaginator
        {
            [Test]
            public void Test()
            {
                Assert.That(Paginator.CancelCommand.CanExecute(null), Is.False);
            }
        }

        public class ThenBusyIsFalse : WhenUsingPaginator
        {
            [Test]
            public void Test()
            {
                Assert.That(Paginator.Busy, Is.False);
            }
        }

        public class ThenCriteriaTypeIsDownFromInfinity : WhenUsingPaginator
        {
            [Test]
            public void Test()
            {
                Assert.That(Paginator.CriteriaType, Is.EqualTo(CriteriaType.DownFromInfinity));
            }
        }

        public class AndPaginationServiceSetted : WhenUsingPaginator
        {
            protected Mock<IPaginationService<Item, int>>  PaginationService;
            protected Action<IEnumerable<Item>> GetItemsCallback;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                PaginationService = new Mock<IPaginationService<Item, int>>();
                PaginationService
                    .Setup(s => s.GetItems(It.IsAny<CriteriaType>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Action<IEnumerable<Item>>>()))
                    .Callback(new Action<CriteriaType, int, int, Action<IEnumerable<Item>>>(MockGetItemsCallback));
                Paginator.PaginationService = PaginationService.Object;
            }

            private void MockGetItemsCallback(CriteriaType criteriaType, int maxCount, int criteria, Action<IEnumerable<Item>> callback)
            {
                GetItemsCallback = callback;
            }

            public class ThenRefreshCanExecuteIsTrue : AndPaginationServiceSetted
            {
                [Test]
                public void Test()
                {
                    Assert.That(Paginator.RefreshCommand.CanExecute(null), Is.True);
                }
            }

            public class AndRefreshCommandExecuted : AndPaginationServiceSetted
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    Paginator.RefreshCommand.Execute(null);
                }

                public class ThenPaginationServiceGetItemsInvokedWithCriteriaTypeDownFromInfinity : AndRefreshCommandExecuted
                {
                    [Test]
                    public void Test()
                    {
                        PaginationService.Verify(s => s.GetItems(CriteriaType.DownFromInfinity, Paginator.PageSize, It.IsAny<int>(), It.IsAny<Action<IEnumerable<Item>>>()), Times.Once());
                    }
                }

                public class ThenBusyIsTrue : AndRefreshCommandExecuted
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(Paginator.Busy, Is.True);
                    }
                }

                public class AndCallbackInvoked : AndRefreshCommandExecuted
                {
                    protected Item Item1;
                    protected Item Item2;

                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        Item1 = new Item(1, false);
                        Item2 = new Item(2, false);
                        GetItemsCallback(new []{Item1, Item2});
                    }

                    public class ThenBusyIsFalse1 : AndCallbackInvoked
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.Busy, Is.False);
                        }
                    }

                    public class ThenItemsAreUpdated : AndCallbackInvoked
                    {
                        [Test]
                        public void Test()
                        {
                            CollectionAssert.AreEqual(Paginator.Items, new []{Item1, Item2});
                        }
                    }

                    public class AndRefreshExecuted1 : AndCallbackInvoked
                    {
                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            Paginator.RefreshCommand.Execute(null);
                        }

                        public class AndCallbackInvoked1 : AndRefreshExecuted1
                        {
                            protected Item Item21;
                            protected Item Item3;

                            protected override void EstablishContext()
                            {
                                base.EstablishContext();
                                Item21 = new Item(2, true);
                                Item3 = new Item(3, false);
                                GetItemsCallback(new[] { Item21, Item3 });
                            }

                            public class ThenItemsUpdated : AndCallbackInvoked1
                            {
                                [Test]
                                public void Test()
                                {
                                    CollectionAssert.AreEqual(Paginator.Items, new []{Item2, Item3});
                                    Assert.That(Item2.Value, Is.True);
                                }
                            }
                        }
                    }

                    public class ThenRefreshCanExecuteIsTrue1 : AndCallbackInvoked
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.RefreshCommand.CanExecute(null), Is.True);
                        }
                    }
                }

                public class AndCallbackReturnsPageSizeElementCount : AndRefreshCommandExecuted
                {
                    protected Item Item1;
                    protected Item Item2;
                    protected Item Item3;

                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        Item1 = new Item(1, false);
                        Item2 = new Item(2, false);
                        Item3 = new Item(3, false);
                        GetItemsCallback(new[] { Item1, Item2, Item3 });
                    }

                    public class ThenDownCommandCanExecuteIsTrue : AndCallbackReturnsPageSizeElementCount
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.DownCommand.CanExecute(null), Is.True);
                        }
                    }

                    public class ThenUpCommandCanExecuteIsFalse1 : AndCallbackReturnsPageSizeElementCount
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.UpCommand.CanExecute(null), Is.False);
                        }
                    }

                    public class AndPageSizeChangedToLessThanItemsCount : AndCallbackReturnsPageSizeElementCount
                    {
                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            Paginator.PageSize = 2;
                        }

                        public class ThenItemsIsTrimmed : AndPageSizeChangedToLessThanItemsCount
                        {
                            [Test]
                            public void Test()
                            {
                                CollectionAssert.AreEqual(new[] { Item1, Item2 }, Paginator.Items);
                            }
                        }
                    }

                    public class ThenCriteriaIsSetToUpperElementCriteria : AndCallbackReturnsPageSizeElementCount
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.Criteria, Is.EqualTo(Item1.Criteria));
                        }
                    }
                }

                public class AndCallbackReturnsLessThanPageSizeElementCount : AndRefreshCommandExecuted
                {
                    protected Item Item1;
                    protected Item Item2;

                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        Item1 = new Item(1, false);
                        Item2 = new Item(2, false);
                        GetItemsCallback(new[] { Item1, Item2 });
                    }

                    public class ThenDownCommandCanExecuteIsFalse1 : AndCallbackReturnsLessThanPageSizeElementCount
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.DownCommand.CanExecute(null), Is.False);
                        }
                    }

                    public class ThenUpCommandCanExecuteIsFalse1 : AndCallbackReturnsLessThanPageSizeElementCount
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.UpCommand.CanExecute(null), Is.False);
                        }
                    }

                    public class ThenCriteriaIsSetToUpperElementCriteria : AndCallbackReturnsLessThanPageSizeElementCount
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.Criteria, Is.EqualTo(Item1.Criteria));
                        }
                    }
                }

                public class AndCallbackReturns0Elements : AndRefreshCommandExecuted
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        GetItemsCallback(new Item[0]);
                    }

                    public class ThenDownCommandCanExecuteIsFalse1 : AndCallbackReturns0Elements
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.DownCommand.CanExecute(null), Is.False);
                        }
                    }

                    public class ThenUpCommandCanExecuteIsFalse1 : AndCallbackReturns0Elements
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.UpCommand.CanExecute(null), Is.False);
                        }
                    }
                }
            }

            public class ThenUpUpCanExecuteIsTrue : AndPaginationServiceSetted
            {
                [Test]
                public void Test()
                {
                    Assert.That(Paginator.UpUpCommand.CanExecute(null), Is.True);
                }
            }

            public class AndUpUpCommandExecuted : AndPaginationServiceSetted
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    Paginator.UpUpCommand.Execute(null);
                }

                public class ThenPaginationServiceGetItemsInvokedWithCriteriaTypeDownFromInfinity : AndUpUpCommandExecuted
                {
                    [Test]
                    public void Test()
                    {
                        PaginationService.Verify(s => s.GetItems(CriteriaType.DownFromInfinity, Paginator.PageSize, It.IsAny<int>(), It.IsAny<Action<IEnumerable<Item>>>()), Times.Once());
                    }
                }
            }

            public class ThenDownDownCanExecuteIsTrue : AndPaginationServiceSetted
            {
                [Test]
                public void Test()
                {
                    Assert.That(Paginator.DownDownCommand.CanExecute(null), Is.True);
                }
            }

            public class AndDownDownCommandExecuted : AndPaginationServiceSetted
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    Paginator.DownDownCommand.Execute(null);
                }

                public class ThenPaginationServiceGetItemsInvokedWithCriteriaTypeUpFromInfinity : AndDownDownCommandExecuted
                {
                    [Test]
                    public void Test()
                    {
                        PaginationService.Verify(s => s.GetItems(CriteriaType.UpFromInfinity, Paginator.PageSize, It.IsAny<int>(), It.IsAny<Action<IEnumerable<Item>>>()), Times.Once());
                    }
                }
            }

            public class AndRefreshInvokedWithTypeDownFrom : AndPaginationServiceSetted
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    Paginator.Refresh(CriteriaType.DownFrom, -1);
                }

                public class ThenCriteriaTypeStays : AndRefreshInvokedWithTypeDownFrom
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(Paginator.CriteriaType, Is.EqualTo(CriteriaType.DownFromInfinity));
                    }
                }

                public class ThenCriteriaStays : AndRefreshInvokedWithTypeDownFrom
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(Paginator.Criteria, Is.EqualTo(0));
                    }
                }

                public class ThenPaginationServiceGetItemsInvokedWithCriteriaTypeDownFrom : AndRefreshInvokedWithTypeDownFrom
                {
                    [Test]
                    public void Test()
                    {
                        PaginationService.Verify(s => s.GetItems(CriteriaType.DownFrom, Paginator.PageSize, It.IsAny<int>(), It.IsAny<Action<IEnumerable<Item>>>()), Times.Once());
                    }
                }

                public class AndCallbackReturnsPageSizeElementCount : AndRefreshInvokedWithTypeDownFrom
                {
                    protected Item Item1;
                    protected Item Item2;
                    protected Item Item3;

                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        Item1 = new Item(1, false);
                        Item2 = new Item(2, false);
                        Item3 = new Item(3, false);
                        GetItemsCallback(new[] { Item1, Item2, Item3 });
                    }

                    public class ThenCriteriaTypeUpdates : AndCallbackReturnsPageSizeElementCount
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.CriteriaType, Is.EqualTo(CriteriaType.DownFrom));
                        }
                    }

                    public class ThenCriteriaUpdates : AndCallbackReturnsPageSizeElementCount
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.Criteria, Is.EqualTo(-1));
                        }
                    }

                    public class ThenDownCommandCanExecuteIsTrue : AndCallbackReturnsPageSizeElementCount
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.DownCommand.CanExecute(null), Is.True);
                        }
                    }

                    public class ThenUpCommandCanExecuteIsTrue : AndCallbackReturnsPageSizeElementCount
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.UpCommand.CanExecute(null), Is.True);
                        }
                    }

                    public class AndPaginationServiceIsSettedToNull : AndCallbackReturnsPageSizeElementCount
                    {
                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            Paginator.PaginationService = null;
                        }

                        public class ThenDownCommandCanExecuteIsFalse1 : AndPaginationServiceIsSettedToNull
                        {
                            [Test]
                            public void Test()
                            {
                                Assert.That(Paginator.DownCommand.CanExecute(null), Is.False);
                            }
                        }

                        public class ThenUpCommandCanExecuteIsFalse1 : AndPaginationServiceIsSettedToNull
                        {
                            [Test]
                            public void Test()
                            {
                                Assert.That(Paginator.UpCommand.CanExecute(null), Is.False);
                            }
                        }
                    }

                    public class AndPageSizeChangedToMoreThanItemsCount : AndCallbackReturnsPageSizeElementCount
                    {
                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            Paginator.PageSize = 10;
                        }

                        public class ThenItemsStays : AndPageSizeChangedToMoreThanItemsCount
                        {
                            [Test]
                            public void Test()
                            {
                                CollectionAssert.AreEqual(new[] { Item1, Item2, Item3 }, Paginator.Items);
                            }
                        }
                    }

                    public class AndPageSizeChangedToLessThanItemsCount : AndCallbackReturnsPageSizeElementCount
                    {
                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            Paginator.PageSize = 2;
                        }

                        public class ThenItemsIsTrimmed : AndPageSizeChangedToLessThanItemsCount
                        {
                            [Test]
                            public void Test()
                            {
                                CollectionAssert.AreEqual(new[] { Item1, Item2 }, Paginator.Items);
                            }
                        }
                    }
                }

                public class AndCallbackReturnsLessThanPageSizeElementCount : AndRefreshInvokedWithTypeDownFrom
                {
                    protected Item Item1;
                    protected Item Item2;

                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        Item1 = new Item(1, false);
                        Item2 = new Item(2, false);
                        GetItemsCallback(new[] { Item1, Item2 });
                    }

                    public class ThenDownCommandCanExecuteIsFalse1 : AndCallbackReturnsLessThanPageSizeElementCount
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.DownCommand.CanExecute(null), Is.False);
                        }
                    }

                    public class ThenUpCommandCanExecuteIsTrue : AndCallbackReturnsLessThanPageSizeElementCount
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.UpCommand.CanExecute(null), Is.True);
                        }
                    }
                }

                public class AndCallbackReturns0Elements : AndRefreshInvokedWithTypeDownFrom
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        GetItemsCallback(new Item[0]);
                    }

                    public class ThenDownCommandCanExecuteIsFalse1 : AndCallbackReturns0Elements
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.DownCommand.CanExecute(null), Is.False);
                        }
                    }

                    public class ThenUpCommandCanExecuteIsTrue : AndCallbackReturns0Elements
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.UpCommand.CanExecute(null), Is.True);
                        }
                    }

                    public class AndUpCommandExecuted : AndCallbackReturns0Elements
                    {
                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            Paginator.UpCommand.Execute(null);
                        }

                        public class ThenPaginationServiceGetItemsInvokedWithCriteriaTypeUpFromOrEqual : AndUpCommandExecuted
                        {
                            [Test]
                            public void Test()
                            {
                                PaginationService.Verify(s => s.GetItems(CriteriaType.UpFromOrEqual, Paginator.PageSize, It.IsAny<int>(), It.IsAny<Action<IEnumerable<Item>>>()), Times.Once());
                            }
                        }
                    }
                }
            }

            public class AndRefreshInvokedWithTypeDownFromOrEqual : AndPaginationServiceSetted
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    Paginator.Refresh(CriteriaType.DownFromOrEqual, -1);
                }

                public class ThenPaginationServiceGetItemsInvokedWithCriteriaTypeDownFromOrEqual : AndRefreshInvokedWithTypeDownFromOrEqual
                {
                    [Test]
                    public void Test()
                    {
                        PaginationService.Verify(s => s.GetItems(CriteriaType.DownFromOrEqual, Paginator.PageSize, It.IsAny<int>(), It.IsAny<Action<IEnumerable<Item>>>()), Times.Once());
                    }
                }

                public class AndCallbackReturnsPageSizeElementCount : AndRefreshInvokedWithTypeDownFromOrEqual
                {
                    protected Item Item1;
                    protected Item Item2;
                    protected Item Item3;

                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        Item1 = new Item(1, false);
                        Item2 = new Item(2, false);
                        Item3 = new Item(3, false);
                        GetItemsCallback(new[] { Item1, Item2, Item3 });
                    }

                    public class ThenCriteriaTypeUpdates : AndCallbackReturnsPageSizeElementCount
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.CriteriaType, Is.EqualTo(CriteriaType.DownFromOrEqual));
                        }
                    }

                    public class ThenCriteriaUpdates : AndCallbackReturnsPageSizeElementCount
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.Criteria, Is.EqualTo(-1));
                        }
                    }

                    public class ThenDownCommandCanExecuteIsTrue : AndCallbackReturnsPageSizeElementCount
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.DownCommand.CanExecute(null), Is.True);
                        }
                    }

                    public class ThenUpCommandCanExecuteIsTrue : AndCallbackReturnsPageSizeElementCount
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.UpCommand.CanExecute(null), Is.True);
                        }
                    }

                    public class AndPaginationServiceIsSettedToNull : AndCallbackReturnsPageSizeElementCount
                    {
                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            Paginator.PaginationService = null;
                        }

                        public class ThenDownCommandCanExecuteIsFalse1 : AndPaginationServiceIsSettedToNull
                        {
                            [Test]
                            public void Test()
                            {
                                Assert.That(Paginator.DownCommand.CanExecute(null), Is.False);
                            }
                        }

                        public class ThenUpCommandCanExecuteIsFalse1 : AndPaginationServiceIsSettedToNull
                        {
                            [Test]
                            public void Test()
                            {
                                Assert.That(Paginator.UpCommand.CanExecute(null), Is.False);
                            }
                        }
                    }

                    public class AndPageSizeChangedToMoreThanItemsCount : AndCallbackReturnsPageSizeElementCount
                    {
                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            Paginator.PageSize = 10;
                        }

                        public class ThenItemsStays : AndPageSizeChangedToMoreThanItemsCount
                        {
                            [Test]
                            public void Test()
                            {
                                CollectionAssert.AreEqual(new[] { Item1, Item2, Item3 }, Paginator.Items);
                            }
                        }
                    }

                    public class AndPageSizeChangedToLessThanItemsCount : AndCallbackReturnsPageSizeElementCount
                    {
                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            Paginator.PageSize = 2;
                        }

                        public class ThenItemsIsTrimmed : AndPageSizeChangedToLessThanItemsCount
                        {
                            [Test]
                            public void Test()
                            {
                                CollectionAssert.AreEqual(new[] { Item1, Item2 }, Paginator.Items);
                            }
                        }
                    }
                }

                public class AndCallbackReturnsLessThanPageSizeElementCount : AndRefreshInvokedWithTypeDownFromOrEqual
                {
                    protected Item Item1;
                    protected Item Item2;

                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        Item1 = new Item(1, false);
                        Item2 = new Item(2, false);
                        GetItemsCallback(new[] { Item1, Item2 });
                    }

                    public class ThenDownCommandCanExecuteIsFalse1 : AndCallbackReturnsLessThanPageSizeElementCount
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.DownCommand.CanExecute(null), Is.False);
                        }
                    }

                    public class ThenUpCommandCanExecuteIsTrue : AndCallbackReturnsLessThanPageSizeElementCount
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.UpCommand.CanExecute(null), Is.True);
                        }
                    }
                }

                public class AndCallbackReturns0Elements : AndRefreshInvokedWithTypeDownFromOrEqual
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        GetItemsCallback(new Item[0]);
                    }

                    public class ThenDownCommandCanExecuteIsFalse1 : AndCallbackReturns0Elements
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.DownCommand.CanExecute(null), Is.False);
                        }
                    }

                    public class ThenUpCommandCanExecuteIsTrue : AndCallbackReturns0Elements
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.UpCommand.CanExecute(null), Is.True);
                        }
                    }

                    public class AndUpCommandExecuted : AndCallbackReturns0Elements
                    {
                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            Paginator.UpCommand.Execute(null);
                        }

                        public class ThenPaginationServiceGetItemsInvokedWithCriteriaTypeUpFrom : AndUpCommandExecuted
                        {
                            [Test]
                            public void Test()
                            {
                                PaginationService.Verify(s => s.GetItems(CriteriaType.UpFrom, Paginator.PageSize, It.IsAny<int>(), It.IsAny<Action<IEnumerable<Item>>>()), Times.Once());
                            }
                        }
                    }
                }
            }

            public class AndRefreshInvokedWithTypeUpFromOrEqual : AndPaginationServiceSetted
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    Paginator.Refresh(CriteriaType.UpFromOrEqual, 100);
                }

                public class ThenPaginationServiceGetItemsInvokedWithCriteriaTypeUpFromOrEqual : AndRefreshInvokedWithTypeUpFromOrEqual
                {
                    [Test]
                    public void Test()
                    {
                        PaginationService.Verify(s => s.GetItems(CriteriaType.UpFromOrEqual, Paginator.PageSize, 100, It.IsAny<Action<IEnumerable<Item>>>()), Times.Once());
                    }
                }

                public class AndCallbackReturnsPageSizeElementCount : AndRefreshInvokedWithTypeUpFromOrEqual
                {
                    protected Item Item1;
                    protected Item Item2;
                    protected Item Item3;

                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        Item1 = new Item(1, false);
                        Item2 = new Item(2, false);
                        Item3 = new Item(3, false);
                        GetItemsCallback(new[] { Item1, Item2, Item3 });
                    }

                    public class ThenDownCommandCanExecuteIsTrue : AndCallbackReturnsPageSizeElementCount
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.DownCommand.CanExecute(null), Is.True);
                        }
                    }

                    public class ThenUpCommandCanExecuteIsTrue : AndCallbackReturnsPageSizeElementCount
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.UpCommand.CanExecute(null), Is.True);
                        }
                    }

                    public class AndDownCommandExecuted : AndCallbackReturnsPageSizeElementCount
                    {
                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            Paginator.DownCommand.Execute(null);
                        }

                        public class ThenRefreshInvoked : AndDownCommandExecuted
                        {
                            [Test]
                            public void Test()
                            {
                                PaginationService.Verify(s => s.GetItems(CriteriaType.DownFrom, Paginator.PageSize, Item3.Criteria, It.IsAny<Action<IEnumerable<Item>>>()), Times.Once());
                            }
                        }
                    }

                    public class AndUpCommandExecuted : AndCallbackReturnsPageSizeElementCount
                    {
                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            Paginator.UpCommand.Execute(null);
                        }

                        public class ThenCriteriaTypeIsSetToUpFromOrEqual : AndUpCommandExecuted
                        {
                            [Test]
                            public void Test()
                            {
                                Assert.That(Paginator.CriteriaType, Is.EqualTo(CriteriaType.UpFromOrEqual));
                            }
                        }

                        public class ThenRefreshInvoked : AndUpCommandExecuted
                        {
                            [Test]
                            public void Test()
                            {
                                PaginationService.Verify(s => s.GetItems(CriteriaType.UpFrom, Paginator.PageSize, Item1.Criteria, It.IsAny<Action<IEnumerable<Item>>>()), Times.Once());
                            }
                        }
                    }

                    public class AndPageSizeChangedToLessThanItemsCount : AndCallbackReturnsPageSizeElementCount
                    {
                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            Paginator.PageSize = 2;
                        }

                        public class ThenItemsIsTrimmed : AndPageSizeChangedToLessThanItemsCount
                        {
                            [Test]
                            public void Test()
                            {
                                CollectionAssert.AreEqual(new[] { Item2, Item3 }, Paginator.Items);
                            }
                        }
                    }
                }

                public class AndCallbackReturnsLessThanPageSizeElementCount : AndRefreshInvokedWithTypeUpFromOrEqual
                {
                    protected Item Item1;
                    protected Item Item2;

                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        Item1 = new Item(1, false);
                        Item2 = new Item(2, false);
                        GetItemsCallback(new[] { Item1, Item2 });
                    }

                    public class ThenDownCommandCanExecuteIsTrue : AndCallbackReturnsLessThanPageSizeElementCount
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.DownCommand.CanExecute(null), Is.True);
                        }
                    }

                    public class ThenUpCommandCanExecuteIsFalse1 : AndCallbackReturnsLessThanPageSizeElementCount
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.UpCommand.CanExecute(null), Is.False);
                        }
                    }
                }

                public class AndCallbackReturns0Elements : AndRefreshInvokedWithTypeUpFromOrEqual
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        GetItemsCallback(new Item[0]);
                    }

                    public class ThenDownCommandCanExecuteIsTrue : AndCallbackReturns0Elements
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.DownCommand.CanExecute(null), Is.True);
                        }
                    }

                    public class AndDownCommandExecuted : AndCallbackReturns0Elements
                    {
                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            Paginator.DownCommand.Execute(null);
                        }

                        public class ThenPaginationServiceGetItemsInvokedWithCriteriaTypeDownFrom : AndDownCommandExecuted
                        {
                            [Test]
                            public void Test()
                            {
                                PaginationService.Verify(s => s.GetItems(CriteriaType.DownFrom, Paginator.PageSize, It.IsAny<int>(), It.IsAny<Action<IEnumerable<Item>>>()), Times.Once());
                            }
                        }
                    }

                    public class ThenUpCommandCanExecuteIsFalse1 : AndCallbackReturns0Elements
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.UpCommand.CanExecute(null), Is.False);
                        }
                    }
                }

                public class ThenCancelCommandCanExecuteIsTrue : AndRefreshInvokedWithTypeUpFromOrEqual
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(Paginator.CancelCommand.CanExecute(null), Is.True);
                    }
                }

                public class AndCancelExecuted : AndRefreshInvokedWithTypeUpFromOrEqual
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        Paginator.CancelCommand.Execute(null);
                    }

                    public class ThenBusyIsFalse1 : AndCancelExecuted
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.Busy, Is.False);
                        }
                    }

                    public class AndCallbackInvoked : AndCancelExecuted
                    {
                        protected Item Item1;
                        protected Item Item2;
                        protected Item Item3;

                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            Item1 = new Item(1, false);
                            Item2 = new Item(2, false);
                            Item3 = new Item(3, false);
                            GetItemsCallback(new[] { Item1, Item2, Item3 });
                        }

                        public class ThenNotApplied : AndCallbackInvoked
                        {
                            [Test]
                            public void Test()
                            {
                                Assert.That(Paginator.Items, Is.Empty);
                            }
                        }
                    }
                }
            }

            public class AndRefreshInvokedWithTypeUpFrom : AndPaginationServiceSetted
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    Paginator.Refresh(CriteriaType.UpFrom, 100);
                }

                public class ThenPaginationServiceGetItemsInvokedWithCriteriaTypeUpFrom : AndRefreshInvokedWithTypeUpFrom
                {
                    [Test]
                    public void Test()
                    {
                        PaginationService.Verify(s => s.GetItems(CriteriaType.UpFrom, Paginator.PageSize, 100, It.IsAny<Action<IEnumerable<Item>>>()), Times.Once());
                    }
                }

                public class AndCallbackReturnsPageSizeElementCount : AndRefreshInvokedWithTypeUpFrom
                {
                    protected Item Item1;
                    protected Item Item2;
                    protected Item Item3;

                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        Item1 = new Item(1, false);
                        Item2 = new Item(2, false);
                        Item3 = new Item(3, false);
                        GetItemsCallback(new[] { Item1, Item2, Item3 });
                    }

                    public class ThenDownCommandCanExecuteIsTrue : AndCallbackReturnsPageSizeElementCount
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.DownCommand.CanExecute(null), Is.True);
                        }
                    }

                    public class ThenUpCommandCanExecuteIsTrue : AndCallbackReturnsPageSizeElementCount
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.UpCommand.CanExecute(null), Is.True);
                        }
                    }

                    public class AndDownCommandExecuted : AndCallbackReturnsPageSizeElementCount
                    {
                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            Paginator.DownCommand.Execute(null);
                        }

                        public class ThenRefreshInvoked : AndDownCommandExecuted
                        {
                            [Test]
                            public void Test()
                            {
                                PaginationService.Verify(s => s.GetItems(CriteriaType.DownFrom, Paginator.PageSize, Item3.Criteria, It.IsAny<Action<IEnumerable<Item>>>()), Times.Once());
                            }
                        }
                    }

                    public class AndUpCommandExecuted : AndCallbackReturnsPageSizeElementCount
                    {
                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            Paginator.UpCommand.Execute(null);
                        }

                        public class ThenCriteriaTypeIsSetToUpFrom : AndUpCommandExecuted
                        {
                            [Test]
                            public void Test()
                            {
                                Assert.That(Paginator.CriteriaType, Is.EqualTo(CriteriaType.UpFrom));
                            }
                        }

                        public class ThenRefreshInvoked : AndUpCommandExecuted
                        {
                            [Test]
                            public void Test()
                            {
                                PaginationService.Verify(s => s.GetItems(CriteriaType.UpFrom, Paginator.PageSize, Item1.Criteria, It.IsAny<Action<IEnumerable<Item>>>()), Times.Once());
                            }
                        }
                    }

                    public class AndPageSizeChangedToLessThanItemsCount : AndCallbackReturnsPageSizeElementCount
                    {
                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            Paginator.PageSize = 2;
                        }

                        public class ThenItemsIsTrimmed : AndPageSizeChangedToLessThanItemsCount
                        {
                            [Test]
                            public void Test()
                            {
                                CollectionAssert.AreEqual(new[] { Item2, Item3 }, Paginator.Items);
                            }
                        }
                    }
                }

                public class AndCallbackReturnsLessThanPageSizeElementCount : AndRefreshInvokedWithTypeUpFrom
                {
                    protected Item Item1;
                    protected Item Item2;

                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        Item1 = new Item(1, false);
                        Item2 = new Item(2, false);
                        GetItemsCallback(new[] { Item1, Item2 });
                    }

                    public class ThenDownCommandCanExecuteIsTrue : AndCallbackReturnsLessThanPageSizeElementCount
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.DownCommand.CanExecute(null), Is.True);
                        }
                    }

                    public class ThenUpCommandCanExecuteIsFalse1 : AndCallbackReturnsLessThanPageSizeElementCount
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.UpCommand.CanExecute(null), Is.False);
                        }
                    }
                }

                public class AndCallbackReturns0Elements : AndRefreshInvokedWithTypeUpFrom
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        GetItemsCallback(new Item[0]);
                    }

                    public class ThenDownCommandCanExecuteIsTrue : AndCallbackReturns0Elements
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.DownCommand.CanExecute(null), Is.True);
                        }
                    }

                    public class AndDownCommandExecuted : AndCallbackReturns0Elements
                    {
                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            Paginator.DownCommand.Execute(null);
                        }

                        public class ThenPaginationServiceGetItemsInvokedWithCriteriaTypeDownFromOrEqual : AndDownCommandExecuted
                        {
                            [Test]
                            public void Test()
                            {
                                PaginationService.Verify(s => s.GetItems(CriteriaType.DownFromOrEqual, Paginator.PageSize, It.IsAny<int>(), It.IsAny<Action<IEnumerable<Item>>>()), Times.Once());
                            }
                        }
                    }

                    public class ThenUpCommandCanExecuteIsFalse1 : AndCallbackReturns0Elements
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.UpCommand.CanExecute(null), Is.False);
                        }
                    }
                }

                public class ThenCancelCommandCanExecuteIsTrue : AndRefreshInvokedWithTypeUpFrom
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(Paginator.CancelCommand.CanExecute(null), Is.True);
                    }
                }

                public class AndCancelExecuted : AndRefreshInvokedWithTypeUpFrom
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        Paginator.CancelCommand.Execute(null);
                    }

                    public class ThenBusyIsFalse1 : AndCancelExecuted
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.Busy, Is.False);
                        }
                    }

                    public class AndCallbackInvoked : AndCancelExecuted
                    {
                        protected Item Item1;
                        protected Item Item2;
                        protected Item Item3;

                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            Item1 = new Item(1, false);
                            Item2 = new Item(2, false);
                            Item3 = new Item(3, false);
                            GetItemsCallback(new[] { Item1, Item2, Item3 });
                        }

                        public class ThenNotApplied : AndCallbackInvoked
                        {
                            [Test]
                            public void Test()
                            {
                                Assert.That(Paginator.Items, Is.Empty);
                            }
                        }
                    }
                }
            }

            public class AndRefreshInvokedWithTypeUpFromInfinity : AndPaginationServiceSetted
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    Paginator.Refresh(CriteriaType.UpFromInfinity, 0);
                }

                public class ThenPaginationServiceGetItemsInvokedWithCriteriaTypeUpFromInfinity : AndRefreshInvokedWithTypeUpFromInfinity
                {
                    [Test]
                    public void Test()
                    {
                        PaginationService.Verify(s => s.GetItems(CriteriaType.UpFromInfinity, Paginator.PageSize, It.IsAny<int>(), It.IsAny<Action<IEnumerable<Item>>>()), Times.Once());
                    }
                }

                public class AndCallbackReturnsPageSizeElementCount : AndRefreshInvokedWithTypeUpFromInfinity
                {
                    protected Item Item1;
                    protected Item Item2;
                    protected Item Item3;

                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        Item1 = new Item(1, false);
                        Item2 = new Item(2, false);
                        Item3 = new Item(3, false);
                        GetItemsCallback(new[] { Item1, Item2, Item3 });
                    }

                    public class ThenDownCommandCanExecuteIsFalse1 : AndCallbackReturnsPageSizeElementCount
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.DownCommand.CanExecute(null), Is.False);
                        }
                    }

                    public class ThenUpCommandCanExecuteIsTrue : AndCallbackReturnsPageSizeElementCount
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.UpCommand.CanExecute(null), Is.True);
                        }
                    }

                    public class AndPageSizeChangedToLessThanItemsCount : AndCallbackReturnsPageSizeElementCount
                    {
                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            Paginator.PageSize = 2;
                        }

                        public class ThenItemsIsTrimmed : AndPageSizeChangedToLessThanItemsCount
                        {
                            [Test]
                            public void Test()
                            {
                                CollectionAssert.AreEqual(new[] { Item2, Item3 }, Paginator.Items);
                            }
                        }
                    }

                    public class ThenCriteriaIsSetToLowerElementCriteria : AndCallbackReturnsPageSizeElementCount
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.Criteria, Is.EqualTo(Item3.Criteria));
                        }
                    }
                }

                public class AndCallbackReturnsLessThanPageSizeElementCount : AndRefreshInvokedWithTypeUpFromInfinity
                {
                    protected Item Item1;
                    protected Item Item2;

                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        Item1 = new Item(1, false);
                        Item2 = new Item(2, false);
                        GetItemsCallback(new[] { Item1, Item2 });
                    }

                    public class ThenDownCommandCanExecuteIsFalse1 : AndCallbackReturnsLessThanPageSizeElementCount
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.DownCommand.CanExecute(null), Is.False);
                        }
                    }

                    public class ThenUpCommandCanExecuteIsFalse1 : AndCallbackReturnsLessThanPageSizeElementCount
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.UpCommand.CanExecute(null), Is.False);
                        }
                    }

                    public class ThenCriteriaIsSetToLowerElementCriteria : AndCallbackReturnsLessThanPageSizeElementCount
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.Criteria, Is.EqualTo(Item2.Criteria));
                        }
                    }
                }

                public class AndCallbackReturns0Elements : AndRefreshInvokedWithTypeUpFromInfinity
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        GetItemsCallback(new Item[0]);
                    }

                    public class ThenDownCommandCanExecuteIsFalse1 : AndCallbackReturns0Elements
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.DownCommand.CanExecute(null), Is.False);
                        }
                    }

                    public class ThenUpCommandCanExecuteIsFalse1 : AndCallbackReturns0Elements
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Paginator.UpCommand.CanExecute(null), Is.False);
                        }
                    }
                }
            }


        }
    }
}
