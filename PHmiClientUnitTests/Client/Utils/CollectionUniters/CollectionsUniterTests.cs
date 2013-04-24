using NUnit.Framework;
using PHmiClient.Utils.CollectionUniters;
using System.Collections.ObjectModel;
using System.Linq;

namespace PHmiClientUnitTests.Client.Utils.CollectionUniters
{
    public class WhenUsingCollectionUniter : Specification
    {
        protected CollectionUniter<int> Uniter;

        protected override void EstablishContext()
        {
            base.EstablishContext();
            Uniter = new CollectionUniter<int>((i1, i2) => i1.CompareTo(i2));
        }

        public class AndAddingCollection : WhenUsingCollectionUniter
        {
            protected ObservableCollection<int> Collection;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                Collection = new ObservableCollection<int>(new []
                    {
                        5, 3, 6, 5
                    });
                Uniter.Collections.Add(Collection);
            }

            public class ThenItems : AndAddingCollection
            {
                [Test]
                public void Inserted()
                {
                    CollectionAssert.AreEquivalent(Collection, Uniter);
                }

                [Test]
                public void Ordered()
                {
                    CollectionAssert.IsOrdered(Uniter);
                }
            }

            public class AndAddedToCollection : AndAddingCollection
            {
                protected int AddedValue = 1;

                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    Collection.Add(AddedValue);
                }

                public class ThenAddedToUniter : AndAddedToCollection
                {
                    [Test]
                    public void Test()
                    {
                        Assert.Contains(AddedValue, Uniter);
                    }

                    [Test]
                    public void InOrder()
                    {
                        CollectionAssert.IsOrdered(Uniter);
                    }
                }
            }

