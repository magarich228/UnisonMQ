using System;
using UnisonMQ.Client;

namespace UnisonMQ.Performance;

[TestFixture]
public class PerformanceTests
{
    [Test]
    public void OneTimePubTest()
    {
        IUnisonMqClient client = new UnisonMqClientService("127.0.0.1", 5888);
        
        var connected = client.ConnectAsync();
        
        Assert.That(connected, Is.True);
        
        client.Publish("test", new Message());
        
        Assert.Pass();
        
        client.CloseAsync();
        client.DisposeAsync();
    }
}

public class Message
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Time { get; set; } = DateTime.Now;
}