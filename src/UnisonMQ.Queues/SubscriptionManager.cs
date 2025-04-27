using System.Collections.Concurrent;
using UnisonMQ.Abstractions;

namespace UnisonMQ.Queues;

internal class SubscriptionManager
{
    private readonly ConcurrentDictionary<SubscriptionKey, Subscription> _subscriptions = new();
    
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
                _subscriptions.TryRemove(key, out _);
            }
            else
            {
                _subscriptions.TryUpdate(
                    key,
                    new Subscription(subscription.Subject, maxMessages.Value),
                    subscription);
            }
        }
        else
        {
            var keys = _subscriptions.Keys.Where(k => k.ClientId == clientId);

            foreach (var key in keys)
            {
                _subscriptions.Remove(key, out _);
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