using UnisonMQ.Abstractions;

namespace UnisonMQ.Queues;

internal class QueueService : IQueueService
{
    private readonly SubscriptionManager _subManager = new();
    
    public void Subscribe(Guid clientId, int sid, string subject)
    {
        _subManager.Subscribe(clientId, sid, subject);
    }

    public void Unsubscribe(Guid clientId, int? sid, int? maxMessages = null)
    {
        _subManager.Unsubscribe(clientId, sid, maxMessages);
    }

    public ClientSubscription[] GetSubscribersForSend(string queueName)
    {
        return _subManager.GetSubscribersForSend(queueName);
    }
}