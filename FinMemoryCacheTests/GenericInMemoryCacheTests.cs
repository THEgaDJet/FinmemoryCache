using GenericInMemoryCache.Classes;
using GenericInMemoryCache.Config;
using Microsoft.Extensions.Options;
using Moq;

namespace FinMemoryCacheTests;

public class GenericInMemoryCacheTests
{
    [Fact]
    public void InvalidConfig_Throws()
    {
        var mockOptions = new Mock<IOptions<GenericInMemoryCacheOptions>>();
        mockOptions.Setup(o => o.Value).Returns(new GenericInMemoryCacheOptions { CacheSize = 0 });

        Assert.Throws<ArgumentException>(() => new GenericInMemoryCache<int>(mockOptions.Object));
    }

    #region Add, Update, Get
    [Fact]
    public void AddFirstSingleValue_ReturnsCountOfOne()
    {
        var mockOptions = new Mock<IOptions<GenericInMemoryCacheOptions>>();
        mockOptions.Setup(o => o.Value).Returns(new GenericInMemoryCacheOptions { CacheSize = 3 });

        var genericInMemoryCache = new GenericInMemoryCache<char>(mockOptions.Object);

        var key = "1";
        genericInMemoryCache.AddOrUpdate(key, 'a');

        var currentSize = genericInMemoryCache.CurrentCacheSize;
        Assert.Equal(1, currentSize);
    }

    [Fact]
    public void AddThenGetSingleObject_ReturnsSameObject()
    {
        var mockOptions = new Mock<IOptions<GenericInMemoryCacheOptions>>();
        mockOptions.Setup(o => o.Value).Returns(new GenericInMemoryCacheOptions { CacheSize = 3 });
        
        var genericInMemoryCache = new GenericInMemoryCache<int>(mockOptions.Object);

        var key = "1";
        var cacheObject = 1000;
        genericInMemoryCache.AddOrUpdate(key, cacheObject);

        var cacheVal = genericInMemoryCache.Get(key);
        Assert.Equal(cacheObject, cacheVal);
        Assert.Equal(1, genericInMemoryCache.CurrentCacheSize);
    }

    [Fact]
    public void AddMultipleObjects_ReturnsCorrectObjects()
    {
        var mockOptions = new Mock<IOptions<GenericInMemoryCacheOptions>>();
        mockOptions.Setup(o => o.Value).Returns(new GenericInMemoryCacheOptions { CacheSize = 3 });

        var genericInMemoryCache = new GenericInMemoryCache<int>(mockOptions.Object);

        var key1 = "1";
        var key2 = "2";
        var key3 = "3";
        var cacheObject1 = 1000;
        var cacheObject2 = 2000;
        var cacheObject3 = 3000;

        genericInMemoryCache.AddOrUpdate(key1, cacheObject1);
        genericInMemoryCache.AddOrUpdate(key2, cacheObject2);
        genericInMemoryCache.AddOrUpdate(key3, cacheObject3);

        Assert.Equal(cacheObject1, genericInMemoryCache.Get(key1));
        Assert.Equal(cacheObject2, genericInMemoryCache.Get(key2));
        Assert.Equal(cacheObject3, genericInMemoryCache.Get(key3));
        Assert.Equal(3, genericInMemoryCache.CurrentCacheSize);
    }

    [Fact]
    public void AddToExistingKey_ReturnsUpdatedObject()
    {
        var mockOptions = new Mock<IOptions<GenericInMemoryCacheOptions>>();
        mockOptions.Setup(o => o.Value).Returns(new GenericInMemoryCacheOptions { CacheSize = 5 });

        var genericInMemoryCache = new GenericInMemoryCache<int>(mockOptions.Object);
        
        var key1 = "1";
        var key2 = "2";
        var cacheObject1 = 1000;
        var cacheObject2 = 2000;
        var cacheObjectUpdate = 1111;

        genericInMemoryCache.AddOrUpdate(key1, cacheObject1);
        genericInMemoryCache.AddOrUpdate(key2, cacheObject2);
        genericInMemoryCache.AddOrUpdate(key1, cacheObjectUpdate);

        var cacheVal = genericInMemoryCache.Get(key1);
        Assert.Equal(cacheObjectUpdate, cacheVal);
        Assert.Equal(2, genericInMemoryCache.CurrentCacheSize);
    }

