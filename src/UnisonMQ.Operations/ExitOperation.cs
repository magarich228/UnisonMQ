using UnisonMQ.Abstractions;

namespace UnisonMQ.Operations;

internal class ExitOperation : Operation
{
    public override string Keyword => "exit";
    
    public override OperationResult ExecuteAsync(IUnisonMqSession session, byte[] data, object? context = null)
    {
        session.SendAsync(ResultHelper.Ok());
        session.Disconnect();

        return new CompletedResult();
    }
}