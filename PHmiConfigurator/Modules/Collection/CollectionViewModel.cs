using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using PHmiClient.Controls.Input;
using PHmiClient.Utils;
using PHmiConfigurator.Dialogs;
using PHmiResources.Loc;
using PHmiModel.Interfaces;

namespace PHmiConfigurator.Modules.Collection
{
    public abstract class CollectionViewModel<T, TMeta> : ModuleViewModel, ICollectionViewModel
        where T : class, IDataErrorInfo, INotifyPropertyChanged, INamedEntity, new() 
        where TMeta : class, IDataErrorInfo, new()
    {
        protected CollectionViewModel(ICollectionService service) : base(service)
        {
            _service = service ?? new CollectionService();
            _readOnlyCollection = new ReadOnlyObservableCollection<T>(List);
            _addCommand = new DelegateCommand(AddCommandExecuted, AddCommandCanExecute);
            _editCommand = new DelegateCommand(EditCommandExecuted, EditCommandCanExecute);
            _deleteCommand = new DelegateCommand(DeleteCommandExecuted, DeleteCommandCanExecute);
            _copyCommand = new DelegateCommand(CopyCommandExecuted, CopyCommandCanExecute);
            _pasteCommand = new DelegateCommand(PasteCommandExecuted);
            _unselectCommand = new DelegateCommand(UnselectCommandExecuted, UnselectCommandCanExecute);
            _selectedItems.CollectionChanged += SelectedItemsCollectionChanged;
        }

        private readonly ICollectionService _service;

        public override bool IsValid
        {
            get { return base.IsValid && List.All(d => string.IsNullOrEmpty(d.Error)); }
        }

        public override string Error
        {
            get
            {
                var names = List.GroupBy(i => i.name).Where(g => g.Count() > 1).Select(g => "\"" + g.Key + "\"").ToArray();
                var error = string.Empty;
                if (names.Any())
                {
                    error =
                        string.Format(Res.UniqueErrorMessage, ReflectionHelper.GetDisplayName<T>(t => t.name))
                        + Environment.NewLine
                        + string.Join(", ", names) + ".";
                }
                return error;
            }
        }

        protected readonly ObservableCollection<T> List = new ObservableCollection<T>();
        private readonly ReadOnlyObservableCollection<T> _readOnlyCollection;
        public ReadOnlyObservableCollection<T> Collection { get { return _readOnlyCollection; } }
        
        protected override void PostReloadAction()
        {
            List.Clear();
            var result = Context.Get<T>().OrderBy(i => i.id).ToArray();
            foreach (var i in result)
            {
                List.Add(i);
            }
        }

        #region SelectedItem

        private T _selectedItem;

        public T SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                OnPropertyChanged(this, v => v.SelectedItem);
                _editCommand.RaiseCanExecuteChanged();
                _unselectCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region SelectedItems

        private readonly ObservableCollection<T> _selectedItems = new ObservableCollection<T>();

        public ObservableCollection<T> SelectedItems
        {
            get { return _selectedItems; }
        }

        private void SelectedItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _deleteCommand.RaiseCanExecuteChanged();
            _copyCommand.RaiseCanExecuteChanged();
        }

        #endregion

        protected abstract IEditDialog<TMeta> CreateAddDialog();
        protected abstract IEditDialog<TMeta> CreateEditDialog();

        #region AddCommand

        private readonly DelegateCommand _addCommand;

        protected void RaiseAddCommandCanExecuteChanged()
        {
            _addCommand.RaiseCanExecuteChanged();
        }

        public ICommand AddCommand
        {
            get { return _addCommand; }
        }

        protected virtual bool AddCommandCanExecute(object obj)
        {
            return true;
        }

        private void AddCommandExecuted(object obj)
        {
            var dialog = CreateAddDialog();
            var entity = new T();
            var meta = _service.EditorHelper.Clone(entity);
            dialog.Entity = (TMeta) meta;
            if (dialog.ShowDialog() == true)
            {
                _service.EditorHelper.Update(meta, entity);
                List.Add(entity);
                OnBeforeAddedToContext(entity);
                Context.AddTo(entity);
                SelectedItem = entity;
            }
        }

        protected virtual void OnBeforeAddedToContext(T entity)
        {
        }

        #endregion

        #region EditCommand

        private readonly DelegateCommand _editCommand;

        public ICommand EditCommand
        {
            get { return _editCommand; }
        }

        private bool EditCommandCanExecute(object obj)
        {
            return SelectedItem != null;
        }

        private void EditCommandExecuted(object obj)
        {
            var dialog = CreateEditDialog();
            var meta = (TMeta) _service.EditorHelper.Clone(SelectedItem);
            dialog.Entity = meta;
            if (dialog.ShowDialog() == true)
            {
                _service.EditorHelper.Update(meta, SelectedItem);
            }
        }

        #endregion

        #region DeleteCommand

        private readonly DelegateCommand _deleteCommand;

        public ICommand DeleteCommand
        {
            get { return _deleteCommand; }
        }

        private bool DeleteCommandCanExecute(object obj)
        {
            return SelectedItems.Any();
        }

