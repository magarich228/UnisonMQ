using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace UnisonMQ.Server;

internal class UnisonMqHostedService : IHostedService, IDisposable
{
    private readonly UnisonMqServer _server;
    private readonly ILogger<UnisonMqHostedService> _logger;

    public UnisonMqHostedService(
        UnisonMqServer server,
        ILogger<UnisonMqHostedService> logger)
    {
        _server = server;
        _logger = logger;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var success = _server.Start();
        
        _logger.LogDebug("Success start: {Success}", success);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _server.Dispose();
    }
}