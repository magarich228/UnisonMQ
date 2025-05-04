using Microsoft.Extensions.Logging;
using UnisonMQ.Abstractions;

namespace UnisonMQ.Operations;

internal class ProcessorFactory : IOperationProcessorFactory
{
    private readonly Operation[] _operations;
    private readonly ILogger<Processor> _processorLogger;
    
    public ProcessorFactory(Operation[] operations, ILoggerFactory loggerFactory)
    {
        _operations = operations;
        _processorLogger = loggerFactory.CreateLogger<Processor>();
    }
    
    public IOperationProcessor Create()
    {
        return new Processor(_operations, _processorLogger);
    }
}