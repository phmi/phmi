using System;
using NUnit.Framework;
using PHmiClient.Controls.Input;

namespace PHmiClientUnitTests.Client.Controls.Input
{
    [TestFixture]
    class DelegateCommandTests
    {
        [Test]
        public void ExecuteInvokesAction()
        {
            var invoked = false;
            Action<object> action =
                o =>
                    {
                        invoked = true;
                    };
            var command = new DelegateCommand(action);
            command.Execute(null);
            Assert.IsTrue(invoked);
        }

        [Test]
        public void ExecutePassesParameterToAction()
        {
            var parameter = new object();
            object passedParameter = null;
            Action<object> action =
                o =>
                    {
                        passedParameter = o;
                    };
            var command = new DelegateCommand(action);
            command.Execute(parameter);
            Assert.AreSame(parameter, passedParameter);
        }

        [Test]
        public void ExecuteThrowsIfActionIsNull()
        {
            var command = new DelegateCommand(null);
            Assert.Catch<NullReferenceException>(() => command.Execute(null));
        }

        [Test]
        public void CanExecuteReturnsTrueIfPredicateIsNotPassed()
        {
            var command = new DelegateCommand(null);
            Assert.IsTrue(command.CanExecute(null));
        }

        [Test]
        public void CanExecuteReturnsTrueIfPredicateReturnsTrue()
        {
            var command = new DelegateCommand(null, o => true);
            Assert.IsTrue(command.CanExecute(null));
        }

        [Test]
        public void CanExecuteReturnsFalseIfPredicateReturnsFalse()
        {
            var command = new DelegateCommand(null, o => false);
            Assert.IsFalse(command.CanExecute(null));
        }

        [Test]
        public void CanExecutePassesParameterToPredicate()
        {
            object passedParameter = null;
            Predicate<object> predicate = 
                o =>
                    {
                        passedParameter = o;
                        return false;
                    };
            var command = new DelegateCommand(null, predicate);
            var parameter = new object();
            command.CanExecute(parameter);
            Assert.AreSame(parameter, passedParameter);
        }

        [Test]
        public void RaiseCanExecuteChangedTest()
        {
            var command = new DelegateCommand(null);
            var raised = false;
            command.CanExecuteChanged += (sender, args) => { raised = true; };
            command.RaiseCanExecuteChanged();
            Assert.IsTrue(raised);
        }
    }
}
