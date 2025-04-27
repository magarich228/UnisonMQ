namespace UnisonMQ.Abstractions;

public interface IQueueService
{
    void Subscribe(Guid clientId, int sid, string subject);
    void Unsubscribe(Guid clientId, int? sid, int? maxMessages = null);
    ClientSubscription[] GetSubscribersForSend(string queueName);
}