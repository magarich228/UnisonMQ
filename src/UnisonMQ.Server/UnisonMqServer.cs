using Microsoft.Extensions.Logging;
using NetCoreServer;

namespace UnisonMQ.Server;

internal class UnisonMqServer : TcpServer
{
    private readonly ILogger<UnisonMqServer> _logger;
    private readonly ILoggerFactory _loggerFactory;

    public UnisonMqServer(
        TcpServerConfiguration configuration,
        ILoggerFactory loggerFactory) : base(configuration.Ip, configuration.Port)
    {
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<UnisonMqServer>();
    }

    public UnisonMqServer(
        string address,
        int port,
        ILoggerFactory loggerFactory) : base(address, port)
    {
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<UnisonMqServer>();
    }

    public override bool Start()
    {
        _logger.LogInformation("Starting UnisonMQ server on {Address}:{Port}", Address, Port);
        
        return base.Start();
    }

    public override bool Stop()
    {
        _logger.LogInformation("Stopping UnisonMQ server on {Address}:{Port}", Address, Port);
        
        return base.Stop();
    }
    
    protected override TcpSession CreateSession() => new UnisonMqSession(this, _loggerFactory);
}