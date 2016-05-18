using Moq;
using NUnit.Framework;
using PHmiConfigurator.Dialogs;
using PHmiConfigurator.Modules.Collection;
using PHmiModel.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace PHmiUnitTests.Configurator.Modules.Collection
{
    [TestFixture]
    public class CollectionViewModelTests
    {
        #region Stubs

        public class Meta : IDataErrorInfo
        {
            public bool Property { get; set; }

            public string this[string columnName]
            {
                get { throw new NotImplementedException(); }
            }

            public string Error { get; private set; }
        }

        public class DataErrorInfo : IDataErrorInfo, INamedEntity, INotifyPropertyChanged
        {
            public bool HasError;

            public string this[string columnName]
            {
                get { throw new NotImplementedException(); }
            }

            public string Error
            {
                get { return HasError ? "Error" : string.Empty; } 
            }

            public event PropertyChangedEventHandler PropertyChanged;

            public void OnPropertyChanged(string propertyName)
            {
                var handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            }

            public bool Property { get; set; }
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class Service : ModuleViewModelTests.Service, ICollectionService
        {
        }

        private class ViewModel : CollectionViewModel<DataErrorInfo, Meta>
        {
            public ViewModel(Service service) : base(service) { }

            public override string Name
            {
                get { return "ViewModel"; }
            }

            public readonly List<DataErrorInfo> PostReloadItemsToInsert = new List<DataErrorInfo>();

            protected override void PostReloadAction()
            {
                base.PostReloadAction();
                foreach (var dataErrorInfo in PostReloadItemsToInsert)
                {
                    List.Add(dataErrorInfo);
                }
            }

            public void SetDataErrorInfo(DataErrorInfo info)
            {
                List.Add(info);
            }

            public IEditDialog<Meta> EditDialog;
            public IEditDialog<Meta> AddDialog; 

            protected override IEditDialog<Meta> CreateAddDialog()
            {
                return AddDialog;
            }

            protected override IEditDialog<Meta> CreateEditDialog()
            {
                return EditDialog;
            }

            public ObservableCollection<DataErrorInfo> ItemsList
            {
                get { return List; }
            }

            public DataErrorInfo BeforeAddedToContextEntity;

            protected override void OnBeforeAddedToContext(DataErrorInfo entity)
            {
                BeforeAddedToContextEntity = entity;
                base.OnBeforeAddedToContext(entity);
            }

            protected override string[] GetCopyData(DataErrorInfo item)
            {
                return new []{item.Property.ToString(CultureInfo.InvariantCulture)};
            }

            protected override string[] GetCopyHeaders()
            {
                return new []{ "Property" };
            }

            protected override void SetCopyData(DataErrorInfo item, string[] data)
            {
                item.Property = data[0] == "True";
            }
        }

        #endregion

        private Service _service;
        private Mock<IModelContext> _contextStub;
        private Mock<IEditDialog<Meta>> _editDialogStub;
        private ViewModel _viewModel;

        [SetUp]
        public void SetUp()
        {
            _contextStub = new Mock<IModelContext>();
            _service = new Service();
            _service.ContextFactoryStub.Setup(f => f.Create(It.IsAny<string>(), true)).Returns(_contextStub.Object);
            _viewModel = new ViewModel(_service);
            _editDialogStub = new Mock<IEditDialog<Meta>>();
            _viewModel.EditDialog = _editDialogStub.Object;
            _viewModel.AddDialog = _editDialogStub.Object;
        }

        #region IsValid

        [Test]
        public void IsValidReturnsTrueIfCollectionIsEmpty()
        {
            Assert.IsTrue(_viewModel.IsValid);
        }

        [Test]
        public void IsValidReturnsTrueIfAllItemsAreValid()
        {
            var one = new DataErrorInfo { HasError = false, Name = "one" };
            var two = new DataErrorInfo { HasError = false, Name = "two" };
            _viewModel.SetDataErrorInfo(one);
            _viewModel.SetDataErrorInfo(two);
            Assert.IsTrue(_viewModel.IsValid);
        }

        [Test]
        public void IsValidReturnsFalseIfAnyItemIsInvalid()
        {
            var one = new DataErrorInfo { HasError = false, Name = "one" };
            var two = new DataErrorInfo { HasError = true, Name = "two" };
            _viewModel.SetDataErrorInfo(one);
            _viewModel.SetDataErrorInfo(two);
            Assert.IsFalse(_viewModel.IsValid);
        }

        [Test]
        public void IsValidReturnsFalseIfDublicateItemsNames()
        {
            var one = new DataErrorInfo { HasError = false, Name = "name" };
            var two = new DataErrorInfo { HasError = false, Name = "name" };
            _viewModel.SetDataErrorInfo(one);
            _viewModel.SetDataErrorInfo(two);
            Assert.IsFalse(_viewModel.IsValid);
        }

        #endregion

        #region SelectedItem

        [Test, TestCaseSource("DataErrorInfoTestCases")]
        public void SelectedItemSetRaisesOnPropertyChanged(DataErrorInfo value)
        {
            var raised = false;
            _viewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == "SelectedItem")
                        raised = true;
                };
            _viewModel.SelectedItem = value;
            Assert.IsTrue(raised);
        }

        #endregion

        #region Reload

        [Test]
        public void ReloadOrdersById()
        {
            var one = new DataErrorInfo { Id = 2 };
            var two = new DataErrorInfo { Id = 1 };
            var infos = new[] { one, two }.AsQueryable();
            _contextStub.Setup(c => c.Get<DataErrorInfo>()).Returns(infos);
            _viewModel.Reload();
            Assert.AreEqual(2, _viewModel.Collection.Count);
            Assert.AreEqual(two, _viewModel.Collection.First());
            Assert.AreEqual(one, _viewModel.Collection.Last());
        }

        [Test]
        public void ReloadClearsIoDevices()
        {
            var one = new DataErrorInfo { Id = 1 };
            var two = new DataErrorInfo { Id = 2 };
            var ioDevices = new[] { one, two }.AsQueryable();
            _contextStub.Setup(c => c.Get<DataErrorInfo>()).Returns(ioDevices);
            _viewModel.Reload();
            _contextStub.Setup(c => c.Get<DataErrorInfo>()).Returns(new DataErrorInfo[0].AsQueryable());
            _viewModel.Reload();
            Assert.IsEmpty(_viewModel.Collection);
        }

        #endregion

        private readonly static object[] DataErrorInfoTestCases = new object[] {new DataErrorInfo(), null};
        private readonly static object[] BooleanTestCases = new object[] {true, false};

        #region AddCommand

        [Test]
        public void AddCommandCanExecuteReturnsTrue()
        {
            Assert.IsTrue(_viewModel.AddCommand.CanExecute(null));
        }

        [Test, TestCaseSource("BooleanTestCases")]
        public void AddCommandTest(bool dialogReturnValue)
        {
            _editDialogStub.Setup(d => d.ShowDialog()).Returns(dialogReturnValue);
            _viewModel.Reload();
            _viewModel.AddCommand.Execute(null);
            _editDialogStub.Verify();
            _contextStub.Verify(c => c.AddTo(It.IsAny<DataErrorInfo>()), dialogReturnValue ? Times.Once() : Times.Never());
            if (dialogReturnValue)
            {
                Assert.IsNotEmpty(_viewModel.Collection);
            }
            else
            {
                Assert.IsEmpty(_viewModel.Collection);
            }
        }

        [Test, TestCaseSource("BooleanTestCases")]
        public void AddCommandSetSelectedItemToNewItem(bool dialogReturnValue)
        {
            _editDialogStub.Setup(d => d.ShowDialog()).Returns(dialogReturnValue);
            var count = _viewModel.Collection.Count;
            _viewModel.Reload();
            _viewModel.AddCommand.Execute(null);
            _editDialogStub.Verify();
            if (dialogReturnValue)
            {
                Assert.AreEqual(_viewModel.Collection.Last(), _viewModel.BeforeAddedToContextEntity);
                Assert.AreEqual(count + 1, _viewModel.Collection.Count);
                Assert.AreEqual(_viewModel.Collection.Last(), _viewModel.SelectedItem);
            }
            else
            {
                Assert.IsNull(_viewModel.BeforeAddedToContextEntity);
                Assert.AreEqual(count, _viewModel.Collection.Count);
            }
        }

        #endregion

        #region EditCommand

        [Test, TestCaseSource("DataErrorInfoTestCases")]
        public void EditCommandCanExecuteReturnsSelectedItemIsNotNull(DataErrorInfo selectedItem)
        {
            _viewModel.SelectedItem = selectedItem;
            Assert.AreEqual(selectedItem != null, _viewModel.EditCommand.CanExecute(null));
        }

        [Test, TestCaseSource("DataErrorInfoTestCases")]
        public void SetSelectedItemRaisesEditCommandCanExecuteChanged(DataErrorInfo value)
        {
            var raised = false;
            _viewModel.EditCommand.CanExecuteChanged += (sender, args) =>
                {
                    raised = true;
                };
            _viewModel.SelectedItem = value;
            Assert.IsTrue(raised);
        }

        [Test, TestCaseSource("BooleanTestCases")]
        public void EditCommandExecutedTest(bool dialogReturnValue)
        {
            var clone = new Meta();
            _editDialogStub.Setup(d => d.ShowDialog()).Returns(dialogReturnValue).Callback(() =>
                {
                    clone.Property = true;
                });
            var info = new DataErrorInfo();
            _service.EditorHelperStub.Setup(h => h.Update(clone, info)).Callback(() =>
                {
                    info.Property = clone.Property;
                });
            _viewModel.PostReloadItemsToInsert.Add(info);
            _viewModel.Reload();
            _viewModel.SelectedItem = info;
            _service.EditorHelperStub.Setup(h => h.Clone(info)).Returns(clone);
            _viewModel.EditCommand.Execute(null);
            _editDialogStub.Verify();
            if (dialogReturnValue)
            {
                Assert.IsTrue(info.Property);
                _service.EditorHelperStub.Verify(h => h.Update(clone, info), Times.Once());
            }
            else
            {
                Assert.IsFalse(info.Property);
                _service.EditorHelperStub.Verify(h => h.Update(clone, info), Times.Never());
            }
        }

        #endregion

        #region DeleteCommand

        [Test, TestCaseSource("BooleanTestCases")]
        public void DeleteCommandCanExecuteReturnsSelectedItemsNotEmpty(bool empty)
        {
            if (!empty)
                _viewModel.SelectedItems.Add(new DataErrorInfo());
            Assert.AreEqual(!empty, _viewModel.DeleteCommand.CanExecute(null));
        }

        [Test, TestCaseSource("DataErrorInfoTestCases")]
        public void ChangeSelectedItemsRaisesDeleteCommandCanExecuteChanged(DataErrorInfo value)
        {
            if (value == null)
                _viewModel.SelectedItems.Add(new DataErrorInfo());
            var raised = false;
            _viewModel.DeleteCommand.CanExecuteChanged += (sender, args) =>
            {
                raised = true;
            };
            if (value == null)
                _viewModel.SelectedItems.Clear();
            else
                _viewModel.SelectedItems.Add(value);
            Assert.IsTrue(raised);
        }

        [Test, TestCaseSource("BooleanTestCases")]
        public void DeleteCommandExecutedTest(bool dialogReturnValue)
        {
            _service.DialogHelperStub
                .Setup(h => h.Message(
                    It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.YesNo, MessageBoxImage.Question, It.IsAny<object>()))
                .Returns(dialogReturnValue);
            var info = new DataErrorInfo();
            var info2 = new DataErrorInfo();
            var info3 = new DataErrorInfo();
            _viewModel.PostReloadItemsToInsert.Add(info);
            _viewModel.PostReloadItemsToInsert.Add(info2);
            _viewModel.PostReloadItemsToInsert.Add(info3);
            _viewModel.Reload();
            _viewModel.SelectedItems.Add(info);
            _viewModel.SelectedItems.Add(info3);
            _viewModel.DeleteCommand.Execute(null);

            if (dialogReturnValue)
            {
                Assert.AreEqual(1, _viewModel.Collection.Count);
                Assert.AreEqual(info2, _viewModel.Collection[0]);
                _contextStub.Verify(c => c.DeleteObject(info), Times.Once());
                _contextStub.Verify(c => c.DeleteObject(info3), Times.Once());
            }
            else
            {
                Assert.AreEqual(3, _viewModel.Collection.Count);
                Assert.AreEqual(info, _viewModel.Collection[0]);
                Assert.AreEqual(info2, _viewModel.Collection[1]);
                Assert.AreEqual(info3, _viewModel.Collection[2]);
                _contextStub.Verify(c => c.DeleteObject(It.IsAny<object>()), Times.Never());
            }
        }

        [Test]
        public void DeleteCommandExecutedCollectionEffectTest()
        {
            _service.DialogHelperStub
                .Setup(h => h.Message(
                    It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.YesNo, MessageBoxImage.Question, It.IsAny<object>()))
                .Returns(true);
            var info = new DataErrorInfo();
            var info2 = new DataErrorInfo();
            var info3 = new DataErrorInfo();
            _viewModel.PostReloadItemsToInsert.Add(info);
            _viewModel.PostReloadItemsToInsert.Add(info2);
            _viewModel.PostReloadItemsToInsert.Add(info3);
            _viewModel.Reload();
            _viewModel.SelectedItems.Add(info);
            _viewModel.SelectedItems.Add(info2);
            _viewModel.SelectedItems.Add(info3);
            _viewModel.ItemsList.CollectionChanged += (sender, args) => _viewModel.SelectedItems.Remove((DataErrorInfo)args.OldItems[0]);
            TestDelegate del = () => _viewModel.DeleteCommand.Execute(null);
            Assert.DoesNotThrow(del);
        }

        #endregion

        #region CopyCommand

        [Test, TestCaseSource("BooleanTestCases")]
        public void CopyCommandCanExecuteReturnsSelectedItemsNotEmpty(bool empty)
        {
            if (!empty)
                _viewModel.SelectedItems.Add(new DataErrorInfo());
            Assert.AreEqual(!empty, _viewModel.CopyCommand.CanExecute(null));
        }

        [Test, TestCaseSource("DataErrorInfoTestCases")]
        public void ChangeSelectedItemsRaisesCopyCommandCanExecuteChanged(DataErrorInfo value)
        {
            if (value == null)
                _viewModel.SelectedItems.Add(new DataErrorInfo());
            var raised = false;
            _viewModel.CopyCommand.CanExecuteChanged += (sender, args) =>
            {
                raised = true;
            };
            if (value == null)
                _viewModel.SelectedItems.Clear();
            else
                _viewModel.SelectedItems.Add(value);
            Assert.IsTrue(raised);
        }

        [Test]
        public void CopyCommandExecutedTest()
        {
            var item1 = new DataErrorInfo {Id = 1, Name = "item1", Property = true};
            var item2 = new DataErrorInfo {Id = 2, Name = "item2", Property = false};
            _viewModel.SelectedItems.Add(item1);
            _viewModel.SelectedItems.Add(item2);
            _viewModel.CopyCommand.Execute(null);
            _service.ClipboardHelperStub.Verify(h => h.SetText("Id\tName\tProperty\r\n1\titem1\tTrue\r\n2\titem2\tFalse"));
        }

        #endregion

        #region PasteCommand

        [Test]
        public void PasteCommandExecutedTest()
        {
            _service.ClipboardHelperStub.Setup(h => h.GetText()).Returns("2\titem22\tTrue\r\n\titem3\tTrue\r\n4\titem3\tTrue");
            var item1 = new DataErrorInfo { Id = 1, Name = "item1", Property = true };
            var item2 = new DataErrorInfo { Id = 2, Name = "item2", Property = false };
            _viewModel.PostReloadItemsToInsert.Add(item1);
            _viewModel.PostReloadItemsToInsert.Add(item2);
            _viewModel.Reload();
            _viewModel.PasteCommand.Execute(null);
            _service.ClipboardHelperStub.Verify();
            Assert.AreEqual(3, _viewModel.Collection.Count);
            Assert.Contains(item1, _viewModel.Collection);
            Assert.AreEqual(1, item1.Id);
            Assert.AreEqual("item1", item1.Name);
            Assert.AreEqual(true, item1.Property);
            Assert.Contains(item2, _viewModel.Collection);
            Assert.AreEqual(2, item2.Id);
            Assert.AreEqual("item22", item2.Name);
            Assert.AreEqual(true, item2.Property);
            var added = _viewModel.Collection.Last();
            Assert.AreEqual(0, added.Id);
            Assert.AreEqual("item3", added.Name);
            Assert.AreEqual(true, added.Property);
        }

        #endregion

        #region UnselectCommand

        [Test, TestCaseSource("DataErrorInfoTestCases")]
        public void UnselectCommandCanExecuteReturnsSelectedItemIsNotNull(DataErrorInfo selectedItem)
        {
            _viewModel.SelectedItem = selectedItem;
            Assert.AreEqual(selectedItem != null, _viewModel.UnselectCommand.CanExecute(null));
        }

        [Test, TestCaseSource("DataErrorInfoTestCases")]
        public void SetSelectedItemRaisesUnselectCommandCanExecuteChanged(DataErrorInfo value)
        {
            var raised = false;
            _viewModel.UnselectCommand.CanExecuteChanged += (sender, args) =>
            {
                raised = true;
            };
            _viewModel.SelectedItem = value;
            Assert.IsTrue(raised);
        }

        [Test]
        public void UnselectCommandSetsSelectedItemToNull()
        {
            _viewModel.SelectedItem = new DataErrorInfo();
            _viewModel.UnselectCommand.Execute(null);
            Assert.IsNull(_viewModel.SelectedItem);
        }

        #endregion
    }
}
