using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using UnisonMQ.Client;

namespace UnisonMQ.Performance;

[Category(Categories.Performance)]
[TestFixture]
public class PerformanceTests
{
    private static readonly ConcurrentDictionary<int, IUnisonMqClient> Clients = new();
    
    [Test]
    public void OneTimePubTest()
    {
        var threadId = Thread.CurrentThread.ManagedThreadId;
        File.AppendAllLines("log.txt", new[] { threadId.ToString() });

        if (!Clients.TryGetValue(threadId, out var client))
        {
            client = new UnisonMqClientService(new UnisonMqConfiguration());
            Clients.TryAdd(threadId, client);
            
            client.ConnectAsync();
        }
        
        client.Publish("test", new Message());
        
        Assert.Pass();
    }
}

public class Message
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Time { get; set; } = DateTime.Now;
}