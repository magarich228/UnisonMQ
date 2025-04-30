using UnisonMQ.Abstractions;

namespace UnisonMQ.Operations;

internal class PingOperation : Operation
{
    private const string PongResponse = "PONG\r\n";

    public override string Keyword => "PING";
    
    public override OperationResult ExecuteAsync(IUnisonMqSession session, byte[] data, object? context = null)
    {
        session.SendAsync(PongResponse);

        return new CompletedResult();
    }
}