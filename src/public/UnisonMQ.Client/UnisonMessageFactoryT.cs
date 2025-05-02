using System;

namespace UnisonMQ.Client;

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