namespace UnisonMQ.Abstractions;

public class ClientSubscription
{
    public ClientSubscription(Guid clientId, int sid, string queueName)
    {
        ClientId = clientId;
        Sid = sid;
        QueueName = queueName;
    }

    public Guid ClientId { get; }
    public int Sid { get; }
    public string QueueName { get; }
}