using DemoConsole.Contracts;
using UnisonMQ.Client;

await using IUnisonMqClient client = new UnisonMqClientService("127.0.0.1", 5888);

client.ConnectAsync();

client.Ping();

client.Subscribe<TimeEvent>(
    "time",
    m => Console.WriteLine($"{m.Subject}: Id - {m.Data.MessageId}, Time - {m.Data.TimeStamp}"));

Console.WriteLine("Listening...");

Console.ReadKey();