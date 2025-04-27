using UnisonMQ.Abstractions;

namespace UnisonMQ.Queues;

internal readonly struct SubscriptionKey(Guid clientId, int sid)
{
    public Guid ClientId { get; } = clientId;
    public int Sid { get; } = sid;
    
    public ClientSubscription ToClientSubscription(string queueName) => new(ClientId, Sid, queueName);
}