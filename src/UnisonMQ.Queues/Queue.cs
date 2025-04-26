using System.Collections.Concurrent;

namespace UnisonMQ.Queues;

internal class Queue
{
    private readonly ConcurrentBag<int> _subscribers;
    
    public Queue(string name)
    {
        Name = name;

        _subscribers = new();
    }
    
    public string Name { get; }

    public void Subscribe(int sid)
    {
        _subscribers.Add(sid);
    }
}