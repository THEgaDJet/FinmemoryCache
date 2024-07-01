using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FinMemoryCache.Services;
using GenericInMemoryCache.Classes;
using GenericInMemoryCache.Config;
using GenericInMemoryCache.Interfaces;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Configuration.Sources.Clear();

IHostEnvironment env = builder.Environment;

builder.Configuration
    .AddJsonFile("appsettings.json", true, true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);

builder.Services.Configure<GenericInMemoryCacheOptions>(
    builder.Configuration.GetSection(key: nameof(GenericInMemoryCacheOptions)));
builder.Services.AddSingleton<IGenericInMemoryCache<object>, GenericInMemoryCache<object>>();
builder.Services.AddTransient<Observer>();
builder.Services.AddTransient<TestService>();

using IHost host = builder.Build();

TestCache(host.Services);

await host.RunAsync();

static void TestCache(IServiceProvider hostProvider)
{
    using IServiceScope serviceScope = hostProvider.CreateScope();
    IServiceProvider provider = serviceScope.ServiceProvider;
    TestService testService = provider.GetRequiredService<TestService>();
    testService.TestCache();
}