    [Fact]
    public void AddAddUpdateAdd_ReturnsCorrectObjects()
    {
        var mockOptions = new Mock<IOptions<GenericInMemoryCacheOptions>>();
        mockOptions.Setup(o => o.Value).Returns(new GenericInMemoryCacheOptions { CacheSize = 2 });

        var genericInMemoryCache = new GenericInMemoryCache<int>(mockOptions.Object);

        var key1 = "1";
        var key2 = "2";
        var key4 = "4";
        var cacheObject1 = 1000;
        var cacheObject2 = 2000;
        var cacheObjectUpdate = 111;
        var cacheObject4 = 4000;

        genericInMemoryCache.AddOrUpdate(key1, cacheObject1);
        genericInMemoryCache.AddOrUpdate(key2, cacheObject2);
        genericInMemoryCache.AddOrUpdate(key1, cacheObjectUpdate);
        genericInMemoryCache.AddOrUpdate(key4, cacheObject4);

        var updatedCacheVal = genericInMemoryCache.Get(key1);
        var addedCacheVal = genericInMemoryCache.Get(key4);
        Assert.Equal(cacheObjectUpdate, updatedCacheVal);
        Assert.Equal(cacheObject4, addedCacheVal);

        var ejectedCacheVal = genericInMemoryCache.Get(key2);
        Assert.Equal(0, ejectedCacheVal);
    }

    [Fact]
    public void AddsWithEviction_ReturnsCorrectObjects()
    {
        var mockOptions = new Mock<IOptions<GenericInMemoryCacheOptions>>();
        mockOptions.Setup(o => o.Value).Returns(new GenericInMemoryCacheOptions { CacheSize = 2 });

        var genericInMemoryCache = new GenericInMemoryCache<int>(mockOptions.Object);

        var key1 = "1";
        var key2 = "2";
        var key4 = "4";
        var cacheObject1 = 1000;
        var cacheObject2 = 2000;
        var cacheObject4 = 4000;

        genericInMemoryCache.AddOrUpdate(key1, cacheObject1);
        genericInMemoryCache.AddOrUpdate(key2, cacheObject2);
        genericInMemoryCache.AddOrUpdate(key4, cacheObject4);

        var mostRecentlyAddedCacheVal = genericInMemoryCache.Get(key4);
        var previousCacheVal = genericInMemoryCache.Get(key2);
        var evictedCacheVal = genericInMemoryCache.Get(key1);

        Assert.Equal(cacheObject4, mostRecentlyAddedCacheVal);
        Assert.Equal(cacheObject2, previousCacheVal);
        Assert.Equal(2, genericInMemoryCache.CurrentCacheSize);
        Assert.Equal(0, evictedCacheVal);
    }
    
    [Fact]
    public void GetFromEmptyCache_ReturnsNull()
    {
        var mockOptions = new Mock<IOptions<GenericInMemoryCacheOptions>>();
        mockOptions.Setup(o => o.Value).Returns(new GenericInMemoryCacheOptions { CacheSize = 1 });

        var genericInMemoryCache = new GenericInMemoryCache<string>(mockOptions.Object);

        var key = "nothing";

        var cacheObject = genericInMemoryCache.Get(key);

        var currentSize = genericInMemoryCache.CurrentCacheSize;
        Assert.Equal(0, currentSize);
        Assert.Null(cacheObject);
    }

