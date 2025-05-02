using System;

namespace UnisonMQ.Client
{
    public interface IUnisonMqClient : IAsyncDisposable
    {
        void Ping();
        bool ConnectAsync();
        bool CloseAsync();
    }
}