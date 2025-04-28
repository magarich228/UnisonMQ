using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using UnisonMQ.Abstractions;

namespace UnisonMQ.Operations;

internal class PublishOperation : Operation
{
    private readonly IQueueService _queueService;
    private readonly ISessionManager _sessionManager;
    private readonly ILogger<PublishOperation> _logger;
    
    private readonly Regex _subjectRegex = new(@"^(?!\.)(?!.*\.$)([a-zA-Z0-9_-]+)(\.[a-zA-Z0-9_-]+)*$");

    public PublishOperation(
        IQueueService queueService,
        ISessionManager sessionManager, ILogger<PublishOperation> logger)
    {
        _queueService = queueService;
        _sessionManager = sessionManager;
        _logger = logger;
    }

    public override string Keyword => "PUB";

    public override void ExecuteAsync(IUnisonMqSession session, string message)
    {
        var parts = message.Split(' ');

        if (parts.Length != 4)
        {
            session.SendAsync("Invalid protocol message format.".Error());
            session.Disconnect();

            return;
        }

        var subject = parts[1];

        if (string.IsNullOrWhiteSpace(subject) ||
            !IsValidPublishSubject(subject))
        {
            session.SendAsync("Invalid subject.".Error());
            session.Disconnect();

            return;
        }

        var messageLengthPath = parts[2];

        if (!int.TryParse(messageLengthPath, out var messageLength))
        {
            session.SendAsync("Invalid message length operation argument.".Error());
            session.Disconnect();

            return;
        }

        var part3 = parts[3];
        var messageBody = part3.Substring(0, part3.Length - 2);

        if (messageBody.Length != messageLength)
        {
            session.SendAsync("Invalid message length.".Error());
            session.Disconnect();

            return;
        }
        
        var subs = _queueService.GetSubscribersForSend(subject);

        foreach (var sub in subs)
        {
            if (!_sessionManager.TryGet(sub.ClientId, out var subSession) ||
                subSession == null)
            {
                _logger.LogWarning("No session received for {0} {1}", sub.ClientId, sub.Sid);
                
                continue;
            }

            // var messageBytes = payloadBuffer.MessageBytes(subject, sub.Sid, messageLength);
            var result = subSession.SendAsync(messageBody.Message(sub.QueueName, sub.Sid, messageLength));

            _logger.LogTrace("Sent {0} {1} {2}", sub.ClientId, sub.Sid, result);
        }

        session.SendAsync(ResultHelper.Ok());
    }

    public bool IsValidPublishSubject(string subject)
    {
        return _subjectRegex.IsMatch(subject);
    }
}