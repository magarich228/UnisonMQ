using Microsoft.Extensions.DependencyInjection;
using UnisonMQ.Client;

namespace UnisonMQ.DependencyInjection;

public static class Extensions
{
    public static IServiceCollection AddUnisonMqClient(this IServiceCollection services)
    {
        var configuration = new UnisonMqConfiguration();
        
        return services.AddUnisonMqClient(configuration);
    }
    
    public static IServiceCollection AddUnisonMqClient(
        this IServiceCollection services,
        Action<UnisonMqConfiguration> options)
    {
        var configuration = new UnisonMqConfiguration();
        options(configuration);
        
        return services.AddUnisonMqClient(configuration);
    }
    
    public static IServiceCollection AddUnisonMqClient(
        this IServiceCollection services,
        UnisonMqConfiguration configuration)
    {
        services.AddSingleton(configuration);
        services.AddScoped<IUnisonMqClient, UnisonMqClientService>();
        
        return services;
    }
}