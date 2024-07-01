using GenericInMemoryCache.Classes;

namespace GenericInMemoryCache.Interfaces
{
    public interface IGenericInMemoryCache<T>
    {
        int CurrentCacheSize { get; }

        void AddOrUpdate(string key, T cacheObject);
        T? Get(string key);
        void Delete(string key);

        IDisposable Subscribe(IObserver<EvictionNotification> observer);
    }
}