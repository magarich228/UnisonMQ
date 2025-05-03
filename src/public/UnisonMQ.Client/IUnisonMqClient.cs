using System;

namespace UnisonMQ.Client
{
    public interface IUnisonMqClient : IAsyncDisposable
    {
        void Ping();
        bool ConnectAsync();
        bool CloseAsync();
        void Subscribe<T>(string subject, Action<UnisonMessage<T>> onReceived);
        void Publish(string subject, object data);
    }
}