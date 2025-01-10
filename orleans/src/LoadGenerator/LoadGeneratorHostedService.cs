using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OrleansTest.LoadGenerator;

public sealed class LoadGeneratorHostedService : BackgroundService
{
    private readonly ILogger<LoadGeneratorHostedService> _logger;
    private readonly IClusterClient _client;

    public LoadGeneratorHostedService(ILogger<LoadGeneratorHostedService> logger, IClusterClient client)
    {
        _logger = logger;
        _client = client;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Generate a set of Activities
    }
}
