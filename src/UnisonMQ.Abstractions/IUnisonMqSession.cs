namespace UnisonMQ.Abstractions;

public interface IUnisonMqSession
{
    Guid Id { get; }

    long Receive(byte[] buffer);
    long Receive(byte[] buffer, long offset, long size);
    string Receive(long size);

    long Send(byte[] buffer);
    long Send(byte[] buffer, long offset, long size);
    long Send(ReadOnlySpan<byte> buffer);
    long Send(ReadOnlySpan<char> text);
    long Send(string text);
    
    bool SendAsync(byte[] buffer);
    bool SendAsync(byte[] buffer, long offset, long size);
    bool SendAsync(ReadOnlySpan<byte> buffer);
    bool SendAsync(ReadOnlySpan<char> text);
    bool SendAsync(string text);

    bool Disconnect();
}