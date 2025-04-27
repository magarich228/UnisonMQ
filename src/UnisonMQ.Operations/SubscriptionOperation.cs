using System.Text.RegularExpressions;
using UnisonMQ.Abstractions;

namespace UnisonMQ.Operations;

internal class SubscriptionOperation : Operation
{
    private readonly IQueueService _queueService;
    private readonly Regex _subjectRegex = new(@"^(?!\.)(?!.*\.$)([a-zA-Z0-9_*-]+)(\.[a-zA-Z0-9_*-]+)*(\.>)?$");
    
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
        
        var subject = parts[1]; // TODO: validate

        if (string.IsNullOrWhiteSpace(subject) ||
            !IsValidSubscriptionSubject(subject))
        {
            session.SendAsync("Invalid subject".Error());
            session.Disconnect();

            return;
        }
        
        if (!int.TryParse(parts[2], out int sid) ||
            sid < 0)
        {
            session.SendAsync("Invalid subscription identifier.".Error());
            session.Disconnect();

            return;
        }

        _queueService.Subscribe(session.Id, sid, subject);
        
        session.SendAsync(ResultHelper.Ok());
    }
    
    private bool IsValidSubscriptionSubject(string subject)
    {
        return _subjectRegex.IsMatch(subject) && 
               !subject.Contains("..") && 
               (subject.IndexOf('>') == -1 || subject.EndsWith(".>"));
    }
}