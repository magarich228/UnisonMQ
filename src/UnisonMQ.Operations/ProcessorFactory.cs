using UnisonMQ.Abstractions;

namespace UnisonMQ.Operations;

internal class ProcessorFactory : IOperationProcessorFactory
{
    private readonly Operation[] _operations;
    
    public ProcessorFactory(Operation[] operations)
    {
        _operations = operations;
    }
    
    public IOperationProcessor Create()
    {
        return new Processor(_operations);
    }
}