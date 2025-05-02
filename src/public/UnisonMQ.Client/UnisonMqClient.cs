using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetCoreServer;

namespace UnisonMQ.Client;

// TODO: internal
public class UnisonMqClient(UnisonMqConfiguration configuration)
    : TcpClient(configuration.Ip, configuration.Port), IUnisonMqClient
{
    private readonly Dictionary<long, UnisonMessageFactory> _subscriptions = new();
    private long _subscriptionId;
    
    public UnisonMqClient(string hostname, int port) : this(new UnisonMqConfiguration()
    {
        Ip = hostname,
        Port = port
    }) { }

    public void Ping()
    {
        var buffer = ArrayPool<byte>.Shared.Rent(4);
        
        base.Send("ping");
        base.Receive(buffer);
        
        var pong = Encoding.UTF8.GetString(buffer);
        ArrayPool<byte>.Shared.Return(buffer);

        if (pong != "PONG")
        {
            throw new UnisonMqClientException("Pong not received.");
        }
    }
    
    public void Subscribe<T>(string subject, Action<UnisonMessage<T>> onReceived)
    {
        var sid = _subscriptionId++;
        var buffer = ArrayPool<byte>.Shared.Rent(128);
        
        base.Send($"sub {subject} {sid}");
        base.Receive(buffer);
        
        var response = Encoding.UTF8.GetString(buffer);
        ArrayPool<byte>.Shared.Return(buffer);
        
        CheckResponse(response);
        
        _subscriptions.Add(sid, new UnisonMessageFactory<T>(onReceived));
    }
    
    public override bool ConnectAsync()
    {
        return base.Connect();
    }

    public bool CloseAsync()
    {
        return base.DisconnectAsync();
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        if (buffer.ElementAtOrDefault(0) == 'M' &&
            buffer.ElementAtOrDefault(1) == 'S' &&
            buffer.ElementAtOrDefault(2) == 'G' &&
            buffer.ElementAtOrDefault(3) == ' ')
        {
            var nIndex = Array.IndexOf(buffer, (byte)'\n');
            var messageBuffer = ArrayPool<byte>.Shared.Rent(nIndex + 1);

            Array.Copy(buffer, messageBuffer, messageBuffer.Length);
            var message = Encoding.UTF8.GetString(messageBuffer);
            
            ArrayPool<byte>.Shared.Return(messageBuffer);
            
            var parts = message.Split(' ');
            
            var subject = parts[1];
            var sid = long.Parse(parts[2]);
            var messageLength = long.Parse(parts[3]);
            
            var messageBodyBuffer = ArrayPool<byte>.Shared.Rent((int)messageLength);
            Array.Copy(buffer, nIndex + 1, messageBodyBuffer, 0, messageLength);

            if (_subscriptions.TryGetValue(sid, out var factory))
            {
                factory.CreateAndInvoke(subject, messageBodyBuffer);
            }
            
            ArrayPool<byte>.Shared.Return(messageBodyBuffer);
        }
        
        base.OnReceived(buffer, offset, size);
    }

    protected override void OnConnected()
    {
        // var buffer = ArrayPool<byte>.Shared.Rent(4);
        // TODO: get info
        base.OnConnected();
    }
    
    private void CheckResponse(string response)
    {
        if (response.StartsWith("+OK"))
        {
            return;
        }

        if (response.StartsWith("-ERR"))
        {
            var parts = response.Split(' ');
            throw new UnisonMqClientException(parts[1]);
        }
    }

    #region IDisposable
    protected virtual ValueTask DisposeAsyncCore()
    {
        base.Dispose(true);
        
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }
    
    #endregion
}