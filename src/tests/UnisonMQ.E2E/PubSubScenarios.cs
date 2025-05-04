using UnisonMQ.Client;

namespace UnisonMQ.E2E;

[TestFixture]
public class PubSubScenarios
{
    private IUnisonMqClient _client = null!;
    
    [OneTimeSetUp]
    public void Setup()
    {
        _client = new UnisonMqClientService("127.0.0.1", 5888);
        var connected = _client.ConnectAsync();
        
        Assert.IsTrue(connected);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _client.CloseAsync();
        _client.DisposeAsync();
    }

    [Test]
    public void SubAnd5PubsTest()
    {
        var subject = "test";
        var testMessage = new TestMessage();
        
        var messageCounter = 0;
        var expectedMessages = 5;
        
        _client.Subscribe<TestMessage>(subject, m =>
        {
            Interlocked.Increment(ref messageCounter);
        });

        TryPublish(_client, subject, testMessage);
        TryPublish(_client, subject, testMessage);
        TryPublish(_client, subject, testMessage);
        TryPublish(_client, subject, testMessage);
        TryPublish(_client, subject, testMessage);
        
        // Task.Delay(1000).Wait();
        
        Assert.That(messageCounter, Is.EqualTo(expectedMessages));
    }
    
    private bool TryPublish(IUnisonMqClient client, string subject, TestMessage testMessage)
    {
        try
        {
            client.Publish(subject, testMessage);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return false;
        }
    }
}