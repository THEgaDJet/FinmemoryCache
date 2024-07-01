using GenericInMemoryCache.Classes;
using GenericInMemoryCache.Config;
using GenericInMemoryCache.Interfaces;
using Microsoft.Extensions.Options;

namespace FinMemoryCache.Services;

internal sealed class TestService(Observer _subscriber, IGenericInMemoryCache<object> _cache, IOptions<GenericInMemoryCacheOptions> _options)
{            
    public void TestCache()
    {
        Console.WriteLine($"Cache max size {_options.Value.CacheSize}");

        _subscriber.Subscribe();

        var task1 = Task.Run(() => PopulateCache("Task1"));   
        var task2 = Task.Run(() => PopulateCache("Task2"));

        Task.WaitAll(task1, task2); 
    }

    private void PopulateCache(string name)
    {
        while (true)
        {
            var cacheVal = Random.Shared.Next(1, 10);
            Console.WriteLine($"{name} added val {cacheVal}");
            _cache.AddOrUpdate(cacheVal.ToString(), cacheVal);
            
            var sleep = Random.Shared.Next(3, 5);
            Thread.Sleep(sleep * 100);
        }
    }
}