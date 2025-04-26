using UnisonMQ.Abstractions;

namespace UnisonMQ.Operations;

internal class SubscriptionOperation : Operation
{
    private readonly IQueueService _queueService;
    
    public SubscriptionOperation(IQueueService queueService)
    {
        _queueService = queueService;
    }
    
    public override string Keyword => "SUB";
    public override void ExecuteAsync(IUnisonMqSession session, string message)
    {
        var parts = message.Split(' ');
        
        // TODO: queue group
        if (parts.Length != 3)
        {
            session.SendAsync("Protocol message format error.".Error());
            session.Disconnect();

            return;
        }
        
        var queue = parts[1]; // TODO: validate
        
        if (!int.TryParse(parts[2], out int sid) ||
            sid < 0)
        {
            session.SendAsync("Invalid subscription identifier.\r\n".Error());
            session.Disconnect();

            return;
        }

        _queueService.Subscribe(session.Id, sid, queue);
        
        session.SendAsync(ResultHelper.Ok());
    }
}