using DemoConsole.Contracts;
using UnisonMQ.Client;

await using IUnisonMqClient client = new UnisonMqClientService("127.0.0.1", 5888);

client.ConnectAsync();

client.Ping();

while (true)
{
    try
    {
        client.Publish("time", new TimeEvent());
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }

    Console.WriteLine("Published message.");
    
    await Task.Delay(1000);
}