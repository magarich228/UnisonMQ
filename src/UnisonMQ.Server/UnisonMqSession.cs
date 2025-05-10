using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using NetCoreServer;
using UnisonMQ.Abstractions;
using UnisonMQ.Metrics;

namespace UnisonMQ.Server;

public class UnisonMqSession : TcpSession, IUnisonMqSession
{
    private readonly IOperationProcessor _operationProcessor;
    private readonly ILogger<UnisonMqSession> _logger;
    
    public UnisonMqSession(
        TcpServer server,
        IOperationProcessorFactory operationProcessorFactory,
        ILogger<UnisonMqSession> logger) : base(server)
    {
        _operationProcessor = operationProcessorFactory.Create();
        _logger = logger;
    }
    
    protected override void OnConnected()
    {
        _logger.LogInformation("Client connected. Id: {0}", base.Id);
        
        base.SendAsync($"UnisonMQ Server\r\nInfo: {base.Id}\r\n");
        base.OnConnected();
        
        UnisonMetrics.ConnectionOpened();
    }

    protected override void OnDisconnected()
    {
        _logger.LogInformation("Client disconnected. Id: {0}", base.Id);
        
        base.OnDisconnected();
        
        UnisonMetrics.ConnectionClosed();
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        var data = new byte[size - offset];
        Array.Copy(buffer, offset, data, 0, data.Length);
        
        UnisonMetrics.BytesReceived(data.Length);
        
        _operationProcessor.Execute(this, data);
        
        base.OnReceived(buffer, offset, size);
    }

    protected override void OnError(SocketError error)
    {
        _logger.LogError("Client error. Id: {0}, error: {1}", base.Id, error);
        this.Disconnect();
        
        base.OnError(error);
    }
}