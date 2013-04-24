using NUnit.Framework;
using PHmiClient.Utils.ViewInterfaces;
using PHmiTools.ViewModels;

namespace PHmiUnitTests.Tools.ViewModels
{
    [TestFixture]
    public class ViewModelBaseTests
    {
        private class ViewModel : ViewModelBase<IWindow>
        {
            public string Property { get; set; }

            public void TestPropertyChanged()
            {
                OnPropertyChanged(this, v => v.Property);
            }
        }

        [Test]
        public void OnPropertyChangedRaisesEvent()
        {
            var raised = false;
            var viewModel = new ViewModel();
            viewModel.PropertyChanged += (sender, args) => { raised = true; };
            viewModel.TestPropertyChanged();
            Assert.IsTrue(raised);
        }

        [Test]
        public void OnPropertyChangedPassesSenderAndArgsPropertyName()
        {
            const string property = "Property";
            var viewModel = new ViewModel();
            viewModel.PropertyChanged +=
                (sender, args) =>
                    {
                        Assert.AreSame(viewModel, sender);
                        Assert.AreEqual(property, args.PropertyName);
                    };
            viewModel.TestPropertyChanged();
        }
    }
}
