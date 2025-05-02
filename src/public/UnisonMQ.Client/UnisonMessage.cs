namespace UnisonMQ.Client;

public class UnisonMessage<T>(T data, string subject)
{
    public T Data { get; } = data;
    public string Subject { get; } = subject;
}