namespace UnisonMQ.Abstractions;

public class UnisonMqException : Exception
{
    public UnisonMqException() { }
    public UnisonMqException(string? message) : base(message) { }
    public UnisonMqException(string? message, Exception? inner) : base(message, inner) { }
}