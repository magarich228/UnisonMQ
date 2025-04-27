using System.Buffers;
using System.Text.RegularExpressions;
using UnisonMQ.Abstractions;

namespace UnisonMQ.Operations;

internal class PublishOperation : Operation
{
    private readonly IQueueService _queueService;
    private readonly ISessionManager _sessionManager;
    private readonly Regex _subjectRegex = new(@"^(?!\.)(?!.*\.$)([a-zA-Z0-9_-]+)(\.[a-zA-Z0-9_-]+)*$");
    
    public PublishOperation(
        IQueueService queueService, 
        ISessionManager sessionManager)
    {
        _queueService = queueService;
        _sessionManager = sessionManager;
    }
    
    public override string Keyword => "PUB";
    public override void ExecuteAsync(IUnisonMqSession session, string message)
    {
        var lines = message.Split("\r\n")
            .Where(l => !string.IsNullOrEmpty(l))
            .ToArray();
        
        var firstLine = lines.First();

        if (lines.Length == 2)
        {
            var parts = firstLine.Split(' ');

            if (parts.Length != 3)
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

            var subs = _queueService.GetSubscribersForSend(subject);
            
            var payload = lines[1];

            foreach (var sub in subs)
            {
                if (!_sessionManager.TryGet(sub.ClientId, out var subSession) ||
                    subSession == null)
                {
                    Console.WriteLine("Не получена сессия для подписчика!!!"); //TODO: Log
                    continue;
                }
                
                subSession.SendAsync(payload.Message(subject, sub.Sid, messageLength));
            }
            
            session.SendAsync(ResultHelper.Ok());
        }
        else if (lines.Length == 1)
        {
            var parts = firstLine.Split(' ');

            if (parts.Length != 3)
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

            var subs = _queueService.GetSubscribersForSend(subject);
            
            var payloadBuffer = ArrayPool<byte>.Shared.Rent(messageLength);
            var payloadLength = session.Receive(payloadBuffer, 0, messageLength); // TODO: буферизация?

            if (payloadLength != messageLength)
            {
                Console.WriteLine("Разный размер"); // TODO: log, catch
            }
            
            foreach (var sub in subs)
            {
                if (!_sessionManager.TryGet(sub.ClientId, out var subSession) ||
                    subSession == null)
                {
                    Console.WriteLine("Не получена сессия для подписчика!!!"); //TODO: Log
                    continue;
                }

                var result = subSession.SendAsync(payloadBuffer.MessageBytes(subject, sub.Sid, messageLength)); // TODO: не вижу сообщения
                Console.WriteLine($"Отправлено {sub.ClientId} {sub.Sid} {result}");
            }
            
            session.SendAsync(ResultHelper.Ok());
        }
        else
        {
            session.SendAsync("Invalid protocol message format.".Error());
            session.Disconnect();
        }
    }
    
    public bool IsValidPublishSubject(string subject)
    {
        return _subjectRegex.IsMatch(subject);
    }
}