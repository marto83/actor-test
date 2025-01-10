using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

Console.Title = "Presence Server";

await Host.CreateDefaultBuilder()
    
    .ConfigureLogging(builder => builder.AddConsole())
    .RunConsoleAsync();