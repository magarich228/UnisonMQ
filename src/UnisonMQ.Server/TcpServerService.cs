using System.Buffers;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace UnisonMQ.Server;

internal class TcpServerService : BackgroundService
{
    private readonly ILogger<TcpServerService> _logger;
    private readonly TcpListener _listener;

    public TcpServerService(ILogger<TcpServerService> logger, TcpServerConfiguration config)
    {
        _logger = logger;
        
        var ip = IPAddress.Parse(config.Ip);
        var port = config.Port;
        
        _listener = new TcpListener(ip, port);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _listener.Start();
        _logger.LogInformation("Server started on {Endpoint}", _listener.LocalEndpoint);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync(stoppingToken);
                _ = HandleClientAsync(client, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // TODO: 
        }
        finally
        {
            _listener.Stop();
            _logger.LogInformation("Server stopped");
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken ct)
    {
        using (client)
        await using (var stream = client.GetStream())
        {
            var buffer = ArrayPool<byte>.Shared.Rent(4096);
            try
            {
                while (true)
                {
                    int bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), ct);

                    if (bytesRead == 0)
                        break;
                    
                    await stream.WriteAsync(buffer.AsMemory(0, bytesRead), ct);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Client handle error");
            }
        }
    }
}