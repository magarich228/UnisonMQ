using System;
using System.Threading.Tasks;

namespace UnisonMQ.Client;

public sealed class UnisonMqClientService : IUnisonMqClient
{
    private readonly IUnisonMqClient _clientImplementation;

    public UnisonMqClientService(string hostname, int port)
    {
        _clientImplementation = new UnisonMqClient(hostname, port);
    }

    public UnisonMqClientService(UnisonMqConfiguration configuration)
    {
        _clientImplementation = new UnisonMqClient(configuration);
    }

    public void Ping() => _clientImplementation.Ping();

    public bool ConnectAsync() => _clientImplementation.ConnectAsync();

    public bool CloseAsync() => _clientImplementation.ConnectAsync();

    public void Subscribe<T>(string subject, Action<UnisonMessage<T>> onReceived) =>
        _clientImplementation.Subscribe<T>(subject, onReceived);

    public void Publish(string subject, object data) => 
        _clientImplementation.Publish(subject, data);

    public async ValueTask DisposeAsync() =>
        await _clientImplementation.DisposeAsync();
}