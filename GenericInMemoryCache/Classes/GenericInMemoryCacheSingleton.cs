using System.Collections.Concurrent;
using GenericInMemoryCache.Interfaces;

namespace GenericInMemoryCache.Classes
{
    /// <summary>
    /// Realised DI might not be what was meant by Singleton
    /// So spun up a quick modification not using DI
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class GenericInMemoryCacheSingleton<T> : IGenericInMemoryCache<T>
    {
        private readonly ConcurrentDictionary<string, LinkedListNode<T>> _cacheObjects;
        private readonly LinkedList<T> _cache;
        private readonly object _cacheLock = new();
        private readonly HashSet<IObserver<EvictionNotification>> _observers = [];
        
        private int _cacheSize = 10;

        private static readonly Lazy<GenericInMemoryCacheSingleton<T>> _lazyInstance = new(() => new GenericInMemoryCacheSingleton<T>());

        private GenericInMemoryCacheSingleton()
        {
            _cache = new();
            _cacheObjects = new ConcurrentDictionary<string, LinkedListNode<T>>();
        }

        public static GenericInMemoryCacheSingleton<T> Instance { get { return _lazyInstance.Value; } }

        public int CacheSize { 
            set 
            {
                lock (_cacheLock)
                {
                    _cacheSize = value;
                }
            } 
        }

        public int CurrentCacheSize => _cache.Count;

        #region Public Methods
        public void AddOrUpdate(string key, T cacheObject)
        {
            _ = _cacheObjects.AddOrUpdate(
                key,
                addValueFactory: (_) => AddToCache(cacheObject),
                updateValueFactory: (_, existingNode) => UpdateCache(existingNode, cacheObject));
        }

        public T? Get(string key)
        {
            if (_cacheObjects.TryGetValue(key, out var cacheObjectValue))
            {
                return cacheObjectValue.Value;
            }
            return default;
        }

        public void Delete(string key) => DeleteFromCache(key);

        public IDisposable Subscribe(IObserver<EvictionNotification> observer)
        {
            _observers.Add(observer);

            return new Unsubscriber<EvictionNotification>(_observers, observer);
        }
        #endregion

        #region Private Methods
        // Use this method for the case where object does NOT exist
        private LinkedListNode<T> AddToCache(T cacheObject)
        {
            lock (_cacheLock)
            {
                if (_cache.Count >= _cacheSize)
                {
                    var evictedObject = _cache.Last;
                    var evictedKey = _cacheObjects.SingleOrDefault(o => o.Value == evictedObject).Key;
                    _cacheObjects.Remove(evictedKey, out _);

                    foreach (IObserver<EvictionNotification> observer in _observers)
                    {
                        observer.OnNext(new EvictionNotification(evictedKey));
                    }
                    _cache.RemoveLast();
                }

                return _cache.AddFirst(cacheObject);
            }
        }

        // Use this method for the case where object DOES exist
        private LinkedListNode<T> UpdateCache(LinkedListNode<T> existingNode, T newCacheObject)
        {
            lock (_cacheLock)
            {
                _cache.Remove(existingNode);

                return _cache.AddFirst(newCacheObject);
            }
        }

        private void DeleteFromCache(string key)
        {
            if (_cacheObjects.Remove(key, out var existingCacheNode))
            {
                _cache.Remove(existingCacheNode);
            }
        }
        #endregion
    }
}