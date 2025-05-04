namespace UnisonMQ.E2E;

public class TestMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime TimeStamp = DateTime.Now;
    public TestFile File = new();
}

public class TestFile
{
    public string Name { get; set; } = Path.GetRandomFileName();
}