using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetCoreServer;

namespace UnisonMQ.Client;

public class UnisonMqClient(UnisonMqConfiguration configuration)
    : TcpClient(configuration.Ip, configuration.Port), IUnisonMqClient
{
    private readonly Dictionary<long, UnisonMessageFactory> _subscriptions = new();
    private readonly CommunicationManager _manager = new();

    private long _subscriptionId;

    #region Ctors

    public UnisonMqClient(string hostname, int port) : this(new UnisonMqConfiguration()
    {
        Ip = hostname,
        Port = port
    }) { }

    #endregion

    #region IUnisonMqClient
    
    public Guid? ConnectionId { get; private set; }

    public void Ping()
    {
        base.SendAsync("ping\r\n");
        var expectationResult = _manager.ExpectDuring(TimeSpan.FromSeconds(5), Response.Pong);

        if (expectationResult != 0)
        {
            throw new UnisonMqClientException("Pong not received.");
        }
    }
    
    public void Subscribe<T>(string subject, Action<UnisonMessage<T>> onReceived)
    {
        var sid = _subscriptionId++;
        
        base.SendAsync($"sub {subject} {sid}\r\n");
        var expectationResult = _manager.ExpectDuring(
            TimeSpan.FromSeconds(5), 
            Response.Ok, 
            Response.Error);
        
        if (expectationResult != 0)
        {
            throw new UnisonMqClientException($"Subscription operation failed with code {expectationResult}.");
        }

        _subscriptions.Add(sid, new UnisonMessageFactory<T>(onReceived));
    }

    public void Publish(string subject, object data)
    {
        var messageBody = Serialization.Serialize(data);
        var messageBodyLength = messageBody.Length;
        
        var message = $"pub {subject} {messageBodyLength}\r\n";
        var messageBytes = Encoding.UTF8.GetBytes(message);

        base.SendAsync(messageBytes, 0, messageBytes.Length);
        base.SendAsync(messageBody, 0, messageBodyLength);
        
        // var buffer = ArrayPool<byte>.Shared.Rent(messageBytes.Length + messageBodyLength);
        // Buffer.BlockCopy(messageBytes, 0, buffer, 0, messageBytes.Length);
        // Buffer.BlockCopy(messageBody, 0, buffer, messageBytes.Length, messageBodyLength);
        
        // base.SendAsync(buffer, 0, messageBytes.Length + messageBodyLength);
        var expectationResult = _manager.ExpectDuring(
            TimeSpan.FromSeconds(5), 
            Response.Ok, 
            Response.Error);
        
        if (expectationResult != 0)
        {
            throw new UnisonMqClientException($"Publish operation failed with code {expectationResult}.");
        }
    }

    public override bool ConnectAsync()
    {
        return base.Connect();
    }

    public bool CloseAsync()
    {
        return base.DisconnectAsync();
    }
    
    #endregion

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        Console.WriteLine($"Received: {Encoding.UTF8.GetString(buffer)}, offset: {offset}, size: {size}, count: {buffer.Length}");
        
        if (buffer.ElementAtOrDefault(0) == 'M' &&
            buffer.ElementAtOrDefault(1) == 'S' &&
            buffer.ElementAtOrDefault(2) == 'G' &&
            buffer.ElementAtOrDefault(3) == ' ')
        {
            Console.WriteLine("Process message..");
            ProcessMessage(buffer);
        }
        else if (buffer.ElementAtOrDefault(0) == '+' &&
                 buffer.ElementAtOrDefault(1) == 'O' &&
                 buffer.ElementAtOrDefault(2) == 'K')
        {
            Console.WriteLine("Ok received..");
            _manager.Received(Response.Ok);
        }
        else if (buffer.ElementAtOrDefault(0) == '-' &&
                 buffer.ElementAtOrDefault(1) == 'E' &&
                 buffer.ElementAtOrDefault(2) == 'R' &&
                 buffer.ElementAtOrDefault(3) == 'R')
        {
            Console.WriteLine("Error received..");
            _manager.Received(Response.Error);
        }
        else if (buffer.ElementAtOrDefault(0) == 'P' &&
                buffer.ElementAtOrDefault(1) == 'O' &&
                buffer.ElementAtOrDefault(2) == 'N' &&
                buffer.ElementAtOrDefault(3) == 'G')
        {
            Console.WriteLine("Pong received..");
            _manager.Received(Response.Pong);
        }

        base.OnReceived(buffer, offset, size);
    }

    // protected override void OnConnected()
    // {
    //     const long connectedMessageLength = 61;
    //     const long infoMessageLength = 44;
    //     
    //     var infoMessage = base.Receive(connectedMessageLength);
    //
    //     var nIndex = infoMessage.IndexOf('\n');
    //     var info = infoMessage.Substring(nIndex + 1);
    //
    //     if (string.IsNullOrWhiteSpace(info))
    //     {
    //         info = base.Receive(infoMessageLength);
    //     }
    //
    //     var connectionId = info.Split(' ').Last();
    //
    //     ConnectionId = Guid.Parse(connectionId);
    //
    //     base.OnConnected();
    // }

    private void ProcessMessage(byte[] buffer)
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

        var messageBodyBuffer = ArrayPool<byte>.Shared.Rent((int) messageLength);
        Array.Copy(buffer, nIndex + 1, messageBodyBuffer, 0, messageLength);

        if (_subscriptions.TryGetValue(sid, out var factory))
        {
            factory.CreateAndInvoke(subject, messageBodyBuffer);
        }

        ArrayPool<byte>.Shared.Return(messageBodyBuffer);
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