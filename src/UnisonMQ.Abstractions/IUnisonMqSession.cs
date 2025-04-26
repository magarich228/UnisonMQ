namespace UnisonMQ.Abstractions;

public interface IUnisonMqSession
{
    Guid Id { get; }
    
    bool SendAsync(ReadOnlySpan<char> text);
    bool SendAsync(string text);

    bool Disconnect();
}