    [Fact]
    public void GetNonExistingKey_ReturnsNull()
    {
        var mockOptions = new Mock<IOptions<GenericInMemoryCacheOptions>>();
        mockOptions.Setup(o => o.Value).Returns(new GenericInMemoryCacheOptions { CacheSize = 1 });

        var genericInMemoryCache = new GenericInMemoryCache<string>(mockOptions.Object);
        
        var existingKey = "existingKey";
        genericInMemoryCache.AddOrUpdate(existingKey, existingKey);

        var nonExistingKey = "nonExistingKey";
        var cacheObject = genericInMemoryCache.Get(nonExistingKey);

        var currentSize = genericInMemoryCache.CurrentCacheSize;
        Assert.Equal(1, currentSize);
        Assert.Null(cacheObject);
    }

    [Fact]
    public void RegularlyAccessedObject_DoesNotGetEvicted()
    {
        var mockOptions = new Mock<IOptions<GenericInMemoryCacheOptions>>();
        mockOptions.Setup(o => o.Value).Returns(new GenericInMemoryCacheOptions { CacheSize = 3 });

        var genericInMemoryCache = new GenericInMemoryCache<int>(mockOptions.Object);

        var evictedKey1 = "1";
        var evictedKey2 = "2";
        var regularAccessKey = "regular";
        var evictedKey3 = "3";
        var key5 = "5";
        var key6 = "6";
        var key7 = "7";
        var cacheObject1 = 1000;
        var cacheObject2 = 2000;
        var cacheObject3 = 2000;
        var regularCacheObject = 4000;
        var cacheObject5 = 5000;
        var cacheObject6 = 6000;
        var cacheObject7 = 7000;

        genericInMemoryCache.AddOrUpdate(regularAccessKey, regularCacheObject);
        genericInMemoryCache.AddOrUpdate(evictedKey1, cacheObject1);
        genericInMemoryCache.AddOrUpdate(evictedKey2, cacheObject2);
        genericInMemoryCache.Get(regularAccessKey); // [3, 2, 1]
        
        // 1 should be evicted
        genericInMemoryCache.AddOrUpdate(evictedKey3, cacheObject3); // [4, 3, 2]
        
        // 2 should be evicted
        genericInMemoryCache.Get(regularAccessKey); // [3, 4, 2]
        genericInMemoryCache.AddOrUpdate(key5, cacheObject5); // [5, 3, 4]

        genericInMemoryCache.AddOrUpdate(key6, cacheObject6); // [6, 5, 3]
        genericInMemoryCache.Get(regularAccessKey); // [3, 6, 5]
        genericInMemoryCache.AddOrUpdate(key7, cacheObject7); // [7, 3, 6]

        // No hits on 1, 2, 3, 5
        Assert.Equal(default, genericInMemoryCache.Get(evictedKey1));
        Assert.Equal(default, genericInMemoryCache.Get(evictedKey2));
        Assert.Equal(default, genericInMemoryCache.Get(evictedKey3));
        Assert.Equal(default, genericInMemoryCache.Get(key5));

        // Hits on 7, 6, regular
        Assert.Equal(cacheObject7, genericInMemoryCache.Get(key7));
        Assert.Equal(regularCacheObject, genericInMemoryCache.Get(regularAccessKey));
        Assert.Equal(cacheObject6, genericInMemoryCache.Get(key6));
    }
    #endregion

    #region Delete
    [Fact]
    public void AddAddDelete_ReturnsCorrectObject()
    {
        var mockOptions = new Mock<IOptions<GenericInMemoryCacheOptions>>();
        mockOptions.Setup(o => o.Value).Returns(new GenericInMemoryCacheOptions { CacheSize = 2 });

        var genericInMemoryCache = new GenericInMemoryCache<int>(mockOptions.Object);

        var key1 = "1";
        var key4 = "4";
        var cacheObject1 = 1000;
        var cacheObject4 = 4000;

        genericInMemoryCache.AddOrUpdate(key1, cacheObject1);
        genericInMemoryCache.AddOrUpdate(key4, cacheObject4);
        genericInMemoryCache.Delete(key1);

        var addedCacheVal = genericInMemoryCache.Get(key4);
        Assert.Equal(cacheObject4, addedCacheVal);
        Assert.Equal(1, genericInMemoryCache.CurrentCacheSize);

        var deletedCacheVal = genericInMemoryCache.Get(key1);
        Assert.Equal(0, deletedCacheVal);
    }

