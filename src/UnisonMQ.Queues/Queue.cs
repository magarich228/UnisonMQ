// using System.Collections.Concurrent;
// using UnisonMQ.Abstractions;
//
// namespace UnisonMQ.Queues;
//
// internal class Queue
// {
//     private readonly ConcurrentDictionary<int, Subscribtion> _subscribers;
//     
//     public Queue(string name)
//     {
//         Name = name;
//
//         _subscribers = new();
//     }
//     
//     public string Name { get; }
//
//     public void Subscribe(int sid)
//     {
//         _subscribers.TryAdd(sid, new Subscribtion(sid));
//     }
//
//     public IEnumerable<int> GetSubscribersForSend()
//     {
//         foreach (var sid in _subscribers.Keys)
//         {
//             var subscriber = _subscribers[sid];
//             
//             if (subscriber.MessageBalance.HasValue)
//             {
//                 subscriber.MessageBalance -= 1;
//
//                 if (subscriber.MessageBalance == 0)
//                 {
//                     Unsubscribe(sid);
//                     
//                     continue;
//                 }
//             }
//             
//             yield return sid;
//         }
//     }
//
//     public void Unsubscribe(int sid, int? maxMessages = null)
//     {
//         if (!_subscribers.TryGetValue(sid, out var subscriber))
//         {
//             throw new UnisonMqException($"Subscription not found {sid} for queue {Name}.");
//         }
//
//         if (maxMessages == null)
//         {
//             _subscribers.TryRemove(sid, out _);
//         }
//         else
//         {
//             _subscribers.TryUpdate(sid, new Subscribtion(sid, maxMessages), subscriber);
//         }
//     }
// }