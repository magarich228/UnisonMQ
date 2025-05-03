using System.Text;
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

    public override OperationResult ExecuteAsync(IUnisonMqSession session, byte[] data, object? context = null)
    {
        if (context == null)
        {
            var message = Encoding.UTF8.GetString(data);
            var parts = message.Split(' ');

            if (parts.Length != 3)
            {
                session.SendAsync("Invalid protocol message format.".Error());
                session.Disconnect();

                return CompletedResult.Instance;
            }

            var subject = parts[1];

            if (string.IsNullOrWhiteSpace(subject) ||
                !IsValidPublishSubject(subject))
            {
                session.SendAsync("Invalid subject.".Error());
                session.Disconnect();

                return CompletedResult.Instance;
            }

            var messageLengthPath = parts[2];

            if (!int.TryParse(messageLengthPath, out var messageLength))
            {
                var error = "Invalid message length operation argument.".Error();
                
                session.SendAsync(error);
                session.Disconnect();

                _logger.LogDebug(error + $" {messageLengthPath}");
                
                return CompletedResult.Instance;
            }

            var publishContext = new PublishContext(subject, messageLength);

            return new WaitInputResult(publishContext, messageLength);
        }
        else
        {
            var publishContext = context as PublishContext ??
                                 throw new UnisonMqException($"Context of an unexpected type {context.GetType()}.");
            
            var subs = _queueService.GetSubscribersForSend(publishContext.Subject);

            foreach (var sub in subs)
            {
                if (!_sessionManager.TryGet(sub.ClientId, out var subSession) ||
                    subSession == null)
                {
                    _logger.LogWarning("No session received for {0} {1}", sub.ClientId, sub.Sid);

                    continue;
                }

                var messageBytes = data.MessageBytes(publishContext.Subject, sub.Sid, publishContext.MessageLength);
                var result = subSession.SendAsync(messageBytes);

                _logger.LogTrace("Sent {0} {1} {2}", sub.ClientId, sub.Sid, result);
            }

            session.SendAsync(ResultHelper.Ok());
            
            return CompletedResult.Instance;
        }
    }

    public bool IsValidPublishSubject(string subject)
    {
        return _subjectRegex.IsMatch(subject);
    }

    class PublishContext
    {
        public PublishContext(string subject, int messageLength)
        {
            Subject = subject;
            MessageLength = messageLength;
        }

        public string Subject { get; }
        public int MessageLength { get; }
    }
}