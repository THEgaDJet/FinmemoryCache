using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using GenericInMemoryCache.Config;
using GenericInMemoryCache.Interfaces;

namespace GenericInMemoryCache.Classes
{
    /// <summary>
    /// Assumption that DI will be used
    /// Used T as assumption is that "generic" in the task description meant that literally, 
    /// also assuming homogenous objects would be desired. Although T could be set to object.
    /// use IOptions to configure cache size with DI
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class GenericInMemoryCache<T> : IGenericInMemoryCache<T>, IObservable<EvictionNotification>
    {
        private readonly GenericInMemoryCacheOptions _options;
        private readonly ConcurrentDictionary<string, LinkedListNode<T>> _cacheObjects;
        private readonly LinkedList<T> _cache;
        private readonly object _cacheLock = new();
        private readonly int _cacheSize;
        private readonly HashSet<IObserver<EvictionNotification>> _observers = [];

        public GenericInMemoryCache(IOptions<GenericInMemoryCacheOptions> options)
        {
            _options = options.Value;
            if (_options.CacheSize <= 0)
            {
                throw new ArgumentException($"{nameof(GenericInMemoryCacheOptions.CacheSize)} must be greater than 0");
            }

            _cacheSize = _options.CacheSize;
            _cache = new();
            _cacheObjects = new ConcurrentDictionary<string, LinkedListNode<T>>();
        }
        
        // Added for testing
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
            if (_cacheObjects.TryGetValue(key, out var existingCacheNode))
            {
                var existingCacheValue = existingCacheNode.Value;

                // Move node to top of the list when used, and point dict to new location
                var newNode = UpdateCache(existingCacheNode, existingCacheValue);
                _cacheObjects[key] = newNode;
                return existingCacheValue;
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
            if (_cacheObjects.TryRemove(key, out var existingCacheNode))
            {
                _cache.Remove(existingCacheNode);
            }
        }
        #endregion
    }
}