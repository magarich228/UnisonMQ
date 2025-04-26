namespace UnisonMQ.Abstractions;

public interface IQueueService
{
    void Subscribe(Guid clientId, int sid, string queue);
    void Unsubscribe(int sid, int maxMessages = 0);
}