using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using UnisonMQ.Abstractions;

namespace UnisonMQ.Queues;

internal class SubscriptionManager
{
    private readonly ILogger<SubscriptionManager> _logger;
    private readonly ConcurrentDictionary<SubscriptionKey, Subscription> _subscriptions;

    public SubscriptionManager(ILogger<SubscriptionManager> logger)
    {
        _logger = logger;
        _subscriptions = new();
    }
    
    public void Subscribe(Guid clientId, int sid, string subject)
    {
        var key = new SubscriptionKey(clientId, sid);
        
        _subscriptions.AddOrUpdate(key,
            k => new Subscription(subject),
            (k, s) => new Subscription(subject));
    }

    public void Unsubscribe(Guid clientId, int? sid, int? maxMessages = null)
    {
        if (sid.HasValue)
        {
            var key = new SubscriptionKey(clientId, sid.Value);

            if (!_subscriptions.TryGetValue(key, out var subscription))
            {
                return;
            }

            if (!maxMessages.HasValue || maxMessages.Value == 0)
            {
                if (!_subscriptions.TryRemove(key, out _))
                {
                    _logger.LogError("Failed to remove subscription {ClientId} {Sid}.", key.ClientId, key.Sid);
                }
            }
            else if (!_subscriptions.TryUpdate(
                         key,
                         new Subscription(subscription.Subject, maxMessages.Value),
                         subscription))
            {
                _logger.LogError("Failed to update subscription {ClientId} {Sid}.", key.ClientId, key.Sid);
            }
        }
        else
        {
            var keys = _subscriptions.Keys.Where(k => k.ClientId == clientId);
            
            foreach (var key in keys)
            {
                if (!_subscriptions.Remove(key, out _))
                {
                    _logger.LogError("Failed to remove subscription {ClientId} {Sid}.", key.ClientId, key.Sid);
                }
            }
        }
    }

    public ClientSubscription[] GetSubscribersForSend(string queueName)
    {
        return _subscriptions
            .Where(s => s.Value.IsMatch(queueName) && 
                        !s.Value.MessageBalance.HasValue ||
                        s.Value.MessageBalance > 0)
            .Select(s =>
            {
                s.Value.DecrementBalance();

                if (s.Value.MessageBalance == 0)
                {
                    Unsubscribe(s.Key.ClientId, s.Key.Sid); // TODO: перенести, если будет сильно задерживать
                }
                
                return s.Key.ToClientSubscription(queueName);
            }).ToArray();
    }
}