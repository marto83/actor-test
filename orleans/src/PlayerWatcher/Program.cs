using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrleansTest.Grains;
using OrleansTest.PlayerWatcher;

Console.Title = "PlayerWatcher";

await Host.CreateDefaultBuilder()
    .UseOrleansClient(builder =>
    {
        builder.UseLocalhostClustering();
    })
    .ConfigureServices(services =>
    {
        // Add regular services
        services.AddTransient<IGameObserver, LoggerGameObserver>();
        
        // This hosted service runs the sample logic
        services.AddSingleton<IHostedService, PlayerWatcherHostedService>();
    })
    .RunConsoleAsync();