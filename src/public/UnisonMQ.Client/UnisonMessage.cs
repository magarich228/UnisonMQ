using System;

namespace UnisonMQ.Client;

public abstract class UnisonMessage
{
    // internal abstract UnisonMessage CreateAndInvoke(byte[] data);
}

internal abstract class UnisonMessageFactory
{
    public abstract void CreateAndInvoke(string subject, byte[] data);
}

internal class UnisonMessageFactory<T> : UnisonMessageFactory
{
    private readonly Action<UnisonMessage<T>> _onReceived;

    public UnisonMessageFactory(Action<UnisonMessage<T>> onReceived)
    {
        _onReceived = onReceived;
    }
    
    public override void CreateAndInvoke(string subject, byte[] data)
    {
        var messageData = Serialization.Deserialize<T>(data);
        
        var message = new UnisonMessage<T>(messageData, subject);

        _onReceived(message);
    }
}

public class UnisonMessage<T>(T data, string subject) : UnisonMessage
{
    public T Data { get; } = data;
    public string Subject { get; } = subject;
}