    [Fact]
    public void DeleteInvalidKey_FailsGracefully()
    {
        var mockOptions = new Mock<IOptions<GenericInMemoryCacheOptions>>();
        mockOptions.Setup(o => o.Value).Returns(new GenericInMemoryCacheOptions { CacheSize = 1 });

        var genericInMemoryCache = new GenericInMemoryCache<int>(mockOptions.Object);

        var key1 = "1";
        var key4 = "4";
        var cacheObject1 = 1000;

        genericInMemoryCache.AddOrUpdate(key1, cacheObject1);
        genericInMemoryCache.Delete(key4);

        Assert.Equal(1, genericInMemoryCache.CurrentCacheSize);
    }

    [Fact]
    public void DeleteFromEmptyCache_FailsGracefully()
    {
        var mockOptions = new Mock<IOptions<GenericInMemoryCacheOptions>>();
        mockOptions.Setup(o => o.Value).Returns(new GenericInMemoryCacheOptions { CacheSize = 1 });

        var genericInMemoryCache = new GenericInMemoryCache<int>(mockOptions.Object);

        var key1 = "1";

        genericInMemoryCache.Delete(key1);

        Assert.Equal(0, genericInMemoryCache.CurrentCacheSize);
    }
    #endregion

    #region Notification
    [Fact]
    public void SubscribedObserver_ReceivesEvictedCacheObjects()
    {
        var mockOptions = new Mock<IOptions<GenericInMemoryCacheOptions>>();
        mockOptions.Setup(o => o.Value).Returns(new GenericInMemoryCacheOptions { CacheSize = 5 });

        var genericInMemoryCache = new GenericInMemoryCache<object>(mockOptions.Object);
        
        var subscriber = new Observer(genericInMemoryCache);
        subscriber.Subscribe();

        for (var i = 1; i <= 10; i++)
        {
            genericInMemoryCache.AddOrUpdate(i.ToString(), i);
        }

        var expectedEvictedKeys = new List<string> { "1", "2", "3", "4", "5" };

        Assert.Equal(expectedEvictedKeys, subscriber.EvictedKeys);
    }

    [Fact]
    public void MultipleSubscribers_ReceiveCorrectEvictedCacheObjects()
    {
        var mockOptions = new Mock<IOptions<GenericInMemoryCacheOptions>>();
        mockOptions.Setup(o => o.Value).Returns(new GenericInMemoryCacheOptions { CacheSize = 3 });

        var genericInMemoryCache = new GenericInMemoryCache<object>(mockOptions.Object);
        
        var subscriber1 = new Observer(genericInMemoryCache);
        subscriber1.Subscribe();

        for (var i = 1; i <= 5; i++)
        {
            genericInMemoryCache.AddOrUpdate(i.ToString(), i);
        }

        var subscriber2 = new Observer(genericInMemoryCache);
        subscriber2.Subscribe();

        for (var i = 6; i <= 10; i++)
        {
            genericInMemoryCache.AddOrUpdate(i.ToString(), i);
        }

        var expectedEvictedKeys1 = new List<string> { "1", "2", "3", "4", "5", "6", "7" };
        var expectedEvictedKeys2 = new List<string> { "3", "4", "5", "6", "7" };
        
        Assert.Equal(expectedEvictedKeys1, subscriber1.EvictedKeys);
        Assert.Equal(expectedEvictedKeys2, subscriber2.EvictedKeys);
    }
    #endregion 
}