        private void DeleteCommandExecuted(object obj)
        {
            if (_service.DialogHelper.Message(Res.DeleteRowsQuestion, Name, MessageBoxButton.YesNo, MessageBoxImage.Question, View) != true)
                return;
            var entitiesToDelete = SelectedItems.ToArray();
            Action action = () =>
                {
                    var length = entitiesToDelete.Length;
                    _service.ActionHelper.Dispatch(() =>
                        {
                            InProgress = true;
                            ProgressIsIndeterminate = false;
                            ProgressMax = length;
                            Progress = 0;
                        });
                    for (var i = 0; i < length; i++)
                    {
                        var entity = entitiesToDelete[i];
                        var progress = i + 1;
                        _service.ActionHelper.Dispatch(() =>
                            {
                                List.Remove(entity);
                                Context.DeleteObject(entity);
                                Progress = progress;
                            });
                    }
                    _service.ActionHelper.Dispatch(() =>
                        {
                            InProgress = false;
                        });
                };
            _service.ActionHelper.Async(action);
        }

        #endregion

        #region CopyCommand

        private readonly DelegateCommand _copyCommand;

        public ICommand CopyCommand
        {
            get { return _copyCommand; }
        }

        private bool CopyCommandCanExecute(object obj)
        {
            return SelectedItems.Any();
        }

        private void CopyCommandExecuted(object obj)
        {
            var selectedItems = SelectedItems.ToArray();
            var header = string.Join("\t",
                ReflectionHelper.GetDisplayName<T>(i => i.id),
                ReflectionHelper.GetDisplayName<T>(i => i.name),
                string.Join("\t", GetCopyHeaders()));
            var text = selectedItems.Select(i => string.Join("\t", i.id + "\t" + i.name, string.Join("\t", GetCopyData(i)))).ToArray();
            _service.ClipboardHelper.SetText(string.Join("\r\n", header, string.Join("\r\n", text)));
        }

        protected abstract string[] GetCopyData(T item);

        protected abstract string[] GetCopyHeaders();

        #endregion

        #region PasteCommand

        private readonly ICommand _pasteCommand;

        public ICommand PasteCommand
        {
            get { return _pasteCommand; }
        }

        private void PasteCommandExecuted(object obj)
        {
            InProgress = true;
            ProgressIsIndeterminate = true;
            var text = _service.ClipboardHelper.GetText();
            Action action = () =>
                {
                    try
                    {
                        var rows = text.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
                        _service.ActionHelper.Dispatch(() =>
                            {
                                ProgressIsIndeterminate = false;
                                ProgressMax = rows.Length;
                                Progress = 0;
                            });
                        var items = List.ToDictionary(i => i.id);
                        for (var index = 0; index < rows.Length; index++)
                        {
                            var row = rows[index];
                            var columns = row.Split(new[] {"\t"}, StringSplitOptions.None);
                            if (columns.Length != GetCopyHeaders().Length + 2)
                            {
                                throw new Exception(Res.ColumnsCountNotMatchMessage);
                            }
                            int id;
                            T item;
                            var isNewItem = false;
                            if (int.TryParse(columns[0], out id))
                            {
                                if (!items.TryGetValue(id, out item))
                                {
                                    throw new Exception(string.Format(Res.ItemWithIdNotFoundMessage, id));
                                }
                            }
                            else if (string.IsNullOrEmpty(columns[0]))
                            {
                                item = new T();
                                isNewItem = true;
                            }
                            else if (columns[0] == ReflectionHelper.GetDisplayName<T>(i => i.id))
                            {
                                continue;
                            }
                            else
                            {
                                throw new Exception(string.Format(Res.NotValidIdMessage, columns[0]));
                            }
                            var progress = index + 1;
                            Exception toThrow = null;
                            _service.ActionHelper.Dispatch(() =>
                                {
                                    Progress = progress;
                                    try
                                    {
                                        item.name = columns[1];
                                        SetCopyData(item, columns.Skip(2).ToArray());
                                        if (isNewItem)
                                        {
                                            OnBeforeAddedToContext(item);
                                            Context.AddTo(item);
                                            List.Add(item);
                                        }
                                    }
                                    catch (Exception exception)
                                    {
                                        toThrow = exception;
                                    }
                                });
                            if (toThrow != null)
                            {
                                throw new Exception(
                                    String.Format(
                                    Res.PasteRowErrorMessage,
                                    ReflectionHelper.GetDisplayName<T>(t => t.id),
                                    columns[0],
                                    ReflectionHelper.GetDisplayName<T>(t => t.name),
                                    columns[1],
                                    toThrow.Message),
                                    toThrow);
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        _service.DialogHelper.Exception(exception, View);
                    }
                    finally
                    {
                        _service.ActionHelper.Dispatch(() =>
                            {
                                InProgress = false;
                                ProgressIsIndeterminate = false;
                            });
                    }
                };
            _service.ActionHelper.Async(action);
        }

        protected abstract void SetCopyData(T item, string[] data);

        #endregion

        #region UnselectCommand

        private readonly DelegateCommand _unselectCommand;

        public ICommand UnselectCommand { get { return _unselectCommand; } }
        
        private bool UnselectCommandCanExecute(object obj)
        {
            return SelectedItem != null;
        }

        private void UnselectCommandExecuted(object obj)
        {
            SelectedItem = null;
        }

        #endregion
    }
}
