using System;
using System.ComponentModel;
using System.Threading;
using NUnit.Framework;
using PHmiClient.Utils;

namespace PHmiIntegrationTests.Client.Utils
{
    [TestFixture]
    public class EventHelperLoopTests
    {
        private bool _stop;

        [SetUp]
        public void SetUp()
        {
            _stop = false;
        }

        private void LoopTest(Func<bool> raiseFunc, Action loop)
        {
            var started = false;
            var loopAction = new Action(
                () =>
                {
                    started = true;
                    loop.Invoke();
                });
            loopAction.BeginInvoke(loopAction.EndInvoke, null);

            while (!started)
            {
                Thread.Sleep(0);
            }

            TestDelegate testAction =
                () =>
                {
                    for (var i = 0; i < 100000; i++)
                    {
                        raiseFunc.Invoke();
                        i++;
                    }
                    _stop = true;
                };
            Assert.DoesNotThrow(testAction);
        }

        #region Raise

        private event EventHandler Event;

        private void Loop()
        {
            EventHandler handler = (sender, args) => { };
            while (!_stop)
            {
                Event += handler;
                Event -= handler;
            }
        }

        [Test]
        public void RaiseLoopTest()
        {
            LoopTest(() => EventHelper.Raise(ref Event, this, EventArgs.Empty), Loop);
        }

        #endregion

        #region RaiseGeneric

        private event EventHandler<EventArgs> EventGeneric;

        private void LoopGeneric()
        {
            EventHandler<EventArgs> handler = (sender, args) => { };
            while (!_stop)
            {
                EventGeneric += handler;
                EventGeneric -= handler;
            }
        }

        [Test]
        public void RaiseLoopGenericTest()
        {
            LoopTest(() => EventHelper.Raise(ref EventGeneric, this, EventArgs.Empty), LoopGeneric);
        }

        #endregion

        #region RaisePropertyChanged

        private event PropertyChangedEventHandler EventPropertyChanged;

        private void LoopPropertyChanged()
        {
            PropertyChangedEventHandler handler = (sender, args) => { };
            while (!_stop)
            {
                EventPropertyChanged += handler;
                EventPropertyChanged -= handler;
            }
        }

        [Test]
        public void RaiseLoopPropertyChangedTest()
        {
            LoopTest(() => EventHelper.Raise(
                ref EventPropertyChanged, this, new PropertyChangedEventArgs(null)),
                LoopPropertyChanged);
        }

        #endregion
    }
}
