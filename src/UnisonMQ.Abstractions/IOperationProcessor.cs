namespace UnisonMQ.Abstractions;

public interface IOperationProcessor
{
    void Execute(IUnisonMqSession session, byte[] data);
}