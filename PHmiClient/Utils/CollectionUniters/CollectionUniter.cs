using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Markup;

namespace PHmiClient.Utils.CollectionUniters
{
    [ContentProperty("Collections")]
    public class CollectionUniter<T> : ReadOnlyObservableCollection<T>
    {
        private readonly List<IEnumerable<T>> _collectionList = new List<IEnumerable<T>>(); 
        private readonly ObservableCollection<IEnumerable<T>> _collections
            = new ObservableCollection<IEnumerable<T>>();

        private readonly Func<T, T, int> _comparer;

        public CollectionUniter(Func<T, T, int> comparer) : base(new ObservableCollection<T>())
        {
            _collections.CollectionChanged += CollectionsChanged;
            _comparer = comparer;
        }

        private void CollectionsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var newItem in e.NewItems)
                    {
                        AddCollection(newItem);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var oldItem in e.OldItems)
                    {
                        RemoveCollection(oldItem);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (var oldItem in e.OldItems)
                    {
                        RemoveCollection(oldItem);
                    }
                    foreach (var newItem in e.NewItems)
                    {
                        AddCollection(newItem);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    ResetCollections();
                    break;
            }
        }

        private void AddCollection(object newCollection)
        {
            var enumerable = (IEnumerable<T>)newCollection;
            var collection = (INotifyCollectionChanged)enumerable;
            foreach (var i in enumerable)
            {
                Insert(i);
            }
            collection.CollectionChanged += OneOfCollectionsChanged;
            _collectionList.Add(enumerable);
        }

        private void RemoveCollection(object oldCollection)
        {
            var enumerable = (IEnumerable<T>)oldCollection;
            var collection = (INotifyCollectionChanged)enumerable;
            foreach (var i in enumerable)
            {
                Remove(i);
            }
            collection.CollectionChanged -= OneOfCollectionsChanged;
            _collectionList.Remove(enumerable);
        }

        private void ResetCollections()
        {
            foreach (var collection in _collectionList.ToArray())
            {
                RemoveCollection(collection);
            }
        }

        private void OneOfCollectionsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var newItem in e.NewItems)
                    {
                        Insert((T)newItem);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var oldItem in e.OldItems)
                    {
                        Remove((T)oldItem);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (var oldItem in e.OldItems)
                    {
                        Remove((T)oldItem);
                    }
                    foreach (var newItem in e.NewItems)
                    {
                        Insert((T)newItem);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Reset();
                    break;
            }
        }

        public ObservableCollection<IEnumerable<T>> Collections
        {
            get { return _collections; }
        }

        private void Insert(T item)
        {
            var index = Items.BinarySearch(item, _comparer);
            if (index < 0)
                index = ~index;
            Items.Insert(index, item);
        }

        private void Remove(T item)
        {
            Items.Remove(item);
        }

        private void Reset()
        {
            var itemsToRemove = Items.ToList();
            foreach (var item in _collections.SelectMany(c => c))
            {
                itemsToRemove.Remove(item);
            }
            foreach (var item in itemsToRemove)
            {
                Items.Remove(item);
            }
        }
    }
}
