using System;

namespace UnisonMQ.Client
{
    public interface IUnisonMqClient : IAsyncDisposable
    {
        Guid? ConnectionId { get; }
        void Ping();
        bool ConnectAsync();
        bool CloseAsync();
        void Subscribe<T>(string subject, Action<UnisonMessage<T>> onReceived);
        void Publish(string subject, object data);
        void ReceiveAsync();
    }
}