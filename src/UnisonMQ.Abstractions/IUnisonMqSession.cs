namespace UnisonMQ.Abstractions;

public interface IUnisonMqSession
{
    bool SendAsync(ReadOnlySpan<char> text);
    bool SendAsync(string text);

    bool Disconnect();
}