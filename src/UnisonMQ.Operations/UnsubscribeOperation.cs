using System.Text;
using UnisonMQ.Abstractions;

namespace UnisonMQ.Operations;

internal class UnsubscribeOperation : Operation
{
    private readonly IQueueService _queueService;

    public UnsubscribeOperation(IQueueService queueService)
    {
        _queueService = queueService;
    }
    
    public override string Keyword => "UNSUB";
    
    public override OperationResult ExecuteAsync(IUnisonMqSession session, byte[] data, object? context = null)
    {
        var message = Encoding.UTF8.GetString(data);
        var parts = message.Split(' ');
        var partsLength = parts.Length;

        if (partsLength == 2)
        {
            if (!int.TryParse(parts[1], out var sid))
            {
                session.SendAsync("Protocol message sid argument format error.".Error());
                session.Disconnect();

                return CompletedResult.Instance;
            }
            
            _queueService.Unsubscribe(session.Id, sid);
            
            session.SendAsync(ResultHelper.Ok());
        }
        else if (partsLength == 3)
        {
            if (!int.TryParse(parts[1], out var sid))
            {
                session.SendAsync("Protocol message sid argument format error.".Error());
                session.Disconnect();

                return CompletedResult.Instance;
            }
            
            if (!int.TryParse(parts[2], out var maxMessages))
            {
                session.SendAsync("Protocol message max messages argument format error.".Error());
                session.Disconnect();

                return CompletedResult.Instance;
            }
            
            _queueService.Unsubscribe(session.Id, sid, maxMessages);
            
            session.SendAsync(ResultHelper.Ok());
        }
        else
        {
            session.SendAsync("Protocol message format error.".Error());
            session.Disconnect();
        }
        
        return CompletedResult.Instance;
    }
}