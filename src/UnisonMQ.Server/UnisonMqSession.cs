using Microsoft.Extensions.Logging;
using NetCoreServer;

namespace UnisonMQ.Server;

public class UnisonMqSession : TcpSession
{
    private readonly ILogger<UnisonMqSession> _logger;
    
    public UnisonMqSession(
        TcpServer server,
        ILoggerFactory loggerFactory) : base(server)
    {
        _logger = loggerFactory.CreateLogger<UnisonMqSession>();
    }
    
    protected override void OnConnected()
    {
        _logger.LogInformation("Client connected. Id: {0}", base.Id);
        
        base.OnConnected();
    }

    protected override void OnDisconnected()
    {
        _logger.LogInformation("Client disconnected. Id: {0}", base.Id);
        
        base.OnDisconnected();
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        
        
        base.OnReceived(buffer, offset, size);
    }
}