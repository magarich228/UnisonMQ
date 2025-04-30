namespace UnisonMQ.Abstractions;

public interface IOperationProcessorFactory
{
    IOperationProcessor Create();
}