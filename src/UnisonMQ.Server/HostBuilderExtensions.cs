using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UnisonMQ.Abstractions;
using UnisonMQ.Operations;
using UnisonMQ.Queues;

namespace UnisonMQ.Server;

public static class HostBuilderExtensions
{
    public static IHostBuilder AddUnisonMq(this IHostBuilder hostBuilder)
    {
        hostBuilder
            .ConfigureLogging(ConfigureLogging)
            .ConfigureServices(AddUnisonMq);
        
        return hostBuilder;
    }

    private static void ConfigureLogging(HostBuilderContext hostBuilderContext, ILoggingBuilder builder)
    {
        var configuration = hostBuilderContext.Configuration;
        
        builder.AddConfiguration(configuration);
    }
    
    private static void AddUnisonMq(HostBuilderContext hostBuilderContext, IServiceCollection services)
    {
        var configuration = new TcpServerConfiguration();
        hostBuilderContext.Configuration.Bind(configuration);
        
        services.AddSingleton(configuration);
        services.AddSingleton(CreateServer);

        services.AddQueues();
        services.AddSingleton<ISessionManager, SessionManager>();
        services.AddOperations();
        
        services.AddHostedService<UnisonMqHostedService>();
    }

    private static UnisonMqServer CreateServer(IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<TcpServerConfiguration>();
        var sessionManager = serviceProvider.GetRequiredService<ISessionManager>();
        var operationProcessor = serviceProvider.GetRequiredService<IOperationProcessor>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        
        var server = new UnisonMqServer(configuration, sessionManager, operationProcessor, loggerFactory);
        
        return server;
    }
}