using UnisonMQ.Client;

namespace UnisonMQ.E2E;

[TestFixture]
public class Tests
{
    private IUnisonMqClient _client;
    
    [SetUp]
    public void Setup()
    {
        _client = new UnisonMqClient("127.0.0.1", 5889);
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
    public void Test1()
    {
        _client.Ping();
        
        Assert.Pass();
    }
}