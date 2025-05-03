using UnisonMQ.Client;

await using IUnisonMqClient client = new UnisonMqClient("127.0.0.1", 5888);

client.ConnectAsync();

client.Ping();

client.Subscribe<string>("test", m => Console.WriteLine($"DEMO {m.Subject} - {m.Data}"));
client.Publish("test", "testMsg");

Console.WriteLine("TEST");