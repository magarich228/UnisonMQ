using Microsoft.Extensions.DependencyInjection;
using UnisonMQ.Abstractions;

namespace UnisonMQ.Queues;

public static class QueuesExtensions
{
    public static IServiceCollection AddQueues(this IServiceCollection services)
    {
        services.AddSingleton<IQueueService, QueueService>();
        return services;
    }
}