using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using NetCoreServer;
using UnisonMQ.Abstractions;
using UnisonMQ.Operations;

namespace UnisonMQ.Server;

public class UnisonMqSession : TcpSession, IUnisonMqSession
{
    private readonly ILogger<UnisonMqSession> _logger;
    private readonly StringBuilder _buffer = new();
    
    public UnisonMqSession(
        TcpServer server,
        ILoggerFactory loggerFactory) : base(server)
    {
        _logger = loggerFactory.CreateLogger<UnisonMqSession>();
    }
    
    protected override void OnConnected()
    {
        _logger.LogInformation("Client connected. Id: {0}", base.Id);
        
        base.SendAsync("UnisonMQ Server\r\n"); // TODO: + INFO
        
        base.OnConnected();
    }

    protected override void OnDisconnected()
    {
        _logger.LogInformation("Client disconnected. Id: {0}", base.Id);
        
        base.OnDisconnected();
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        string data = Encoding.UTF8.GetString(buffer, (int) offset, (int) size);
        _buffer.Append(data);
        
        if (data.Contains("\r\n"))
        {
            _logger.LogInformation("Client: {0}", _buffer.ToString()); // TODO: Temp
            var message = _buffer.ToString();
            
            Processor.Execute(this, message);
            
            _buffer.Clear();
        }
        
        base.OnReceived(buffer, offset, size);
    }

    protected override void OnError(SocketError error)
    {
        base.OnError(error);
    }
}