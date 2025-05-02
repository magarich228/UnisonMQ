using UnisonMQ.Client;

namespace UnisonMQ.E2E;

[TestFixture]
public class Tests
{
    private IUnisonMqClient _client = null!;
    
    [SetUp]
    public void Setup()
    {
        _client = new UnisonMqClient("127.0.0.1", 5888);
        var connected = _client.ConnectAsync();
        
        Assert.IsTrue(connected);
    }

    [TearDown]
    public void TearDown()
    {
        var closed = _client.CloseAsync();
        _client.DisposeAsync();
        
        Assert.IsTrue(closed);
    }

    [Test]
    public void PingTest()
    {
        _client.Ping();
        
        Assert.Pass();
    }

    [Test]
    public void SimplePubSubTest()
    {
        var received = false;
        var subject = "test;";
        var messageStr = "testMessage";
        
        _client.Subscribe<string>(subject, (message =>
        {
            received = message.Subject == subject &&
                       message.Data == messageStr;
        }));
        
        _client.Publish("test", messageStr);

        Task.Delay(5000).Wait();
        
        Assert.IsTrue(received);
    }
}