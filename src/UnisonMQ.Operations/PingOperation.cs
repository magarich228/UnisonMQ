using UnisonMQ.Abstractions;

namespace UnisonMQ.Operations;

internal class PingOperation : Operation
{
    private const string PongResponse = "PONG\r\n";

    public override string Keyword => "PING";
    
    public override void ExecuteAsync(IUnisonMqSession session, string message)
    {
        session.SendAsync(PongResponse);
    }
}