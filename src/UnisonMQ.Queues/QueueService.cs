using UnisonMQ.Abstractions;

namespace UnisonMQ.Queues;

internal class QueueService : IQueueService
{
    public void Subscribe(Guid clientId, int sid, string queue)
    {
        
    }

    public void Unsubscribe(int sid, int maxMessages = 0)
    {
        throw new NotImplementedException();
    }
}