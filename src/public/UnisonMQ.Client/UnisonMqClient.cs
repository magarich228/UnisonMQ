using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetCoreServer;

namespace UnisonMQ.Client;

internal class UnisonMqClient(UnisonMqConfiguration configuration)
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

    public void Ping()
    {
        base.SendAsync("ping\r\n");
        var expectationResult = _manager.ExpectDuring(TimeSpan.FromSeconds(5), Response.Pong);

        if (expectationResult != 0)
        {
            throw new UnisonMqClientException($"Pong not received {expectationResult}.");
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
        var fullMessageBodyLength = messageBodyLength + 2;

        Array.Resize(ref messageBody, fullMessageBodyLength);
        messageBody[fullMessageBodyLength - 2] = 13;
        messageBody[fullMessageBodyLength - 1] = 10;

        var message = $"pub {subject} {messageBodyLength}\r\n";
        var messageBytes = Encoding.UTF8.GetBytes(message);
        var messageBytesLength = messageBytes.Length;

        var buffer = new byte[messageBytes.Length + fullMessageBodyLength];

        Array.Copy(messageBytes, 0, buffer, 0, messageBytesLength);
        Array.Copy(messageBody, 0, buffer, messageBytesLength, messageBodyLength);

        base.SendAsync(buffer);

        var expectationResult = _manager.ExpectDuring(
            TimeSpan.FromSeconds(60),
            Response.Ok,
            Response.Error);

        if (expectationResult != 0)
        {
            throw new UnisonMqClientException($"Publish operation failed with code {expectationResult}.");
        }
    }

    public override bool ConnectAsync()
    {
        var connected = base.Connect();

        if (connected)
            base.ReceiveAsync();

        return connected;
    }

    public bool CloseAsync()
    {
        return base.DisconnectAsync();
    }

    #endregion

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        var data = new byte[size];
        Array.Copy(buffer, offset, data, 0, data.Length);

        if (data.ElementAtOrDefault(0) == 'M' &&
            data.ElementAtOrDefault(1) == 'S' &&
            data.ElementAtOrDefault(2) == 'G' &&
            data.ElementAtOrDefault(3) == ' ')
        {
            var processed = ProcessMessage(data);

            data = GetTrimmedBuffer(data, processed, size - processed);
        }
        
        if (data.ElementAtOrDefault(0) == '+' &&
            data.ElementAtOrDefault(1) == 'O' &&
            data.ElementAtOrDefault(2) == 'K')
        {
            _manager.Received(Response.Ok);
        }
        else if (data.ElementAtOrDefault(0) == '-' &&
                 data.ElementAtOrDefault(1) == 'E' &&
                 data.ElementAtOrDefault(2) == 'R' &&
                 data.ElementAtOrDefault(3) == 'R')
        {
            _manager.Received(Response.Error);
        }
        else if (data.ElementAtOrDefault(0) == 'P' &&
                 data.ElementAtOrDefault(1) == 'O' &&
                 data.ElementAtOrDefault(2) == 'N' &&
                 data.ElementAtOrDefault(3) == 'G')
        {
            _manager.Received(Response.Pong);
        }

        base.OnReceived(buffer, offset, size);
    }

    private byte[] GetTrimmedBuffer(byte[] buffer, long offset, long size)
    {
        // TODO: profile.
        var dataQuery = buffer
            .Skip((int)offset)
            .Take((int)size)
            .SkipWhile(b => b == 0);

        return dataQuery.ToArray();
    }

    private long ProcessMessage(byte[] buffer)
    {
        var nIndex = Array.IndexOf(buffer, (byte)'\n');
        var messageBufferRequiredLength = nIndex + 1;
        var messageBuffer = ArrayPool<byte>.Shared.Rent(messageBufferRequiredLength);

        Array.Copy(buffer, messageBuffer, messageBufferRequiredLength);
        var message = Encoding.UTF8.GetString(messageBuffer);

        ArrayPool<byte>.Shared.Return(messageBuffer);

        var parts = message.Split(' ');

        var subject = parts[1];
        var sid = long.Parse(parts[2]);
        var messageLength = long.Parse(parts[3]);

        var messageBodyBuffer = new byte[messageLength];
        Array.Copy(buffer, messageBufferRequiredLength, messageBodyBuffer, 0, messageLength);

        if (_subscriptions.TryGetValue(sid, out var factory))
        {
            factory.CreateAndInvoke(subject, messageBodyBuffer);
        }

        return messageBufferRequiredLength + messageLength;
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