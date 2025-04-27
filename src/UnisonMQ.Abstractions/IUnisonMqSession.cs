namespace UnisonMQ.Abstractions;

public interface IUnisonMqSession
{
    Guid Id { get; }

    long Receive(byte[] buffer);
    long Receive(byte[] buffer, long offset, long size);
    string Receive(long size);

    bool SendAsync(byte[] buffer);
    bool SendAsync(byte[] buffer, long offset, long size);
    bool SendAsync(ReadOnlySpan<byte> buffer);
    bool SendAsync(ReadOnlySpan<char> text);
    bool SendAsync(string text);

    bool Disconnect();
}