namespace UnisonMQ.Operations;

internal abstract class OperationResult
{
    public abstract void Apply(Processor processor);
}