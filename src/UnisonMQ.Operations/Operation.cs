using UnisonMQ.Abstractions;

namespace UnisonMQ.Operations;

internal abstract class Operation
{
    public abstract string Keyword { get; }
    
    public abstract void ExecuteAsync(IUnisonMqSession session, string message);
}