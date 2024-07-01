using GenericInMemoryCache.Interfaces;

namespace GenericInMemoryCache.Classes;

public class Observer(IGenericInMemoryCache<object> _cache) : IObserver<EvictionNotification>
{
    private readonly List<string> _evictedKeys = [];
    private IDisposable? _cancellation;

    public virtual void Subscribe() => _cancellation = _cache.Subscribe(this);

    public virtual void Unsubscribe()
    {
        _cancellation?.Dispose();
        _evictedKeys.Clear();
    }

    public virtual void OnCompleted() => _evictedKeys.Clear();

    public virtual void OnError(Exception e)
    { }

    public virtual void OnNext(EvictionNotification notification)
    {
        _evictedKeys.Add(notification.CacheKey);
        Console.WriteLine($"Subscriber1 evicted keys - count:{_evictedKeys.Count}{Environment.NewLine}values:{string.Join(',', _evictedKeys)}");
    }

    public List<string> EvictedKeys => _evictedKeys;
}