            public class AndRemovedFromCollection : AndAddingCollection
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    Collection.RemoveAt(Collection.Count - 1);
                }

                public class ThenRemovedFromUniter : AndRemovedFromCollection
                {
                    [Test]
                    public void Test()
                    {
                        CollectionAssert.AreEquivalent(Collection, Uniter);
                    }
                }
            }

            public class AndReplacedInCollection : AndAddingCollection
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    Collection[0] = 10;
                }

                public class ThenReplacedInUniter : AndReplacedInCollection
                {
                    [Test]
                    public void Test()
                    {
                        CollectionAssert.AreEquivalent(Collection, Uniter);
                    }

                    [Test]
                    public void AndOrdered()
                    {
                        CollectionAssert.IsOrdered(Uniter);
                    }
                }
            }

            public class AndResetedCollection : AndAddingCollection
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    Collection.Clear();
                }

                public class ThenUniterReseted : AndResetedCollection
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(Uniter, Is.Empty);
                    }
                }
            }

            public class AndRemovedCollection : AndAddingCollection
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    Uniter.Collections.Remove(Collection);
                }

                public class ThenRemovedFromUniter : AndRemovedCollection
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(Uniter, Is.Empty);
                    }
                }

                public class AndCollectionChanged : AndRemovedCollection
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        Collection.Add(100);
                        Collection.RemoveAt(0);
                        Collection[0] = 900;
                    }

                    public class ThenUniterRemainsUnchanged : AndCollectionChanged
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Uniter, Is.Empty);
                        }
                    }
                }
            }

            public class AndReplacedCollection : AndAddingCollection
            {
                protected ObservableCollection<int> NewCollection;  

                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    NewCollection = new ObservableCollection<int>(new []
                        {
                            1, 2, 3, 4, 5
                        });
                    Uniter.Collections[0] = NewCollection;
                }

                public class ThenUniterUpdatesToNewCollection : AndReplacedCollection
                {
                    [Test]
                    public void Test()
                    {
                        CollectionAssert.AreEquivalent(NewCollection, Uniter);
                    }
                }
            }

            public class AndResetedCollections : AndAddingCollection
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    Uniter.Collections.Clear();
                }

                public class ThenCollectionRemovedFromUniter : AndResetedCollections
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(Uniter, Is.Empty);
                    }
                }

                public class AndCollectionChanged : AndResetedCollections
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        Collection.Add(100);
                        Collection.RemoveAt(0);
                        Collection[0] = 900;
                    }

                    public class ThenUniterRemainsUnchanged : AndCollectionChanged
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Uniter, Is.Empty);
                        }
                    }
                }
            }

            public class AndAddingAnotherCollection : AndAddingCollection
            {
                protected ObservableCollection<int> AnotherCollection;

                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    AnotherCollection = new ObservableCollection<int>(new []
                        {
                            1000, 2000, 3000
                        });
                    Uniter.Collections.Add(AnotherCollection);
                }

                public class ThenItems1 : AndAddingAnotherCollection
                {
                    [Test]
                    public void Inserted()
                    {
                        Assert.That(AnotherCollection, Is.SubsetOf(Uniter));
                    }

                    [Test]
                    public void Ordered()
                    {
                        CollectionAssert.IsOrdered(Uniter);
                    }
                }

                public class AndAddedToCollection1 : AndAddingAnotherCollection
                {
                    protected int AddedValue = 1;

                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        Collection.Add(AddedValue);
                    }

                    public class ThenAddedToUniter : AndAddedToCollection1
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.Contains(AddedValue, Uniter);
                        }

                        [Test]
                        public void InOrder()
                        {
                            CollectionAssert.IsOrdered(Uniter);
                        }
                    }
                }

                public class AndRemovedFromCollection1 : AndAddingAnotherCollection
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        Collection.RemoveAt(Collection.Count - 1);
                    }

                    public class ThenRemovedFromUniter : AndRemovedFromCollection1
                    {
                        [Test]
                        public void Test()
                        {
                            CollectionAssert.AreEquivalent(Collection.Concat(AnotherCollection), Uniter);
                        }
                    }
                }

                public class AndReplacedInCollection1 : AndAddingAnotherCollection
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        Collection[0] = 10;
                    }

                    public class ThenReplacedInUniter : AndReplacedInCollection1
                    {
                        [Test]
                        public void Test()
                        {
                            CollectionAssert.AreEquivalent(Collection.Concat(AnotherCollection), Uniter);
                        }

                        [Test]
                        public void AndOrdered()
                        {
                            CollectionAssert.IsOrdered(Uniter);
                        }
                    }
                }

                public class AndResetedCollection1 : AndAddingAnotherCollection
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        Collection.Clear();
                    }

                    public class ThenUniterReseted : AndResetedCollection1
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Uniter, Is.EquivalentTo(AnotherCollection));
                        }
                    }
                }

                public class AndRemovedCollection1 : AndAddingAnotherCollection
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        Uniter.Collections.Remove(Collection);
                    }

                    public class ThenRemovedFromUniter : AndRemovedCollection1
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Uniter, Is.EquivalentTo(AnotherCollection));
                        }
                    }

                    public class AndCollectionChanged : AndRemovedCollection1
                    {
                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            Collection.Add(100);
                            Collection.RemoveAt(0);
                            Collection[0] = 900;
                        }

                        public class ThenUniterRemainsUnchanged : AndCollectionChanged
                        {
                            [Test]
                            public void Test()
                            {
                                Assert.That(Uniter, Is.EqualTo(AnotherCollection));
                            }
                        }
                    }
                }

                public class AndReplacedCollection1 : AndAddingAnotherCollection
                {
                    protected ObservableCollection<int> NewCollection;

                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        NewCollection = new ObservableCollection<int>(new[]
                        {
                            1, 2, 3, 4, 5
                        });
                        Uniter.Collections[0] = NewCollection;
                    }

                    public class ThenUniterUpdatesToNewCollection : AndReplacedCollection1
                    {
                        [Test]
                        public void Test()
                        {
                            CollectionAssert.AreEquivalent(NewCollection.Concat(AnotherCollection), Uniter);
                        }
                    }
                }

                public class AndResetedCollections1 : AndAddingAnotherCollection
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        Uniter.Collections.Clear();
                    }

                    public class ThenCollectionRemovedFromUniter : AndResetedCollections1
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Uniter, Is.Empty);
                        }
                    }

                    public class AndCollectionChanged : AndResetedCollections1
                    {
                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            Collection.Add(100);
                            Collection.RemoveAt(0);
                            Collection[0] = 900;
                        }

                        public class ThenUniterRemainsUnchanged : AndCollectionChanged
                        {
                            [Test]
                            public void Test()
                            {
                                Assert.That(Uniter, Is.Empty);
                            }
                        }
                    }
                }
            }
        }
    }
}
