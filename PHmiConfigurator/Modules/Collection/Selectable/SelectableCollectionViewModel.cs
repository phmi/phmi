using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using PHmiResources.Loc;
using PHmiModel.Interfaces;

namespace PHmiConfigurator.Modules.Collection.Selectable
{
    public abstract class SelectableCollectionViewModel<T, TMeta, TSelector> : CollectionViewModel<T, TMeta>
        where T : class, IDataErrorInfo, INotifyPropertyChanged, INamedEntity, new()
        where TMeta : class, IDataErrorInfo, new()
        where TSelector : class, INamedEntity, IRepository
    {
        protected SelectableCollectionViewModel(ISelectableCollectionService service) : base(service)
        {
            _service = service ?? new SelectableCollectionService();
            _readOnlySelectors = new ReadOnlyObservableCollection<TSelector>(SelectorsList);
        }

        private readonly ISelectableCollectionService _service;
        protected readonly ObservableCollection<TSelector> SelectorsList = new ObservableCollection<TSelector>();
        private readonly ReadOnlyObservableCollection<TSelector> _readOnlySelectors;
        private TSelector _currentSelector;

        protected override void PostReloadAction()
        {
            var oldSelector = CurrentSelector;
            var selectors = Context.Get<TSelector>().OrderBy(i => i.name).ToArray();
            SelectorsList.Clear();
            foreach (var i in selectors)
            {
                SelectorsList.Add(i);
            }
            CurrentSelector = oldSelector == null ? null : selectors.FirstOrDefault(i => i.id == oldSelector.id);
            if (CurrentSelector == null && oldSelector == null && selectors.Count() == 1)
                CurrentSelector = selectors.FirstOrDefault();
            LoadCollection();
        }
        
        private void LoadCollection()
        {
            List.Clear();
            var selector = CurrentSelector;
            if (selector == null)
                return;
            foreach (var d in selector.GetRepository<T>().OrderBy(d => d.id))
            {
                List.Add(d);
            }
        }

        public ReadOnlyObservableCollection<TSelector> Selectors
        {
            get { return _readOnlySelectors; }
        }

        public TSelector CurrentSelector
        {
            get { return _currentSelector; }
            set
            {
                if (HasChanges)
                {
                    _service.DialogHelper.Message(Res.CantChangeSelectorMessage, Name, owner: View);
                    return;
                }
                _currentSelector = value;
                OnPropertyChanged(this, v => v.CurrentSelector);
                RaiseAddCommandCanExecuteChanged();
                try
                {
                    LoadCollection();
                }
                catch (Exception exception)
                {
                    _service.DialogHelper.Exception(exception, View);
                    RaiseClosed();
                }
            }
        }
        
        protected override void OnBeforeAddedToContext(T entity)
        {
            CurrentSelector.GetRepository<T>().Add(entity);
        }

        protected override bool AddCommandCanExecute(object obj)
        {
            return base.AddCommandCanExecute(obj) && CurrentSelector != null;
        }
    }
}
