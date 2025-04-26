using UnisonMQ.Abstractions;

namespace UnisonMQ.Operations;

internal class UnsubscribeOperation : Operation
{
    public override string Keyword => "UNSUB";
    public override void ExecuteAsync(IUnisonMqSession session, string message)
    {
        
    }
}