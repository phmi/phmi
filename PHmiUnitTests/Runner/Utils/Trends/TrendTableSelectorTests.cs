using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using PHmiClientUnitTests;
using PHmiRunner.Utils.Trends;

namespace PHmiUnitTests.Runner.Utils.Trends
{
    public class WhenUsingTrendTableSelector : Specification
    {
        protected ITrendTableSelector Selector;

        protected override void EstablishContext()
        {
            base.EstablishContext();
            Selector = new TrendTableSelector();
        }

        public class ThenFirstIs0 : WhenUsingTrendTableSelector
        {
            [Test]
            public void Test()
            {
                Assert.That(Selector.NextTable(), Is.EqualTo(0));
            }
        }

        public class ThenSecondIs1 : WhenUsingTrendTableSelector
        {
            protected override void EstablishContext()
            {
                base.EstablishContext();
                Selector.NextTable();
            }

            [Test]
            public void Test()
            {
                Assert.That(Selector.NextTable(), Is.EqualTo(1));
            }
        }

        public class ThenThirdIs0 : WhenUsingTrendTableSelector
        {
            protected override void EstablishContext()
            {
                base.EstablishContext();
                Selector.NextTable();
                Selector.NextTable();
            }

            [Test]
            public void Test()
            {
                Assert.That(Selector.NextTable(), Is.EqualTo(0));
            }
        }

        public class ThenFourthIs2 : WhenUsingTrendTableSelector
        {
            protected override void EstablishContext()
            {
                base.EstablishContext();
                Selector.NextTable();
                Selector.NextTable();
                Selector.NextTable();
            }

            [Test]
            public void Test()
            {
                Assert.That(Selector.NextTable(), Is.EqualTo(2));
            }
        }

        public class Then512ThIs9 : WhenUsingTrendTableSelector
        {
            protected override void EstablishContext()
            {
                base.EstablishContext();
                for (var i = 1; i <= 511; i++)
                {
                    Selector.NextTable();
                }
            }

            [Test]
            public void Test()
            {
                Assert.That(Selector.NextTable(), Is.EqualTo(9));
            }
        }

        public class Then1024ThIs0 : WhenUsingTrendTableSelector
        {
            protected override void EstablishContext()
            {
                base.EstablishContext();
                for (var i = 1; i <= 1023; i++)
                {
                    Selector.NextTable();
                }
            }

            [Test]
            public void Test()
            {
                Assert.That(Selector.NextTable(), Is.EqualTo(0));
            }
        }

        public class ThenNthIsSomth : WhenUsingTrendTableSelector
        {
            protected int[] Values;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                Values = new []
                    {
                        0, 1, 0, 2, 0, 1, 0, 3, 0, 1, 0, 2, 0, 1, 0, 4
                    };
            }

            [Test]
            public void Test()
            {
                foreach (var t in Values)
                {
                    Assert.That(Selector.NextTable(), Is.EqualTo(t));
                }
            }
        }
    }
}
