using UnisonMQ.Abstractions;

namespace UnisonMQ.Operations;

internal class ExitOperation : Operation
{
    public override string Keyword => "exit";
    
    public override void ExecuteAsync(IUnisonMqSession session, string message)
    {
        session.SendAsync(ResultHelper.Ok());
        session.Disconnect();
    }
}