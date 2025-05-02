using NATS.Net;

var client = new NatsClient();

await client.ConnectAsync();

var sub = client.SubscribeAsync<Test>("test.*");

_ = Task.Run(async () =>
{
    await Task.Delay(5000);
    
    await client.PublishAsync("test.2", new Test2()
    {
        Desc = "Test2 message"
    });

    await client.PublishAsync("test.1", new Test()
    {
        Name = "k.groshev",
        Age = 27
    });
});

await foreach (var s in sub)
{
    var test = s.Data;
    Console.WriteLine(test?.Name + " " + test?.Age);
}

public class Test
{
    public string Name { get; set; }
    public int Age { get; set; }
}

public class Test2
{
    public string Desc { get; set; }
}